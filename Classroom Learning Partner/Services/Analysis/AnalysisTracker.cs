using System;
using System.Collections.Generic;
using Catel.Data;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    [Serializable]
    public class AnalysisTracker : ASerializableBase
    {
        #region Nested

        [Serializable]
        public class PageProgress : ASerializableBase
        {
            #region Constructors

            #endregion // Constructors

            #region Properties

            public int PageNumber
            {
                get => GetValue<int>(PageNumberProperty);
                set => SetValue(PageNumberProperty, value);
            }

            public static readonly PropertyData PageNumberProperty = RegisterProperty("PageNumber", typeof(int), 0);

            public List<string> StudentIDs
            {
                get => GetValue<List<string>>(StudentIDsProperty);
                set => SetValue(StudentIDsProperty, value);
            }

            public static readonly PropertyData StudentIDsProperty = RegisterProperty("StudentIDs", typeof(List<string>), () => new List<string>());

            #endregion // Properties
        }

        #endregion // Nested

        #region Constructors

        #endregion // Constructors

        #region Properties

        public DateTime SaveTime
        {
            get => GetValue<DateTime>(SaveTimeProperty);
            set => SetValue(SaveTimeProperty, value);
        }

        public static readonly PropertyData SaveTimeProperty = RegisterProperty("SaveTime", typeof(DateTime), DateTime.Now);

        public List<Notebook> StudentNotebooks
        {
            get => GetValue<List<Notebook>>(StudentNotebooksProperty);
            set => SetValue(StudentNotebooksProperty, value);
        }

        public static readonly PropertyData StudentNotebooksProperty = RegisterProperty("StudentNotebooks", typeof(List<Notebook>), () => new List<Notebook>());

        public List<int> CompletedPageNumbers
        {
            get => GetValue<List<int>>(CompletedPageNumbersProperty);
            set => SetValue(CompletedPageNumbersProperty, value);
        }

        public static readonly PropertyData CompletedPageNumbersProperty = RegisterProperty("CompletedPageNumbers", typeof(List<int>), () => new List<int>());

        public List<PageProgress> InProgressPages
        {
            get => GetValue<List<PageProgress>>(InProgressPagesProperty);
            set => SetValue(InProgressPagesProperty, value);
        }

        public static readonly PropertyData InProgressPagesProperty = RegisterProperty("InProgressPages", typeof(List<PageProgress>), () => new List<PageProgress>());

        public double FullPageConversionAndAnalysisEntryGenerationTimeInMilliseconds
        {
            get => GetValue<double>(FullPageConversionAndAnalysisEntryGenerationTimeInMillisecondsProperty);
            set => SetValue(FullPageConversionAndAnalysisEntryGenerationTimeInMillisecondsProperty, value);
        }

        public static readonly PropertyData FullPageConversionAndAnalysisEntryGenerationTimeInMillisecondsProperty =
            RegisterProperty("FullPageConversionAndAnalysisEntryGenerationTimeInMilliseconds", typeof(double), 0.0);

        public double PageAnalysisTimeInMilliseconds
        {
            get => GetValue<double>(PageAnalysisTimeInMillisecondsProperty);
            set => SetValue(PageAnalysisTimeInMillisecondsProperty, value);
        }

        public static readonly PropertyData PageAnalysisTimeInMillisecondsProperty = RegisterProperty("PageAnalysisTimeInMilliseconds", typeof(double), 0.0);

        public int TotalPagesAnalyzed
        {
            get => GetValue<int>(TotalPagesAnalyzedProperty);
            set => SetValue(TotalPagesAnalyzedProperty, value);
        }

        public static readonly PropertyData TotalPagesAnalyzedProperty = RegisterProperty("TotalPagesAnalyzed", typeof(int), 0);

        public int TotalHistoryActionsAnalyzed
        {
            get => GetValue<int>(TotalHistoryActionsAnalyzedProperty);
            set => SetValue(TotalHistoryActionsAnalyzedProperty, value);
        }

        public static readonly PropertyData TotalHistoryActionsAnalyzedProperty = RegisterProperty("TotalHistoryActionsAnalyzed", typeof(int), 0);

        public string AnalysisTimeRemaining
        {
            get => GetValue<string>(AnalysisTimeRemainingProperty);
            set => SetValue(AnalysisTimeRemainingProperty, value);
        }

        public static readonly PropertyData AnalysisTimeRemainingProperty = RegisterProperty("AnalysisTimeRemaining", typeof(string), string.Empty);

        #region Calculated Properties

        public double AverageFullPageConversionAndAnalysisEntryGenerationTimeInMilliseconds
        {
            get
            {
                if (TotalPagesAnalyzed == 0)
                {
                    return 0;
                }

                return FullPageConversionAndAnalysisEntryGenerationTimeInMilliseconds / TotalPagesAnalyzed;
            }
        }

        public double AveragePageAnalysisTimeInMilliseconds
        {
            get
            {
                if (TotalPagesAnalyzed == 0)
                {
                    return 0;
                }

                return PageAnalysisTimeInMilliseconds / TotalPagesAnalyzed;
            }
        }

        public double AverageHistoryActionAnalysisTimeInMilliseconds
        {
            get
            {
                if (TotalHistoryActionsAnalyzed == 0)
                {
                    return 0.0;
                }

                return PageAnalysisTimeInMilliseconds / TotalHistoryActionsAnalyzed;
            }
        }

        #endregion // Calculated Properties

        #endregion // Properties
    }
}