using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class HoverBoxViewModel : ViewModelBase
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the HoverBoxViewModel class.
        /// </summary>
        public HoverBoxViewModel(CLPPage page)
        {
            Page = page;
            IsCorrect = false;
            IsUnknown = true;
            IsIncorrect = false;
            IsStarred = false;
            Topics = "";

            //if(Page.PageTopics != null) //TODO: PageTopics gone, need to be Tags
            //{
            //    foreach(string topic in Page.PageTopics)
            //    {
            //        Topics += "Page Topic: " + topic + "\n";
            //    }
            //}

            // TODO: Entities
            //if(Page.PageTags != null)
            //{
            //    foreach(Tag tag in Page.PageTags)
            //    {
            //        if(tag.TagType == null)
            //        {
            //            continue;
            //        } // Skip tags that somehow didn't get a TagType, to avoid an exception in the next line
            //        if(tag.TagType.Name == CorrectnessTagType.Instance.Name)
            //        {
            //            if(tag.Value.Count > 0)
            //            {
            //                String correct = (String)tag.Value.ElementAt(0).Value;
            //                if(correct == "Correct")
            //                {
            //                    Topics += "Correctness: Correct \n";
            //                    IsCorrect = true;
            //                    IsIncorrect = false;
            //                    IsUnknown = false;
            //                }
            //                else if(correct == "Incorrect")
            //                {
            //                    Topics += "Correctness: Incorrect \n";
            //                    IsIncorrect = true;
            //                    IsCorrect = false;
            //                    IsUnknown = false;
            //                }
            //                else
            //                {
            //                    Topics += "Correctness: Unknown \n";
            //                    IsUnknown = true;
            //                    IsCorrect = false;
            //                    IsIncorrect = false;
            //                }
            //            }
            //        }
            //        if(tag.TagType.Name == StarredTagType.Instance.Name)
            //        {
            //            if(tag.Value.Count > 0)
            //            {
            //                String star = (String)tag.Value.ElementAt(0).Value;
            //                if(star == "Starred")
            //                {
            //                    Topics += "Starred: True \n";
            //                    IsStarred = true;
            //                }
            //                else
            //                {
            //                    Topics += "Starred: False \n";
            //                }
            //            }
            //        }
            //    }
            //}

            MarkCorrectCommand = new Command<MouseEventArgs>(OnMarkCorrectCommandExecute);
            MarkIncorrectCommand = new Command<MouseEventArgs>(OnMarkIncorrectCommandExecute);
            MarkUnknownCommand = new Command<MouseEventArgs>(OnMarkUnknownCommandExecute);
            ToggleStarCommand = new Command<MouseEventArgs>(OnToggleStarCommandExecute);
            ShowTagsCommand = new Command<MouseEventArgs>(OnShowTagsCommandExecute);
        }

        public override string Title { get { return "HoverBoxVM"; } }

        #endregion //Constructors

        #region Properties

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

        // TODO: Entities
        ///// <summary>
        ///// Gets or sets the property value.
        ///// </summary>
        //[ViewModelToModel("Page")]
        //public ObservableCollection<Tag> PageTags
        //{
        //    get { return GetValue<ObservableCollection<Tag>>(PageTagsProperty); } 
        //    set { SetValue(PageTagsProperty, value); }
        //}

        //public static readonly PropertyData PageTagsProperty = RegisterProperty("PageTags", typeof(ObservableCollection<Tag>));

        public bool IsStarred
        {
            get { return GetValue<bool>(IsStarredProperty); } 
            private set { SetValue(IsStarredProperty, value); }
        }

        public static readonly PropertyData IsStarredProperty = RegisterProperty("IsStarred", typeof(bool), false);

        public string Topics { get { return GetValue<string>(TopicsProperty); } private set { SetValue(TopicsProperty, value); } }

        public static readonly PropertyData TopicsProperty = RegisterProperty("Topics", typeof(string), "");

        /// <summary>
        ///     Gets or sets the property value.
        /// </summary>
        public bool IsCorrect { get { return GetValue<bool>(IsCorrectProperty); } private set { SetValue(IsCorrectProperty, value); } }

        public static readonly PropertyData IsCorrectProperty = RegisterProperty("IsCorrect", typeof(bool), false);

        /// <summary>
        ///     Gets or sets the property value.
        /// </summary>
        public bool IsIncorrect { get { return GetValue<bool>(IsIncorrectProperty); } private set { SetValue(IsIncorrectProperty, value); } }

        public static readonly PropertyData IsIncorrectProperty = RegisterProperty("IsIncorrect", typeof(bool), false);

        /// <summary>
        ///     Gets or sets the property value.
        /// </summary>
        public bool IsUnknown
        {
            get { return GetValue<bool>(IsUnknownProperty); } 
            private set { SetValue(IsUnknownProperty, value); }
        }

        public static readonly PropertyData IsUnknownProperty = RegisterProperty("IsUnknown", typeof(bool), false);

        #endregion //Properties

        #region Bindings

        /// <summary>
        ///     Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public int NumberOfSubmissions
        {
            get { return GetValue<int>(NumberOfSubmissionsProperty); } 
            set { SetValue(NumberOfSubmissionsProperty, value); }
        }

        public static readonly PropertyData NumberOfSubmissionsProperty = RegisterProperty("NumberOfSubmissions", typeof(int));

        /// <summary>
        ///     Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public int NumberOfGroupSubmissions
        {
            get { return GetValue<int>(NumberOfGroupSubmissionsProperty); } 
            set { SetValue(NumberOfGroupSubmissionsProperty, value); }
        }

        public static readonly PropertyData NumberOfGroupSubmissionsProperty = RegisterProperty("NumberOfGroupSubmissions", typeof(int));

        #endregion //Bindings

        #region Commands

        private bool _isMouseDown;      

        /// <summary>
        ///     Gets the MarkCorrectCommand command.
        /// </summary>
        public Command<MouseEventArgs> MarkCorrectCommand { get; private set; }

        private void OnMarkCorrectCommandExecute(MouseEventArgs e)
        {
            IsCorrect = !IsCorrect;
            if(IsCorrect == true)
            {
                // TODO: Entities
                //if(Page.PageTags != null)
                //{
                //    foreach(Tag tag in Page.PageTags)
                //    {
                //        if(tag.TagType.Name == CorrectnessTagType.Instance.Name)
                //        {
                //            tag.Value.Clear();
                //            tag.Value.Add(new TagOptionValue("Correct", "..\\Images\\Correct.png"));
                //        }
                //    }
                //}
                //IsIncorrect = false;
                //IsUnknown = false;
            }
            //  CLPNotebook notebook = (App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).Notebook;
            //  notebook.Submissions.Remove(Page.UniqueID);
            //notebook.AddStudentSubmission(Page.UniqueID, Page);
            var submissionsPanel = NotebookPagesPanelViewModel.GetSubmissionsPanelViewModel();
            submissionsPanel.SubmissionPages.Remove(Page);
            submissionsPanel.SubmissionPages.Add(Page);
        }

        /// <summary>
        ///     Gets the MarkCorrectCommand command.
        /// </summary>
        public Command<MouseEventArgs> MarkIncorrectCommand { get; private set; }

        private void OnMarkIncorrectCommandExecute(MouseEventArgs e)
        {
            Console.WriteLine("Marking INcorrect");
            IsIncorrect = !IsIncorrect;
            if(IsIncorrect == true)
            {
                IsCorrect = false;
                IsUnknown = false;
                // TODO: Entities
                //if(Page.PageTags != null)
                //{
                //    Console.WriteLine("[age tags:" + Page.PageTags.Count);
                //    foreach(Tag tag in Page.PageTags)
                //    {
                //        if(tag.TagType.Name == CorrectnessTagType.Instance.Name)
                //        {
                //            tag.Value.Clear();
                //            tag.Value.Add(new TagOptionValue("Incorrect", "..\\Images\\Incorrect.png"));
                //        }
                //    }
                //}
            }
            var submissionsPanel = NotebookPagesPanelViewModel.GetSubmissionsPanelViewModel();
            submissionsPanel.SubmissionPages.Remove(Page);
            submissionsPanel.SubmissionPages.Add(Page);
        }

        /// <summary>
        ///     Gets the MarkCorrectCommand command.
        /// </summary>
        public Command<MouseEventArgs> MarkUnknownCommand { get; private set; }

        private void OnMarkUnknownCommandExecute(MouseEventArgs e)
        {
            Console.WriteLine("Marking Unkown");

            IsUnknown = !IsUnknown;
            if(IsUnknown == true)
            {
                IsCorrect = false;
                IsIncorrect = false;
                // TODO: Entities
                //if(Page.PageTags != null)
                //{
                //    Console.WriteLine("in unknown:" + Page.PageTags.Count);
                //    foreach(Tag tag in Page.PageTags)
                //    {
                //        if(tag.TagType.Name == CorrectnessTagType.Instance.Name)
                //        {
                //            tag.Value.Clear();
                //            tag.Value.Add(new TagOptionValue("Unknown", ""));
                //        }
                //    }
                //}
            }
            var submissionsPanel = NotebookPagesPanelViewModel.GetSubmissionsPanelViewModel();
            submissionsPanel.SubmissionPages.Remove(Page);
            submissionsPanel.SubmissionPages.Add(Page);
        }

        /// <summary>
        ///     Gets the MarkCorrectCommand command.
        /// </summary>
        public Command<MouseEventArgs> ToggleStarCommand { get; private set; }

        private void OnToggleStarCommandExecute(MouseEventArgs e)
        {
            IsStarred = !IsStarred;
            // TODO: Entities
            //if(Page.PageTags != null)
            //{
            //    foreach(Tag tag in Page.PageTags)
            //    {
            //        if(tag.TagType.Name == StarredTagType.Instance.Name)
            //        {
            //            Console.WriteLine("Name: " + tag.TagType.Name + " value" + tag.Value.ElementAt(0).Value);

            //            tag.Value.Clear();
            //            if(IsStarred)
            //            {
            //                Topics.Replace("Starred: True", "Starred: False");
            //                tag.Value.Add(new TagOptionValue("Starred", "..\\Images\\Starred.png"));
            //            }
            //            else
            //            {
            //                Topics.Replace("Starred: False", "Starred: True");
            //                tag.Value.Add(new TagOptionValue("Unstarred", "..\\Images\\Unstarred.png"));
            //            }
            //        }
            //    }
            //}
            var submissionsPanel = NotebookPagesPanelViewModel.GetSubmissionsPanelViewModel();
            submissionsPanel.SubmissionPages.Remove(Page);
            submissionsPanel.SubmissionPages.Add(Page);
        }

        /// <summary>
        ///     Gets the MarkCorrectCommand command.
        /// </summary>
        public Command<MouseEventArgs> ShowTagsCommand { get; private set; }

        private void OnShowTagsCommandExecute(MouseEventArgs e) { }

        #endregion //Commands
    }
}