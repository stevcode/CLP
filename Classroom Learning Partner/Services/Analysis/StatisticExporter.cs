using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public static class StatisticExporter
    {
        private static readonly string TSV_FILE_NAME = "HumanTags.tsv";
        private static readonly string DESKTOP_FOLDER_PATH = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private static readonly string TSV_FILE_PATH = Path.Combine(DESKTOP_FOLDER_PATH, TSV_FILE_NAME);

        private static readonly string OUTPUT_FILE_NAME = "TagStatistics.tsv";
        private static readonly string OUTPUT_FILE_PATH = Path.Combine(DESKTOP_FOLDER_PATH, OUTPUT_FILE_NAME);

        private static readonly List<StatisticsHumanTagData> HumanTagData = new List<StatisticsHumanTagData>();

        public static void GenerateStatistics(List<Notebook> notebooks)
        {
            ReadTSVFile();

            var fileRows = new List<string>();

            var allPages = notebooks.SelectMany(n => n.Pages).OrderBy(p => p.PageNumber).ThenBy(p => p.Owner.DisplayName);
            foreach (var page in allPages)
            {
                var pageRow = CreatePageRow(page);
                fileRows.Add(pageRow);
            }

            if (File.Exists(OUTPUT_FILE_PATH))
            {
                File.Delete(OUTPUT_FILE_PATH);
            }

            File.WriteAllText(OUTPUT_FILE_PATH, string.Empty);

            var headerRow = CreateHeaderRow();
            File.AppendAllText(OUTPUT_FILE_PATH, headerRow);

            foreach (var row in fileRows)
            {
                File.AppendAllText(OUTPUT_FILE_PATH, Environment.NewLine + row);
            }
        }

        public static void ReadTSVFile()
        {
            using (var file = File.OpenText(TSV_FILE_PATH))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    var parts = line.Split('\t');
                    var humanTagData = new StatisticsHumanTagData((int)parts[0].ToInt(), parts[1], parts[2]);
                    HumanTagData.Add(humanTagData);
                }
            }
        }

        public static string CreateHeaderRow()
        {
            var rowCells = new List<string>
            {
                // Page Identification
                "Page Number",
                "Student Name",

                // CLP Tag Counts
                "# CLP MA",
                "# CLP MR",
                "# CLP MR2STEP",
                "# CLP SKIPPED",
                "# CLP ARR_SKIP",
                "# CLP REP_DEL",
                "# CLP REP_ORDER",
                "# CLP ARR_PARTIAL_PRODUCT",
                

                // Human Tag Counts
                "# Human MA",
                "# Human MR",
                "# Human MR2STEP",
                "# Human RS",
                "# Human ACR",
                "# Human FMI",
                "# Human DEC INK",
                

                // Direct Comparison (Grouped for readability)
                "# CLP Tags",
                "# Human Tags",
                "# Comparable CLP Tags",
                "# Comparable Human Tags",

                // Actual Tags
                "CLP Tags",
                "Human Tags",
            };

            return string.Join("\t", rowCells);
        }

        public static string CreatePageRow(CLPPage page)
        {
            var humanTagData = HumanTagData.First(td => td.PageNumber == page.PageNumber &&
                                                        td.StudentName == page.Owner.DisplayName);

            var humanCodes = humanTagData.HumanTags;

            var clpCodes = new List<IAnalysisCode>();
            foreach (var tagGroup in page.Tags.GroupBy(t => t.Category).OrderBy(g => g.Key))
            {
                foreach (var tag in tagGroup)
                {
                    if (tag is IAnalysis analysisTag)
                    {
                        var tagCodes = analysisTag.QueryCodes.ToList();
                        clpCodes.AddRange(tagCodes);
                    }
                }
            }

            var rowCells = new List<string>
            {
                // Page Identification
                page.PageNumber.ToString(),
                page.Owner.DisplayName
            };

            #region CLP Tag Counts

            var clpComparable = new List<IAnalysisCode>();
            var maCLPCount = 0;
            var mrCLPCount = 0;
            var mr2stepCLPCount = 0;
            var skippedCLPCount = 0;
            var arrSkipCLPCount = 0;
            var repDelCLPCount = 0;
            var repOrderCLPCount = 0;
            var arrPartProdCLPCount = 0;
            foreach (var code in clpCodes)
            {
                switch (code.AnalysisCodeLabel)
                {
                    case Codings.ANALYSIS_LABEL_MULTIPLE_APPROACHES:
                        maCLPCount++;
                        continue;
                    case Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_1_STEP:
                        mrCLPCount++;
                        continue;
                    case Codings.ANALYSIS_LABEL_MULTIPLE_REPRESENTATIONS_2_STEP:
                        mr2stepCLPCount++;
                        continue;
                    case Codings.ANALYSIS_LABEL_STRATEGY_ARRAY_SKIP:
                        skippedCLPCount++;
                        continue;
                    case Codings.ANALYSIS_LABEL_SKIP_CONSOLIDATION:
                        arrSkipCLPCount++;
                        continue;
                    case Codings.ANALYSIS_LABEL_REPRESENTATIONS_DELETED_SUMMARY:
                        repDelCLPCount++;
                        continue;
                    case Codings.ANALYSIS_LABEL_REPRESENTATION_ORDER:
                        repOrderCLPCount++;
                        continue;
                    case Codings.ANALYSIS_LABEL_STRATEGY_ARRAY_PARTIAL_PRODUCT:
                        arrPartProdCLPCount++;
                        continue;
                    case Codings.ANALYSIS_LABEL_REPRESENTATIONS_USED_SUMMARY:
                    case Codings.ANALYSIS_LABEL_WRONG_GROUPS:
                    case Codings.ANALYSIS_LABEL_FINAL_ANSWER_CORRECTNESS:
                    case Codings.ANALYSIS_LABEL_FINAL_REPRESENTATION_CORRECTNESS:
                    case Codings.ANALYSIS_LABEL_OVERALL_CORRECTNESS:
                    case Codings.ANALYSIS_LABEL_WORD_PROBLEM:
                    case Codings.ANALYSIS_LABEL_PAGE_DEFINITION:
                        continue;
                    default:
                        break;
                }

                clpComparable.Add(code);
            }
            
            rowCells.Add(maCLPCount.ToString());
            rowCells.Add(mrCLPCount.ToString());
            rowCells.Add(mr2stepCLPCount.ToString());
            rowCells.Add(skippedCLPCount.ToString());
            rowCells.Add(arrSkipCLPCount.ToString());
            rowCells.Add(repDelCLPCount.ToString());
            rowCells.Add(repOrderCLPCount.ToString());
            rowCells.Add(arrPartProdCLPCount.ToString());

            #endregion

            #region Human Tag Counts

            var humanComparable = new List<StatisticsHumanTagData.TagParts>();
            var maHumanCount = 0;
            var mrHumanCount = 0;
            var mr2stepHumanCount = 0;
            var rsHumanCount = 0;
            var acrHumanCount = 0;
            var fmiHumanCount = 0;
            var decInkHumanCount = 0;
            foreach (var code in humanCodes)
            {
                if (code.Tag == "MA")
                {
                    maHumanCount++;
                    continue;
                }
                if (code.Tag == "MR")
                {
                    maHumanCount++;
                    continue;
                }
                if (code.Tag == "MR2STEP")
                {
                    maHumanCount++;
                    continue;
                }
                if (code.Tag.StartsWith("RS"))
                {
                    maHumanCount++;
                    continue;
                }
                if (code.Tag.StartsWith("ACR"))
                {
                    maHumanCount++;
                    continue;
                }
                if (code.Tag.StartsWith("FMI"))
                {
                    maHumanCount++;
                    continue;
                }
                if (code.Tag.StartsWith("DEC INK"))
                {
                    maHumanCount++;
                    continue;
                }

                humanComparable.Add(code);
            }

            rowCells.Add(maHumanCount.ToString());
            rowCells.Add(mrHumanCount.ToString());
            rowCells.Add(mr2stepHumanCount.ToString());
            rowCells.Add(rsHumanCount.ToString());
            rowCells.Add(acrHumanCount.ToString());
            rowCells.Add(fmiHumanCount.ToString());
            rowCells.Add(decInkHumanCount.ToString());

            #endregion

            #region Direct Comparison

            var totalCLP = clpCodes.Count;
            rowCells.Add(totalCLP.ToString());

            var totalHuman = humanCodes.Count;
            rowCells.Add(totalHuman.ToString());

            rowCells.Add(clpComparable.Count.ToString());
            rowCells.Add(humanComparable.Count.ToString());

            #endregion

            #region Actual Tags

            var clpTags = "\"" + string.Join(Environment.NewLine, clpComparable.Select(c => c.FormattedValue)).Replace('"', '\'') + "\"";
            rowCells.Add(clpTags);

            var humanTags = "\"" + string.Join(Environment.NewLine, humanComparable.Select(c => c.FormattedValue)).Replace('"', '\'') + "\"";
            rowCells.Add(humanTags);

            #endregion

            return string.Join("\t", rowCells);
        }
    }
}
