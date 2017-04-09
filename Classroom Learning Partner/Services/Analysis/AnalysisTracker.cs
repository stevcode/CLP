using System;
using System.Collections.Generic;
using Catel.Data;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    [Serializable]
    public class AnalysisTracker : AEntityBase
    {
        #region Nested

        [Serializable]
        public class PageProgress : AEntityBase
        {
            #region Constructors

            public PageProgress() { }

            #endregion // Constructors

            #region Properties

            public int PageNumber
            {
                get { return GetValue<int>(PageNumberProperty); }
                set { SetValue(PageNumberProperty, value); }
            }

            public static readonly PropertyData PageNumberProperty = RegisterProperty("PageNumber", typeof(int), 0);

            public List<string> StudentIDs
            {
                get { return GetValue<List<string>>(StudentIDsProperty); }
                set { SetValue(StudentIDsProperty, value); }
            }

            public static readonly PropertyData StudentIDsProperty = RegisterProperty("StudentIDs", typeof(List<string>), () => new List<string>());

            #endregion // Properties
        }

        #endregion // Nested

        #region Constructors

        public AnalysisTracker() { }

        #endregion // Constructors

        #region Properties

        public DateTime SaveTime
        {
            get { return GetValue<DateTime>(SaveTimeProperty); }
            set { SetValue(SaveTimeProperty, value); }
        }

        public static readonly PropertyData SaveTimeProperty = RegisterProperty("SaveTime", typeof(DateTime), DateTime.Now);

        public List<Notebook> StudentNotebooks
        {
            get { return GetValue<List<Notebook>>(StudentNotebooksProperty); }
            set { SetValue(StudentNotebooksProperty, value); }
        }

        public static readonly PropertyData StudentNotebooksProperty = RegisterProperty("StudentNotebooks", typeof(List<Notebook>), () => new List<Notebook>());

        public List<int> CompletedPageNumbers
        {
            get { return GetValue<List<int>>(CompletedPageNumbersProperty); }
            set { SetValue(CompletedPageNumbersProperty, value); }
        }

        public static readonly PropertyData CompletedPageNumbersProperty = RegisterProperty("CompletedPageNumbers", typeof(List<int>), () => new List<int>());

        public List<PageProgress> InProgressPages
        {
            get { return GetValue<List<PageProgress>>(InProgressPagesProperty); }
            set { SetValue(InProgressPagesProperty, value); }
        }

        public static readonly PropertyData InProgressPagesProperty = RegisterProperty("InProgressPages", typeof(List<PageProgress>), () => new List<PageProgress>());

        public double AverageFullPageConversionAndAnalysisEntryGenerationTimeInMilliseconds
        {
            get { return GetValue<double>(AverageFullPageConversionAndAnalysisEntryGenerationTimeInMillisecondsProperty); }
            set { SetValue(AverageFullPageConversionAndAnalysisEntryGenerationTimeInMillisecondsProperty, value); }
        }

        public static readonly PropertyData AverageFullPageConversionAndAnalysisEntryGenerationTimeInMillisecondsProperty =
            RegisterProperty("AverageFullPageConversionAndAnalysisEntryGenerationTimeInMilliseconds", typeof(double), 0.0);

        public double AveragePageAnalysisTimeInMilliseconds
        {
            get { return GetValue<double>(AveragePageAnalysisTimeInMillisecondsProperty); }
            set { SetValue(AveragePageAnalysisTimeInMillisecondsProperty, value); }
        }

        public static readonly PropertyData AveragePageAnalysisTimeInMillisecondsProperty = RegisterProperty("AveragePageAnalysisTimeInMilliseconds", typeof(double), 0.0);

        public double AverageHistoryActionAnalysisTimeInMilliseconds
        {
            get { return GetValue<double>(AverageHistoryActionAnalysisTimeInMillisecondsProperty); }
            set { SetValue(AverageHistoryActionAnalysisTimeInMillisecondsProperty, value); }
        }

        public static readonly PropertyData AverageHistoryActionAnalysisTimeInMillisecondsProperty = RegisterProperty("AverageHistoryActionAnalysisTimeInMilliseconds", typeof(double), 0.0);

        public string AnalysisTimeRemaining
        {
            get { return GetValue<string>(AnalysisTimeRemainingProperty); }
            set { SetValue(AnalysisTimeRemainingProperty, value); }
        }

        public static readonly PropertyData AnalysisTimeRemainingProperty = RegisterProperty("AnalysisTimeRemaining", typeof(string), string.Empty);

        #endregion // Properties
    }
}