using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public class AnalysisTracker : AEntityBase
    {
        #region Constructors

        public AnalysisTracker() { }

        #endregion // Constructors

        #region Properties

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

        // <Page Number, Student IDs>
        public Dictionary<int, List<string>> InProgressPages
        {
            get { return GetValue<Dictionary<int, List<string>>>(InProgressPagesProperty); }
            set { SetValue(InProgressPagesProperty, value); }
        }

        public static readonly PropertyData InProgressPagesProperty = RegisterProperty("InProgressPages", typeof(Dictionary<int, List<string>>), () => new Dictionary<int, List<string>>());

        public double AverageFullPageConversionAndAnalysisEntryGenerationTimeInMilliseconds
        {
            get { return GetValue<double>(AverageFullPageConversionAndAnalysisEntryGenerationTimeInMillisecondsProperty); }
            set { SetValue(AverageFullPageConversionAndAnalysisEntryGenerationTimeInMillisecondsProperty, value); }
        }

        public static readonly PropertyData AverageFullPageConversionAndAnalysisEntryGenerationTimeInMillisecondsProperty = RegisterProperty("AverageFullPageConversionAndAnalysisEntryGenerationTimeInMilliseconds", typeof(double), 0.0);

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

        #endregion // Properties
    }
}
