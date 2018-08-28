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
                    case "V4_EUCLIDEAN_3":
                        CalculatePosition();
                        otherPage.CalculatePosition();
                        var C = new List<double>
                                {
                                    StudentActionDistance,
                                    AnalysisDistance,
                                    ProblemStructureDistance
                                };
                        var D = new List<double>
                                {
                                    otherPage.StudentActionDistance,
                                    otherPage.AnalysisDistance,
                                    otherPage.ProblemStructureDistance
                                };
                        distance = stevcode.ML.Distance.EuclideanDistance(C, D);
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

                File.AppendAllText(filePath, "CLUSTERING_EPSILON;1.7\n");

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
            var zeroWeightLabels = new List<string>
                                   {
                                       Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED,
                                       Codings.ANALYSIS_LABEL_OVERALL_CORRECTNESS,
                                       Codings.ANALYSIS_LABEL_STRATEGY_ARRAY_SKIP,
                                       Codings.ANALYSIS_LABEL_NUMBER_LINE_JUMP_ERASURES
                                   };

            var allAnalysisCodes = AnalysisCode.GenerateAvailableQueryConditions();
            foreach (var code in allAnalysisCodes)
            {
                var zeroWeightConstraintLabels = GetZeroWeightConstraintLabels(code.AnalysisCodeLabel);

                var shortName = Codings.AnalysisLabelToShortName(code.AnalysisCodeLabel);

                File.AppendAllText(filePath, zeroWeightLabels.Contains(code.AnalysisCodeLabel) ? $"{shortName};0.0\n" : $"{shortName};1.0\n");

                foreach (var constraint in code.Constraints)
                {
                    var constraintIndex = 1.0;
                    var possibleValues = AnalysisConstraint.GeneratePossibleConstraintValues(constraint.ConstraintLabel);
                    foreach (var possibleValue in possibleValues.Where(v => v != Codings.CONSTRAINT_VALUE_ANY))
                    {
                        var constraintFriendlyName = Codings.ConstraintLabelToShortName(constraint.ConstraintLabel);
                        if (zeroWeightLabels.Contains(code.AnalysisCodeLabel) ||
                            zeroWeightConstraintLabels.Contains(constraint.ConstraintLabel))
                        {
                            constraintIndex = 0.0;
                        }
                        File.AppendAllText(filePath, $"{shortName}-{constraintFriendlyName}-{possibleValue};{constraintIndex}\n");
                        constraintIndex++;
                    }
                }
            }
        }

        private static double GetAxisWeight(string axis)
        {
            const string FILE_EXTENSION = "txt";
            var fileName = $"CLP Constraint Cluster Settings.{FILE_EXTENSION}";
            var filePath = Path.Combine(DataService.DesktopFolderPath, fileName);

            var axisWeight = string.Empty;
            switch (axis)
            {
                case PROBLEM_STRUCTURE_DISTANCE:
                    axisWeight = File.ReadLines(filePath).FirstOrDefault(l => l.Contains("PROBLEM_STRUCTURE_WEIGHT"));
                    break;
                case STUDENT_ACTION_DISTANCE:
                    axisWeight = File.ReadLines(filePath).FirstOrDefault(l => l.Contains("STUDENT_ACTION"));
                    break;
                case ANALYSIS_DISTANCE:
                    axisWeight = File.ReadLines(filePath).FirstOrDefault(l => l.Contains("ANALYSIS_WEIGHT"));
                    break;
            }
            if (axisWeight is null)
            {
                return 1.0;
            }

            var weightParts = axisWeight.Trim().Split(";");
            var weight = weightParts[1].ToDouble();
            if (weight is null)
            {
                return 1.0;
            }

            return (double)weight;
        }

        private static List<string> GetZeroWeightConstraintLabels(string analysisLabel)
        {
            var zeroWeightLabels = new List<string>();
            switch (analysisLabel)
            {
                case Codings.ANALYSIS_LABEL_FINAL_ANSWER_CORRECTNESS:
                    zeroWeightLabels.Add(Codings.CONSTRAINT_ANSWER_MODIFICATION);
                    break;
                case Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED_SUMMARY:
                    zeroWeightLabels.Add(Codings.CONSTRAINT_REPRESENTATION_COUNT);
                    zeroWeightLabels.Add(Codings.CONSTRAINT_REPRESENTATION_DELETED_COUNT);
                    break;
                case Codings.ANALYSIS_LABEL_REPRESENTATIONS_DELETED_SUMMARY:
                    zeroWeightLabels.Add(Codings.CONSTRAINT_REPRESENTATION_OVERALL_CORRECTNESS);
                    break;
                case Codings.ANALYSIS_LABEL_MULTIPLE_APPROACHES:
                    zeroWeightLabels.Add(Codings.CONSTRAINT_MULTIPLE_REPRESENTATION_MATCHED_STEP);
                    break;
            }

            return zeroWeightLabels;
        }

        private const string PROBLEM_STRUCTURE_DISTANCE = "PS";
        private const string ANALYSIS_DISTANCE = "A";
        private const string STUDENT_ACTION_DISTANCE = "SA";

        private static string GetDistanceAxisForAnalysisCode(string analysisCodeLabel)
        {
            var problemStructure = new List<string>
                                   {
                                       Codings.ANALYSIS_LABEL_WORD_PROBLEM,
                                       Codings.ANALYSIS_LABEL_PAGE_DEFINITION
                                   };
            var studentActions = new List<string>
                                 {
                                     Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED,
                                     Codings.ANALYSIS_LABEL_STRATEGY_ARRAY_SKIP,
                                     Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED_SUMMARY,
                                     Codings.ANALYSIS_LABEL_REPRESENTATIONS_DELETED_SUMMARY,
                                     Codings.ANALYSIS_LABEL_SKIP_CONSOLIDATION,
                                     Codings.ANALYSIS_LABEL_MULTIPLE_APPROACHES,
                                     Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP,
                                     Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_2_STEP,
                                     Codings.ANALYSIS_LABEL_ARRAY_EQUATION,
                                     Codings.ANALYSIS_LABEL_REPRESENTATION_ORDER
                                 };

            if (problemStructure.Contains(analysisCodeLabel))
            {
                return PROBLEM_STRUCTURE_DISTANCE;
            }

            if (studentActions.Contains(analysisCodeLabel))
            {
                return STUDENT_ACTION_DISTANCE;
            }

            return ANALYSIS_DISTANCE;
        }

        private static string GetDistanceAxisForAnalysisConstraint(string analysisCodeLabel, string constraintLabel)
        {
            if (analysisCodeLabel == Codings.ANALYSIS_LABEL_FINAL_ANSWER_CORRECTNESS && 
                constraintLabel == Codings.CONSTRAINT_ANSWER_OBJECT)
            {
                return PROBLEM_STRUCTURE_DISTANCE;
            }

            if (analysisCodeLabel == Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED_SUMMARY &&
                constraintLabel == Codings.CONSTRAINT_REPRESENTATION_OVERALL_CORRECTNESS)
            {
                return ANALYSIS_DISTANCE;
            }

            if (analysisCodeLabel == Codings.ANALYSIS_LABEL_REPRESENTATIONS_DELETED_SUMMARY &&
                constraintLabel == Codings.CONSTRAINT_REPRESENTATION_OVERALL_CORRECTNESS)
            {
                return ANALYSIS_DISTANCE;
            }

            return GetDistanceAxisForAnalysisCode(analysisCodeLabel);
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
                return 0.0;
            }

            var weightParts = weightLine.Trim().Split(";");
            var weight = weightParts[1].ToDouble();
            if (weight is null)
            {
                return 0.0;
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
                return 0.0;
            }

            var weightParts = weightLine.Trim().Split(";");
            var weight = weightParts[1].ToDouble();
            if (weight is null)
            {
                return 0.0;
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
                return 0.0;
            }

            var weightParts = weightLine.Trim().Split(";");
            var weight = weightParts[1].ToDouble();
            if (weight is null)
            {
                return 0.0;
            }

            return (double)weight;
        }

        public static double GetClusteringEpsilon()
        {
            const string FILE_EXTENSION = "txt";
            var fileName = $"CLP Constraint Cluster Settings.{FILE_EXTENSION}";
            var filePath = Path.Combine(DataService.DesktopFolderPath, fileName);

            var epsilonLine = File.ReadLines(filePath).FirstOrDefault(l => l.Contains("CLUSTERING_EPSILON"));
            if (epsilonLine is null)
            {
                return 1.7;
            }

            var weightParts = epsilonLine.Trim().Split(";");
            var weight = weightParts[1].ToDouble();
            if (weight is null)
            {
                return 1.7;
            }

            return (double)weight;
        }

        #endregion // Distance New

        #region Better Distance

        public void CalculatePosition()
        {
            if (IsPositionCached)
            {
                return;
            }

            var problemStructure = new List<Tuple<string,double>>();
            var studentActions = new List<Tuple<string, double>>();
            var analysis = new List<Tuple<string, double>>();

            foreach (var code in AllAnalysisCodes)
            {
                var labelWeight = GetLabelWeight(code.AnalysisCodeLabel);
                var axis = GetDistanceAxisForAnalysisCode(code.AnalysisCodeLabel);

                switch (axis)
                {
                    case PROBLEM_STRUCTURE_DISTANCE:
                        problemStructure.Add(new Tuple<string, double>(code.AnalysisCodeShortName, labelWeight));
                        break;
                    case STUDENT_ACTION_DISTANCE:
                        studentActions.Add(new Tuple<string, double>(code.AnalysisCodeShortName,labelWeight));
                        break;
                    case ANALYSIS_DISTANCE:
                        analysis.Add(new Tuple<string, double>(code.AnalysisCodeShortName, labelWeight));
                        break;
                }
                
                foreach (var constraint in code.Constraints.Where(c => c.IsQueryable))
                {
                    axis = GetDistanceAxisForAnalysisConstraint(code.AnalysisCodeLabel, constraint.ConstraintLabel);
                    var constraintFriendlyName = Codings.ConstraintLabelToShortName(constraint.ConstraintLabel);
                    var constraintWeight = GetConstraintValueWeight(code.AnalysisCodeLabel, constraint.ConstraintLabel, constraint.ConstraintValue);
                    switch (axis)
                    {
                        case PROBLEM_STRUCTURE_DISTANCE:
                            problemStructure.Add(new Tuple<string, double>($"{code.AnalysisCodeShortName}-{constraintFriendlyName}-{constraint.ConstraintValue}", constraintWeight));
                            break;
                        case STUDENT_ACTION_DISTANCE:
                            studentActions.Add(new Tuple<string, double>($"{code.AnalysisCodeShortName}-{constraintFriendlyName}-{constraint.ConstraintValue}", constraintWeight));
                            break;
                        case ANALYSIS_DISTANCE:
                            analysis.Add(new Tuple<string, double>($"{code.AnalysisCodeShortName}-{constraintFriendlyName}-{constraint.ConstraintValue}", constraintWeight));
                            break;
                    }
                }
            }

            StudentActionDistance = studentActions.Select(t => t.Item2).Sum() * GetAxisWeight(STUDENT_ACTION_DISTANCE);
            var studentActionsFormat = $"Student Actions Distance ({StudentActionDistance} * {GetAxisWeight(STUDENT_ACTION_DISTANCE)}):{string.Join("", studentActions.Where(t => t.Item2 != 0.0).Select(t => $"\n  - {t.Item1}: {t.Item2}"))}";

            AnalysisDistance = analysis.Select(t => t.Item2).Sum() * GetAxisWeight(ANALYSIS_DISTANCE);
            var analysisFormat = $"Analysis Distance ({AnalysisDistance} * {GetAxisWeight(ANALYSIS_DISTANCE)}):{string.Join("", analysis.Where(t => t.Item2 != 0.0).Select(t => $"\n  - {t.Item1}: {t.Item2}"))}";

            ProblemStructureDistance = problemStructure.Select(t => t.Item2).Sum() * GetAxisWeight(PROBLEM_STRUCTURE_DISTANCE);
            var problemStructureFormat = $"Problem Structure Distance ({ProblemStructureDistance} * {GetAxisWeight(PROBLEM_STRUCTURE_DISTANCE)}):{string.Join("", problemStructure.Where(t => t.Item2 != 0.0).Select(t => $"\n  - {t.Item1}: {t.Item2}"))}";

            FormattedDistance = $"{studentActionsFormat}\n\n{analysisFormat}\n\n{problemStructureFormat}";

            IsPositionCached = true;
        }

        #endregion // Better Distance
    }
}
