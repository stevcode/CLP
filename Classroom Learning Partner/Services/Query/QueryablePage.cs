using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CLP.Entities;
using stevcode.ML;

namespace Classroom_Learning_Partner.Services
{
    public class QueryablePage
    {
        public QueryablePage()
        {
            MatchingAnalysisCodes = new List<IAnalysisCode>();
            AllAnalysisCodes = new List<IAnalysisCode>();
        }

        public string CacheFilePath { get; set; }
        public string StudentID { get; set; }
        public string StudentName { get; set; }
        public CLPPage.NameComposite PageNameComposite { get; set; }
        public List<IAnalysisCode> MatchingAnalysisCodes { get; set; }
        public List<IAnalysisCode> AllAnalysisCodes { get; set; }

        public double StudentActionDistance { get; set; }
        public double AnalysisDistance { get; set; }
        public double ProblemStructureDistance { get; set; }
        public bool IsPositionCached { get; set; }

        public string FormattedValue
        {
            get { return $"Page {PageNameComposite.PageNumber}, {StudentName}\n - {string.Join("\n - ", AllAnalysisCodes.Select(q => q.FormattedValue))}"; }
        }

        public string FormattedDistance { get; set; }

        #region Methods

        public double Distance(QueryablePage otherPage)
        {
            var distanceMetric = GetDistanceMetric().ToUpper();

            var distance = 0.0;

            switch (distanceMetric)
            {
                    case "ANY":
                        distance = PageHammingDistance(this, otherPage, false);
                        break;
                    case "ALL":
                        distance = PageHammingDistance(this, otherPage, true);
                        break;
                    case "V2":
                        distance += ConstraintSimilarityDistance(this, otherPage, Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED, Codings.CONSTRAINT_REPRESENTATION_NAME_LAX);
                        distance += ConstraintSimilarityDistance(this, otherPage, Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED, Codings.CONSTRAINT_HISTORY_STATUS);
                        distance += ConstraintSimilarityDistance(this, otherPage, Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED, Codings.CONSTRAINT_REPRESENTATION_CORRECTNESS);
                        break;
                    case "V1": // Dist v 1, Label Count
                        var analysisCodeTypes = AllAnalysisCodes.Select(c => c.AnalysisCodeLabel).Distinct().ToList();
                        var analysisCodeTypesOther = otherPage.AllAnalysisCodes.Select(c => c.AnalysisCodeLabel).Distinct().ToList();
                        distance += Math.Abs(analysisCodeTypes.Count - analysisCodeTypesOther.Count);
                        break;
                    case "V4_MANHATTAN_3":
                        CalculatePosition();
                        otherPage.CalculatePosition();
                        var a = new List<double>
                                {
                                    StudentActionDistance,
                                    AnalysisDistance,
                                    ProblemStructureDistance
                                };
                        var b = new List<double>
                                {
                                    otherPage.StudentActionDistance,
                                    otherPage.AnalysisDistance,
                                    otherPage.ProblemStructureDistance
                                };
                        distance = stevcode.ML.Distance.ManhattanDistance(a, b);
                        break;
            }

            return distance;
        }

        private static double CodeHammingDistance(IAnalysisCode code1, IAnalysisCode code2)
        {
            if (code1.AnalysisCodeLabel != code2.AnalysisCodeLabel)
            {
                return Math.Max(code1.Constraints.Count(c => c.IsQueryable), code2.Constraints.Count(c => c.IsQueryable));
            }

            if (code1.Constraints.Count != code2.Constraints.Count)
            {
                // Print error?
                return Math.Max(code1.Constraints.Count(c => c.IsQueryable), code2.Constraints.Count(c => c.IsQueryable));
            }

            var distance = 0.0;
            for (var i = 0; i < code1.Constraints.Count; i++)
            {
                var constraint1 = code1.Constraints[i];
                var constraint2 = code2.Constraints[i];

                if (!constraint1.IsQueryable)
                {
                    continue;
                }
                
                if (constraint1.ConstraintValue != constraint2.ConstraintValue)
                {
                    var constraintWeight = GetConstraintWeight(code1.AnalysisCodeLabel, constraint1.ConstraintLabel);
                    distance += 1.0 * constraintWeight;
                }
            }

            return distance;
        }

        private static double ConstraintSimilarityDistance(QueryablePage page1, QueryablePage page2, string analysisCodeLabel, string constraintLabel)
        {
            var codes = page1.AllAnalysisCodes.Where(c => c.AnalysisCodeLabel == analysisCodeLabel).ToList();
            var codesOther = page2.AllAnalysisCodes.Where(c => c.AnalysisCodeLabel == analysisCodeLabel).ToList();

            var groups = codes.Select(c => c.Constraints.First(con => con.ConstraintLabel == constraintLabel).ConstraintValue).GroupBy(t => t);
            var groupsOther = codesOther.Select(c => c.Constraints.First(con => con.ConstraintLabel == constraintLabel).ConstraintValue).GroupBy(t => t);

            var groupings = groups.ToDictionary(@group => @group.Key, @group => @group.Count());
            var groupingsOther = groupsOther.ToDictionary(@group => @group.Key, @group => @group.Count());

            var distance = 0.0;
            foreach (var groupingsKey in groupings.Keys)
            {
                if (groupingsOther.ContainsKey(groupingsKey))
                {
                    distance += Math.Abs(groupings[groupingsKey] - groupingsOther[groupingsKey]);
                }
                else
                {
                    distance += groupings[groupingsKey];
                }
            }

            return distance;
        }

        #endregion // Methods

        #region Distance New

        private static double PageHammingDistance(QueryablePage page, QueryablePage otherPage, bool isAll)
        {
            var analysisCodeLabels = page.AllAnalysisCodes.Select(c => c.AnalysisCodeLabel).Concat(otherPage.AllAnalysisCodes.Select(c => c.AnalysisCodeLabel)).Distinct().ToList();

            var distance = 0.0;
            foreach (var analysisCodeLabel in analysisCodeLabels)
            {
                var labelWeight = GetLabelWeight(analysisCodeLabel);
                if (labelWeight == 0.0)
                {
                    continue;
                }

                var codes = page.AllAnalysisCodes.Where(c => c.AnalysisCodeLabel == analysisCodeLabel).ToList();
                var otherCodes = otherPage.AllAnalysisCodes.Where(c => c.AnalysisCodeLabel == analysisCodeLabel).ToList();

                var labelDistance = 0.0;
                foreach (var analysisCode in codes)
                {
                    if (!otherCodes.Any())
                    {
                        labelDistance += 1.0;
                        continue;
                    }

                    IAnalysisCode closestCode = null;
                    var smallestDistance = double.MaxValue;
                    foreach (var otherAnalysisCode in otherCodes)
                    {
                        var codeDistance = CodeHammingDistance(analysisCode, otherAnalysisCode);
                        if (codeDistance < smallestDistance)
                        {
                            smallestDistance = codeDistance;
                            closestCode = otherAnalysisCode;
                        }

                        if (smallestDistance == 0.0)
                        {
                            break;
                        }
                    }

                    var constraintWeight = 1.0 / analysisCode.Constraints.Count;
                    labelDistance += smallestDistance * constraintWeight;
                    otherCodes.Remove(closestCode);
                }

                if (isAll)
                {
                    labelDistance += otherCodes.Count;
                }
                else
                {
                    otherCodes = otherPage.AllAnalysisCodes.Where(c => c.AnalysisCodeLabel == analysisCodeLabel).ToList();
                    foreach (var otherAnalysisCode in otherCodes)
                    {
                        if (!codes.Any())
                        {
                            labelDistance += 1.0;
                            continue;
                        }

                        IAnalysisCode closestCode = null;
                        var smallestDistance = double.MaxValue;
                        foreach (var analysisCode in codes)
                        {
                            var codeDistance = CodeHammingDistance(otherAnalysisCode, analysisCode);
                            if (codeDistance < smallestDistance)
                            {
                                smallestDistance = codeDistance;
                                closestCode = otherAnalysisCode;
                            }

                            if (smallestDistance == 0.0)
                            {
                                break;
                            }
                        }

                        var constraintWeight = 1.0 / otherAnalysisCode.Constraints.Count;
                        labelDistance += smallestDistance * constraintWeight;
                        codes.Remove(closestCode);
                    }

                    labelDistance = labelDistance / 2.0;
                }

                labelDistance = labelDistance * labelWeight;
                distance += labelDistance;
            }

            return distance;
        }

        private static string GetDistanceMetric()
        {
            const string FILE_EXTENSION = "txt";
            var fileName = $"CLP Constraint Cluster Settings.{FILE_EXTENSION}";
            var filePath = Path.Combine(DataService.DesktopFolderPath, fileName);
            if (!File.Exists(filePath))
            {
                PopulateWeightsInSettingsFile_V4_MANHATTAN_3(filePath);

                File.AppendAllText(filePath, "CLUSTERING_EPSILON;0.33\n");

                File.AppendAllText(filePath, "V4_MANHATTAN_3");
            }

            return File.ReadLines(filePath).Last(l => !string.IsNullOrWhiteSpace(l)).Trim();
        }

        private static void PopulateWeightsInSettingsFile_V3(string filePath)
        {
            var allAnalysisCodes = AnalysisCode.GenerateAvailableQueryConditions();
            foreach (var code in allAnalysisCodes)
            {
                var shortName = Codings.AnalysisLabelToShortName(code.AnalysisCodeLabel);
                File.AppendAllText(filePath, $"{shortName};1.0\n");
                foreach (var constraint in code.Constraints)
                {
                    var constraintFriendlyName = Codings.ConstraintLabelToShortName(constraint.ConstraintLabel);
                    File.AppendAllText(filePath, $"{shortName}-{constraintFriendlyName};1.0\n");
                }
            }
        }

        private static void PopulateWeightsInSettingsFile_V4_MANHATTAN_3(string filePath)
        {
            var allAnalysisCodes = AnalysisCode.GenerateAvailableQueryConditions();
            foreach (var code in allAnalysisCodes)
            {
                var shortName = Codings.AnalysisLabelToShortName(code.AnalysisCodeLabel);
                File.AppendAllText(filePath, $"{shortName};1.0\n");
                foreach (var constraint in code.Constraints)
                {
                    var constraintIndex = 1.0;
                    var possibleValues = AnalysisConstraint.GeneratePossibleConstraintValues(constraint.ConstraintLabel);
                    foreach (var possibleValue in possibleValues.Where(v => v != Codings.CONSTRAINT_VALUE_ANY))
                    {
                        var constraintFriendlyName = Codings.ConstraintLabelToShortName(constraint.ConstraintLabel);
                        File.AppendAllText(filePath, $"{shortName}-{constraintFriendlyName}-{possibleValue};{constraintIndex}\n");
                        constraintIndex++;
                    }
                }
            }
        }

        private static double GetLabelWeight(string analysisCodeLabel)
        {
            const string FILE_EXTENSION = "txt";
            var fileName = $"CLP Constraint Cluster Settings.{FILE_EXTENSION}";
            var filePath = Path.Combine(DataService.DesktopFolderPath, fileName);

            var analysisCodeShortName = Codings.AnalysisLabelToShortName(analysisCodeLabel);
            var weightLine = File.ReadLines(filePath).FirstOrDefault(l => l.Contains($"{analysisCodeShortName};"));
            if (weightLine is null)
            {
                return 1.0;
            }

            var weightParts = weightLine.Trim().Split(";");
            var weight = weightParts[1].ToDouble();
            if (weight is null)
            {
                return 1.0;
            }

            return (double)weight;
        }

        private static double GetConstraintWeight(string analysisCodeLabel, string constraintLabel)
        {
            const string FILE_EXTENSION = "txt";
            var fileName = $"CLP Constraint Cluster Settings.{FILE_EXTENSION}";
            var filePath = Path.Combine(DataService.DesktopFolderPath, fileName);

            var analysisCodeShortName = Codings.AnalysisLabelToShortName(analysisCodeLabel);
            var constraintShortName = Codings.ConstraintLabelToShortName(constraintLabel);
            var weightLine = File.ReadLines(filePath).FirstOrDefault(l => l.Contains($"{analysisCodeShortName}-{constraintShortName};"));
            if (weightLine is null)
            {
                return 1.0;
            }

            var weightParts = weightLine.Trim().Split(";");
            var weight = weightParts[1].ToDouble();
            if (weight is null)
            {
                return 1.0;
            }

            return (double)weight;
        }

        private static double GetConstraintValueWeight(string analysisCodeLabel, string constraintLabel, string constraintValue)
        {
            const string FILE_EXTENSION = "txt";
            var fileName = $"CLP Constraint Cluster Settings.{FILE_EXTENSION}";
            var filePath = Path.Combine(DataService.DesktopFolderPath, fileName);

            var analysisCodeShortName = Codings.AnalysisLabelToShortName(analysisCodeLabel);
            var constraintShortName = Codings.ConstraintLabelToShortName(constraintLabel);
            var weightLine = File.ReadLines(filePath).FirstOrDefault(l => l.Contains($"{analysisCodeShortName}-{constraintShortName}-{constraintValue};"));
            if (weightLine is null)
            {
                return 1.0;
            }

            var weightParts = weightLine.Trim().Split(";");
            var weight = weightParts[1].ToDouble();
            if (weight is null)
            {
                return 1.0;
            }

            return (double)weight;
        }

        public static double GetClusteringEpsilon()
        {
            return GetLabelWeight("CLUSTERING_EPSILON");
        }

        #endregion // Distance New

        #region Better Distance

        public void CalculatePosition()
        {
            var problemStructure = new List<Tuple<string,double>>();
            var studentActions = new List<Tuple<string, double>>();
            var analysis = new List<Tuple<string, double>>();

            foreach (var code in AllAnalysisCodes)
            {
                var labelWeight = GetLabelWeight(code.AnalysisCodeLabel);

                switch (code.AnalysisCodeLabel)
                {
                    case Codings.ANALYSIS_LABEL_WORD_PROBLEM:
                    case Codings.ANALYSIS_LABEL_PAGE_DEFINITION:
                        problemStructure.Add(new Tuple<string, double>(code.AnalysisCodeShortName, labelWeight));
                        break;
                    case Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED:
                    case Codings.ANALYSIS_LABEL_FINAL_ANSWER_CORRECTNESS:
                    case Codings.ANALYSIS_LABEL_FINAL_REPRESENTATION_CORRECTNESS:
                    case Codings.ANALYSIS_LABEL_OVERALL_CORRECTNESS:
                        studentActions.Add(new Tuple<string, double>(code.AnalysisCodeShortName,labelWeight));
                        break;
                    default:
                        analysis.Add(new Tuple<string, double>(code.AnalysisCodeShortName, labelWeight));
                        break;
                }
                
                foreach (var constraint in code.Constraints.Where(c => c.IsQueryable))
                {
                    var constraintFriendlyName = Codings.ConstraintLabelToShortName(constraint.ConstraintLabel);
                    var constraintWeight = GetConstraintValueWeight(code.AnalysisCodeLabel, constraint.ConstraintLabel, constraint.ConstraintValue);
                    switch (code.AnalysisCodeLabel)
                    {
                        case Codings.ANALYSIS_LABEL_WORD_PROBLEM:
                        case Codings.ANALYSIS_LABEL_PAGE_DEFINITION:
                            problemStructure.Add(new Tuple<string, double>($"{code.AnalysisCodeShortName}-{constraintFriendlyName}-{constraint.ConstraintValue}", constraintWeight));
                            break;
                        case Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED:
                        case Codings.ANALYSIS_LABEL_FINAL_ANSWER_CORRECTNESS:
                        case Codings.ANALYSIS_LABEL_FINAL_REPRESENTATION_CORRECTNESS:
                        case Codings.ANALYSIS_LABEL_OVERALL_CORRECTNESS:
                            studentActions.Add(new Tuple<string, double>($"{code.AnalysisCodeShortName}-{constraintFriendlyName}-{constraint.ConstraintValue}", constraintWeight));
                            break;
                        default:
                            analysis.Add(new Tuple<string, double>($"{code.AnalysisCodeShortName}-{constraintFriendlyName}-{constraint.ConstraintValue}", constraintWeight));
                            break;
                    }
                }
            }

            StudentActionDistance = studentActions.Select(t => t.Item2).Sum();
            var studentActionsFormat = $"Student Actions Distance ({StudentActionDistance}):{string.Join("", studentActions.Select(t => $"\n\t- {t.Item1}: {t.Item2}"))}";

            AnalysisDistance = analysis.Select(t => t.Item2).Sum();
            var analysisFormat = $"Analysis Distance ({AnalysisDistance}):{string.Join("", analysis.Select(t => $"\n\t- {t.Item1}: {t.Item2}"))}";

            ProblemStructureDistance = problemStructure.Select(t => t.Item2).Sum();
            var problemStructureFormat = $"Problem Structure Distance ({ProblemStructureDistance}):{string.Join("", problemStructure.Select(t => $"\n\t- {t.Item1}: {t.Item2}"))}";

            FormattedDistance = $"{studentActionsFormat}\n\n{analysisFormat}\n\n{problemStructureFormat}";
        }

        #endregion // Better Distance
    }
}
