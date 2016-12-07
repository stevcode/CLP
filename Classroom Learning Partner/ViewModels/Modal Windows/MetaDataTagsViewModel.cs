using System.Linq;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class MetaDataTagsViewModel : ViewModelBase
    {
        public enum DifficultyLevels
        {
            None,
            Easy,
            Medium,
            Hard
        }

        #region Constructor

        public MetaDataTagsViewModel(CLPPage page)
        {
            Page = page;
            InitializeCommands();
            LoadMetaData();
        }

        #endregion // Constructor

        #region Model

        /// <summary>Model of this ViewModel.</summary>
        [Model(SupportIEditableObject = false)]
        public CLPPage Page
        {
            get { return GetValue<CLPPage>(PageProperty); }
            set { SetValue(PageProperty, value); }
        }

        public static readonly PropertyData PageProperty = RegisterProperty("Page", typeof(CLPPage));

        #endregion // Model

        #region Bindings

        /// <summary>Value for NAME_WORD_PROBLEM</summary>
        public bool IsWordProblem
        {
            get { return GetValue<bool>(IsWordProblemProperty); }
            set { SetValue(IsWordProblemProperty, value); }
        }

        public static readonly PropertyData IsWordProblemProperty = RegisterProperty("IsWordProblem", typeof(bool), false);

        /// <summary>Value for NAME_ONLY_TOP_PROBLEM</summary>
        public bool IsTopProblem
        {
            get { return GetValue<bool>(IsTopProblemProperty); }
            set { SetValue(IsTopProblemProperty, value); }
        }

        public static readonly PropertyData IsTopProblemProperty = RegisterProperty("IsTopProblem", typeof(bool), false);

        /// <summary>Value for NAME_SPECIAL_INTEREST_GROUP</summary>
        public bool IsSpecialInterestGroup
        {
            get { return GetValue<bool>(IsSpecialInterestGroupProperty); }
            set { SetValue(IsSpecialInterestGroupProperty, value); }
        }

        public static readonly PropertyData IsSpecialInterestGroupProperty = RegisterProperty("IsSpecialInterestGroup", typeof(bool), false);

        /// <summary>Value for NAME_DIFFICULTY_LEVEL</summary>
        public DifficultyLevels DifficultyLevel
        {
            get { return GetValue<DifficultyLevels>(DifficultyLevelProperty); }
            set { SetValue(DifficultyLevelProperty, value); }
        }

        public static readonly PropertyData DifficultyLevelProperty = RegisterProperty("DifficultyLevel", typeof(DifficultyLevels), DifficultyLevels.None);

        #endregion // Bindings

        #region Methods

        private void LoadMetaData()
        {
            var metaTags = Page.Tags.OfType<MetaDataTag>().ToList();
            foreach (var metaDataTag in metaTags)
            {
                switch (metaDataTag.TagName)
                {
                    case MetaDataTag.NAME_WORD_PROBLEM:
                        IsWordProblem = metaDataTag.TagContents == MetaDataTag.VALUE_TRUE;
                        break;
                    case MetaDataTag.NAME_ONLY_TOP_PROBLEM:
                        IsTopProblem = metaDataTag.TagContents == MetaDataTag.VALUE_TRUE;
                        break;
                    case MetaDataTag.NAME_SPECIAL_INTEREST_GROUP:
                        IsSpecialInterestGroup = metaDataTag.TagContents == MetaDataTag.VALUE_TRUE;
                        break;
                    case MetaDataTag.NAME_DIFFICULTY_LEVEL:
                        switch (metaDataTag.TagContents)
                        {
                            case MetaDataTag.VALUE_DIFFICULTY_NONE:
                                DifficultyLevel = DifficultyLevels.None;
                                break;
                            case MetaDataTag.VALUE_DIFFICULTY_EASY:
                                DifficultyLevel = DifficultyLevels.Easy;
                                break;
                            case MetaDataTag.VALUE_DIFFICULTY_MEDIUM:
                                DifficultyLevel = DifficultyLevels.Medium;
                                break;
                            case MetaDataTag.VALUE_DIFFICULTY_HARD:
                                DifficultyLevel = DifficultyLevels.Hard;
                                break;
                        }
                        break;
                }
            }
        }

        private void SaveMetaData()
        {
            var metaTags = Page.Tags.OfType<MetaDataTag>().ToList();
            foreach (var metaDataTag in metaTags)
            {
                Page.RemoveTag(metaDataTag);
            }

            var wordProblemContent = IsWordProblem ? MetaDataTag.VALUE_TRUE : MetaDataTag.VALUE_FALSE;
            Page.AddTag(new MetaDataTag(Page, Origin.Author, MetaDataTag.NAME_WORD_PROBLEM, wordProblemContent));

            var onlyTopProblemContent = IsTopProblem ? MetaDataTag.VALUE_TRUE : MetaDataTag.VALUE_FALSE;
            Page.AddTag(new MetaDataTag(Page, Origin.Author, MetaDataTag.NAME_ONLY_TOP_PROBLEM, onlyTopProblemContent));

            var specialInterestGroupContent = IsSpecialInterestGroup ? MetaDataTag.VALUE_TRUE : MetaDataTag.VALUE_FALSE;
            Page.AddTag(new MetaDataTag(Page, Origin.Author, MetaDataTag.NAME_SPECIAL_INTEREST_GROUP, specialInterestGroupContent));

            Page.AddTag(new MetaDataTag(Page, Origin.Author, MetaDataTag.NAME_DIFFICULTY_LEVEL, DifficultyLevel.ToString()));
        }

        #endregion // Methods

        #region Commands

        private void InitializeCommands()
        {
            ConfirmChangesCommand = new Command(OnConfirmChangesCommandExecute);
            CancelChangesCommand = new Command(OnCancelChangesCommandExecute);
        }

        /// <summary>Validates and confirms changes to the person.</summary>
        public Command ConfirmChangesCommand { get; private set; }

        private async void OnConfirmChangesCommandExecute()
        {
            SaveMetaData();
            await CloseViewModelAsync(true);
        }

        /// <summary>Cancels changes to the person.</summary>
        public Command CancelChangesCommand { get; private set; }

        private async void OnCancelChangesCommandExecute()
        {
            await CloseViewModelAsync(false);
        }

        #endregion // Commands
    }
}