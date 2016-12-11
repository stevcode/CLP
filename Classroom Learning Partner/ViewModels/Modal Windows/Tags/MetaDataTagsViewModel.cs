using System.Collections.Generic;
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

        /// <summary>Value for NAME_DIFFICULTY_LEVEL</summary>
        public DifficultyLevels DifficultyLevel
        {
            get { return GetValue<DifficultyLevels>(DifficultyLevelProperty); }
            set { SetValue(DifficultyLevelProperty, value); }
        }

        public static readonly PropertyData DifficultyLevelProperty = RegisterProperty("DifficultyLevel", typeof(DifficultyLevels), DifficultyLevels.None);

        /// <summary>Value for VALUE_SPECIAL_INTEREST_GROUP_CE</summary>
        public bool IsCommutativeEquivalence
        {
            get { return GetValue<bool>(IsCommutativeEquivalenceProperty); }
            set { SetValue(IsCommutativeEquivalenceProperty, value); }
        }

        public static readonly PropertyData IsCommutativeEquivalenceProperty = RegisterProperty("IsCommutativeEquivalence", typeof(bool), false);

        /// <summary>Value for VALUE_SPECIAL_INTEREST_GROUP_ZERO</summary>
        public bool IsMultiplicationWithZero
        {
            get { return GetValue<bool>(IsMultiplicationWithZeroProperty); }
            set { SetValue(IsMultiplicationWithZeroProperty, value); }
        }

        public static readonly PropertyData IsMultiplicationWithZeroProperty = RegisterProperty("IsMultiplicationWithZero", typeof(bool), false);

        /// <summary>Value for VALUE_SPECIAL_INTEREST_GROUP_SCAF</summary>
        public bool IsScaffolded
        {
            get { return GetValue<bool>(IsScaffoldedProperty); }
            set { SetValue(IsScaffoldedProperty, value); }
        }

        public static readonly PropertyData IsScaffoldedProperty = RegisterProperty("IsScaffolded", typeof(bool), false);

        /// <summary>Value for VALUE_SPECIAL_INTEREST_GROUP_2PSF</summary>
        public bool Is2PSF
        {
            get { return GetValue<bool>(Is2PSFProperty); }
            set { SetValue(Is2PSFProperty, value); }
        }

        public static readonly PropertyData Is2PSFProperty = RegisterProperty("Is2PSF", typeof(bool), false);

        /// <summary>Value for VALUE_SPECIAL_INTEREST_GROUP_2PSS</summary>
        public bool Is2PSS
        {
            get { return GetValue<bool>(Is2PSSProperty); }
            set { SetValue(Is2PSSProperty, value); }
        }

        public static readonly PropertyData Is2PSSProperty = RegisterProperty("Is2PSS", typeof(bool), false);

        /// <summary>Value for </summary>
        public bool IsArrayRequired
        {
            get { return GetValue<bool>(IsArrayRequiredProperty); }
            set { SetValue(IsArrayRequiredProperty, value); }
        }

        public static readonly PropertyData IsArrayRequiredProperty = RegisterProperty("IsArrayRequired", typeof(bool), false);

        /// <summary>Value for </summary>
        public bool IsStampRequired
        {
            get { return GetValue<bool>(IsStampRequiredProperty); }
            set { SetValue(IsStampRequiredProperty, value); }
        }

        public static readonly PropertyData IsStampRequiredProperty = RegisterProperty("IsStampRequired", typeof(bool), false);

        /// <summary>Value for </summary>
        public bool IsNumberLineRequired
        {
            get { return GetValue<bool>(IsNumberLineRequiredProperty); }
            set { SetValue(IsNumberLineRequiredProperty, value); }
        }

        public static readonly PropertyData IsNumberLineRequiredProperty = RegisterProperty("IsNumberLineRequired", typeof(bool), false);

        /// <summary>Value for </summary>
        public bool IsArrayOrNumberLineRequired
        {
            get { return GetValue<bool>(IsArrayOrNumberLineRequiredProperty); }
            set { SetValue(IsArrayOrNumberLineRequiredProperty, value); }
        }

        public static readonly PropertyData IsArrayOrNumberLineRequiredProperty = RegisterProperty("IsArrayOrNumberLineRequired", typeof(bool), false);

        /// <summary>Value for </summary>
        public bool IsArrayAndStampRequired
        {
            get { return GetValue<bool>(IsArrayAndStampRequiredProperty); }
            set { SetValue(IsArrayAndStampRequiredProperty, value); }
        }

        public static readonly PropertyData IsArrayAndStampRequiredProperty = RegisterProperty("IsArrayAndStampRequired", typeof(bool), false);

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
                    case MetaDataTag.NAME_SPECIAL_INTEREST_GROUPS:
                        IsCommutativeEquivalence = metaDataTag.TagContents.Contains(MetaDataTag.VALUE_SPECIAL_INTEREST_GROUP_CE);
                        IsMultiplicationWithZero = metaDataTag.TagContents.Contains(MetaDataTag.VALUE_SPECIAL_INTEREST_GROUP_ZERO);
                        IsScaffolded = metaDataTag.TagContents.Contains(MetaDataTag.VALUE_SPECIAL_INTEREST_GROUP_SCAF);
                        Is2PSF = metaDataTag.TagContents.Contains(MetaDataTag.VALUE_SPECIAL_INTEREST_GROUP_2PSF);
                        Is2PSS = metaDataTag.TagContents.Contains(MetaDataTag.VALUE_SPECIAL_INTEREST_GROUP_2PSS);
                        break;
                    case MetaDataTag.NAME_REQUIRED_REPRESENTATIONS:
                        IsArrayRequired = metaDataTag.TagContents == MetaDataTag.VALUE_REQUIRED_REPRESENTATIONS_ARRAY;
                        IsStampRequired = metaDataTag.TagContents == MetaDataTag.VALUE_REQUIRED_REPRESENTATIONS_STAMP;
                        IsNumberLineRequired = metaDataTag.TagContents == MetaDataTag.VALUE_REQUIRED_REPRESENTATIONS_NUMBER_LINE;
                        IsArrayOrNumberLineRequired = metaDataTag.TagContents == MetaDataTag.VALUE_REQUIRED_REPRESENTATIONS_ARRAY_OR_NUMBER_LINE;
                        IsArrayAndStampRequired = metaDataTag.TagContents == MetaDataTag.VALUE_REQUIRED_REPRESENTATIONS_ARRAY_AND_STAMP;
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

            Page.AddTag(new MetaDataTag(Page, Origin.Author, MetaDataTag.NAME_DIFFICULTY_LEVEL, DifficultyLevel.ToString()));

            var specialInterestGroups = new List<string>();
            if (IsCommutativeEquivalence)
            {
                specialInterestGroups.Add(MetaDataTag.VALUE_SPECIAL_INTEREST_GROUP_CE);
            }
            if (IsMultiplicationWithZero)
            {
                specialInterestGroups.Add(MetaDataTag.VALUE_SPECIAL_INTEREST_GROUP_ZERO);
            }
            if (IsScaffolded)
            {
                specialInterestGroups.Add(MetaDataTag.VALUE_SPECIAL_INTEREST_GROUP_SCAF);
            }
            if (Is2PSF)
            {
                specialInterestGroups.Add(MetaDataTag.VALUE_SPECIAL_INTEREST_GROUP_2PSF);
            }
            if (Is2PSS)
            {
                specialInterestGroups.Add(MetaDataTag.VALUE_SPECIAL_INTEREST_GROUP_2PSS);
            }
            if (!specialInterestGroups.Any())
            {
                specialInterestGroups.Add("None");
            }

            var specialInterestGroupContent = string.Join(", ", specialInterestGroups);
            Page.AddTag(new MetaDataTag(Page, Origin.Author, MetaDataTag.NAME_SPECIAL_INTEREST_GROUPS, specialInterestGroupContent));

            var requiredRepresentationsContent = "None";
            if (IsArrayRequired)
            {
                requiredRepresentationsContent = MetaDataTag.VALUE_REQUIRED_REPRESENTATIONS_ARRAY;
            }
            else if (IsStampRequired)
            {
                requiredRepresentationsContent = MetaDataTag.VALUE_REQUIRED_REPRESENTATIONS_STAMP;
            }
            else if (IsNumberLineRequired)
            {
                requiredRepresentationsContent = MetaDataTag.VALUE_REQUIRED_REPRESENTATIONS_NUMBER_LINE;
            }
            else if (IsArrayOrNumberLineRequired)
            {
                requiredRepresentationsContent = MetaDataTag.VALUE_REQUIRED_REPRESENTATIONS_ARRAY_OR_NUMBER_LINE;
            }
            else if (IsArrayAndStampRequired)
            {
                requiredRepresentationsContent = MetaDataTag.VALUE_REQUIRED_REPRESENTATIONS_ARRAY_AND_STAMP;
            }

            Page.AddTag(new MetaDataTag(Page, Origin.Author, MetaDataTag.NAME_REQUIRED_REPRESENTATIONS, requiredRepresentationsContent));
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