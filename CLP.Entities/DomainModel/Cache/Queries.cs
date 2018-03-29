using System;
using System.Collections.ObjectModel;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class Queries : AInternalZipEntryFile
    {
        #region Constructor

        public Queries() { }

        #endregion // Constructor

        #region Properties

        public ObservableCollection<AnalysisCodeQuery> SavedQueries
        {
            get => GetValue<ObservableCollection<AnalysisCodeQuery>>(SavedQueriesProperty);
            set => SetValue(SavedQueriesProperty, value);
        }

        public static readonly PropertyData SavedQueriesProperty =
            RegisterProperty(nameof(SavedQueries), typeof(ObservableCollection<AnalysisCodeQuery>), () => new ObservableCollection<AnalysisCodeQuery>());

        #endregion // Properties

        #region Storage

        public const string DEFAULT_INTERNAL_FILE_NAME = "queries";

        #endregion // Storage

        #region Overrides of AInternalZipEntryFile

        public override string DefaultZipEntryName => DEFAULT_INTERNAL_FILE_NAME;

        public override string GetZipEntryFullPath(Notebook parentNotebook)
        {
            return $"{DefaultZipEntryName}.xml";
        }

        #endregion
    }
}
