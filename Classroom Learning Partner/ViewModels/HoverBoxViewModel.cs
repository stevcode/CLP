using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class HoverBoxViewModel : ViewModelBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the HoverBoxViewModel class.
        /// </summary>
        public HoverBoxViewModel(CLPPage page)
        {
            Page = page;
            IsCorrect = false;
            IsUnknown = true;
            IsIncorrect = false;
            IsStarred = false;
            Topics = "";

            foreach(ATagBase t in Page.Tags)
            {
                if (t is StarredTag && t.Value == "Starred") 
                {
                    IsStarred = true;
                }
                else if(t is CorrectnessTag)
                {
                    if(t.Value == "Correct")
                    {
                        IsCorrect = true;
                        IsUnknown = false;
                    }
                    else if(t.Value == "Incorrect")
                    {
                        IsIncorrect = true;
                        IsUnknown = false;
                    }
                }
            }
            MarkCorrectCommand = new Command<MouseEventArgs>(OnMarkCorrectCommandExecute);
            MarkIncorrectCommand = new Command<MouseEventArgs>(OnMarkIncorrectCommandExecute);
            MarkUnknownCommand = new Command<MouseEventArgs>(OnMarkUnknownCommandExecute);
            ToggleStarCommand = new Command<MouseEventArgs>(OnToggleStarCommandExecute);
            ShowTagsCommand = new Command<MouseEventArgs>(OnShowTagsCommandExecute);
        }

        public override string Title
        {
            get { return "HoverBoxVM"; }
        }

        #endregion //Constructors

        #region Properties

        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model]
        public CLPPage Page
        {
            get { return GetValue<CLPPage>(PageProperty); }
            private set { SetValue(PageProperty, value); }
        }

        public static readonly PropertyData PageProperty = RegisterProperty("Page", typeof(CLPPage));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public ObservableCollection<ITag> Tags
        {
            get { return GetValue<ObservableCollection<ITag>>(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        public static readonly PropertyData TagsProperty = RegisterProperty("Tags", typeof(ObservableCollection<ITag>));

        #endregion //Model

        public bool IsStarred
        {
            get { return GetValue<bool>(IsStarredProperty); }
            set { SetValue(IsStarredProperty, value); }
        }

        public static readonly PropertyData IsStarredProperty = RegisterProperty("IsStarred", typeof(bool), false);

        public string Topics
        {
            get { return GetValue<string>(TopicsProperty); }
            private set { SetValue(TopicsProperty, value); }
        }

        public static readonly PropertyData TopicsProperty = RegisterProperty("Topics", typeof(string), "");

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsCorrect
        {
            get { return GetValue<bool>(IsCorrectProperty); }
            private set { SetValue(IsCorrectProperty, value); }
        }

        public static readonly PropertyData IsCorrectProperty = RegisterProperty("IsCorrect", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsIncorrect
        {
            get { return GetValue<bool>(IsIncorrectProperty); }
            private set { SetValue(IsIncorrectProperty, value); }
        }

        public static readonly PropertyData IsIncorrectProperty = RegisterProperty("IsIncorrect", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsUnknown
        {
            get { return GetValue<bool>(IsUnknownProperty); }
            private set { SetValue(IsUnknownProperty, value); }
        }

        public static readonly PropertyData IsUnknownProperty = RegisterProperty("IsUnknown", typeof(bool), false);

        #endregion //Properties

        #region Commands

        private bool _isMouseDown;

        /// <summary>
        /// Gets the MarkCorrectCommand command.
        /// </summary>
        public Command<MouseEventArgs> MarkCorrectCommand { get; private set; }

        private void OnMarkCorrectCommandExecute(MouseEventArgs e)
        {
            IsCorrect = !IsCorrect;
            if(IsCorrect)
            {
                IsIncorrect = false;
                IsUnknown = false;
                Page.AddTag(new CorrectnessTag(Page, Origin.Teacher, CorrectnessTag.AcceptedValues.Correct));
            }
        }

        /// <summary>
        /// Gets the MarkCorrectCommand command.
        /// </summary>
        public Command<MouseEventArgs> MarkIncorrectCommand { get; private set; }

        private void OnMarkIncorrectCommandExecute(MouseEventArgs e)
        {
            Console.WriteLine("Marking Incorrect");
            IsIncorrect = !IsIncorrect;
            if(IsIncorrect)
            {
                IsCorrect = false;
                IsUnknown = false;
                Page.AddTag(new CorrectnessTag(Page, Origin.Teacher, CorrectnessTag.AcceptedValues.Incorrect));
            }
        }

        /// <summary>
        /// Gets the MarkCorrectCommand command.
        /// </summary>
        public Command<MouseEventArgs> MarkUnknownCommand { get; private set; }

        private void OnMarkUnknownCommandExecute(MouseEventArgs e)
        {
            Console.WriteLine("Marking Unknown");

            IsUnknown = !IsUnknown;
            if(IsUnknown == true)
            {
                IsCorrect = false;
                IsIncorrect = false;
                Page.AddTag(new CorrectnessTag(Page, Origin.Teacher, CorrectnessTag.AcceptedValues.Unknown));
            }
        }

        /// <summary>
        /// Gets the MarkCorrectCommand command.
        /// </summary>
        public Command<MouseEventArgs> ToggleStarCommand { get; private set; }

        private void OnToggleStarCommandExecute(MouseEventArgs e)
        {
            IsStarred = !IsStarred;
            if(IsStarred)
            {
                Page.AddTag(new StarredTag(Page, StarredTag.AcceptedValues.Starred));
            }
            else
            {
                Page.AddTag(new StarredTag(Page, StarredTag.AcceptedValues.Unstarred));
            }
        }

        /// <summary>
        /// Gets the MarkCorrectCommand command.
        /// </summary>
        public Command<MouseEventArgs> ShowTagsCommand { get; private set; }

        private void OnShowTagsCommandExecute(MouseEventArgs e) { }

        #endregion //Commands
    }
}