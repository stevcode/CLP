﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;
using Classroom_Learning_Partner.Views;


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
            IsUnknown = false;
            IsIncorrect = false;
            IsStarred = false;
            if(page.PageTags != null)
            {
                foreach(Tag tag in Page.PageTags)
                {

                    if(tag.TagType is CorrectnessTagType)
                    {
                        /**  String correct = tag.Value.ElementAt(0).Value;
                          if (correct=="Correct") {
                              IsCorrect = true;
                          }
                          else if(correct == "Incorrect")
                          {
                              IsIncorrect = true;
                          }
                          else
                          {
                              IsUnknown = true;

                          }*/
                    }
                    if(tag.TagType is StarredTagType)
                    {
                        /** String star = tag.Value.ElementAt(0).Value;
                         if(star == "Starred")
                         {
                             IsStarred = true;
                         }*/
                    }

                }
            }




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
        [Model(SupportIEditableObject = false)]
        public CLPPage Page
        {
            get { return GetValue<CLPPage>(PageProperty); }
            private set { SetValue(PageProperty, value); }
        }

        /// <summary>
        /// Register the Page property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageProperty = RegisterProperty("Page", typeof(CLPPage));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public ObservableCollection<Tag> PageTags
        {
            get { return GetValue<ObservableCollection<Tag>>(PageTagsProperty); }
            set { SetValue(PageTagsProperty, value); }
        }

        public static readonly PropertyData PageTagsProperty = RegisterProperty("PageTags", typeof(ObservableCollection<Tag>));

        public bool IsStarred
        {
            get { return GetValue<bool>(IsStarredProperty); }
            private set { SetValue(IsStarredProperty, value); }
        }

        /// <summary>
        /// Register the Page property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsStarredProperty = RegisterProperty("IsStarred", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsCorrect
        {
            get { return GetValue<bool>(IsCorrectProperty); }
            private set { SetValue(IsCorrectProperty, value); }
        }

        /// <summary>
        /// Register the Page property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsCorrectProperty = RegisterProperty("IsCorrect", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsIncorrect
        {
            get { return GetValue<bool>(IsIncorrectProperty); }
            private set { SetValue(IsIncorrectProperty, value); }
        }

        /// <summary>
        /// Register the Page property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsIncorrectProperty = RegisterProperty("IsIncorrect", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsUnknown
        {
            get { return GetValue<bool>(IsUnknownProperty); }
            private set { SetValue(IsUnknownProperty, value); }
        }

        /// <summary>
        /// Register the Page property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsUnknownProperty = RegisterProperty("IsUnknown", typeof(bool), false);


        #endregion //Properties

        #region Bindings

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public int NumberOfSubmissions
        {
            get { return GetValue<int>(NumberOfSubmissionsProperty); }
            set { SetValue(NumberOfSubmissionsProperty, value); }
        }

        public static readonly PropertyData NumberOfSubmissionsProperty = RegisterProperty("NumberOfSubmissions", typeof(int));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("Page")]
        public int NumberOfGroupSubmissions
        {
            get { return GetValue<int>(NumberOfGroupSubmissionsProperty); }
            set { SetValue(NumberOfGroupSubmissionsProperty, value); }
        }

        /// <summary>
        /// Register the NumberOfGroupSubmissions property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NumberOfGroupSubmissionsProperty = RegisterProperty("NumberOfGroupSubmissions", typeof(int));
        #endregion //Bindings

        #region Commands

        private bool _isMouseDown;
        public Canvas TopCanvas = null;

        public T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for(int i = 0; i < numVisuals; i++)
            {
                var v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T ?? GetVisualChild<T>(v);
                if(child != null)
                    break;
            }
            return child;
        }

        public T GetVisualParent<T>(Visual child) where T : Visual
        {
            var p = (Visual)VisualTreeHelper.GetParent(child);
            var parent = p as T ?? GetVisualParent<T>(p);

            return parent;
        }

        public T FindNamedChild<T>(FrameworkElement obj, string name)
        {
            var dep = obj as DependencyObject;
            T ret = default(T);

            if(dep != null)
            {
                int childcount = VisualTreeHelper.GetChildrenCount(dep);
                for(int i = 0; i < childcount; i++)
                {
                    var childDep = VisualTreeHelper.GetChild(dep, i);
                    var child = childDep as FrameworkElement;

                    if(child != null && (child.GetType() == typeof(T) && child.Name == name))
                    {
                        ret = (T)Convert.ChangeType(child, typeof(T));
                        break;
                    }

                    ret = FindNamedChild<T>(child, name);
                    if(ret != null)
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Gets the MarkCorrectCommand command.
        /// </summary>
        public Command<MouseEventArgs> MarkCorrectCommand { get; private set; }

        private void OnMarkCorrectCommandExecute(MouseEventArgs e)
        {
            IsCorrect = !IsCorrect;
            if(IsCorrect == true)
            {
                if(Page.PageTags != null)
                {
                    foreach(Tag tag in Page.PageTags)
                    {
                        if(tag.TagType is CorrectnessTagType)
                        {
                            tag.Value.Clear();
                            tag.Value.Add(new TagOptionValue("Correct", "..\\Images\\Correct.png"));

                        }
                    }
                }
                IsIncorrect = false;
                IsUnknown = false;
                System.Console.WriteLine("page tags:" + Page.PageTags.Count);

            }
           
        }
        /// <summary>
        /// Gets the MarkCorrectCommand command.
        /// </summary>
        public Command<MouseEventArgs> MarkIncorrectCommand { get; private set; }

        private void OnMarkIncorrectCommandExecute(MouseEventArgs e)
        {
            
            IsIncorrect = !IsIncorrect;
            if(IsIncorrect == true)
            {
                IsCorrect = false;
                IsUnknown = false;
                if(Page.PageTags != null)
                {
                    System.Console.WriteLine("[age tags:" + Page.PageTags.Count);
                    foreach(Tag tag in Page.PageTags)
                    {
                        if(tag.TagType is CorrectnessTagType)
                        {
                            tag.Value.Clear();
                            tag.Value.Add(new TagOptionValue("Incorrect", "..\\Images\\Incorrect.png"));

                        }
                    }
                }

            }
        }
        /// <summary>
        /// Gets the MarkCorrectCommand command.
        /// </summary>
        public Command<MouseEventArgs> MarkUnknownCommand { get; private set; }

        private void OnMarkUnknownCommandExecute(MouseEventArgs e)
        {
            

            IsUnknown = !IsUnknown;
            if(IsUnknown == true)
            {
                IsCorrect = false;
                IsIncorrect = false;
                if(Page.PageTags != null)
                {
                    System.Console.WriteLine("in unknown:" + Page.PageTags.Count);
                    foreach(Tag tag in Page.PageTags)
                    {
                        if(tag.TagType is CorrectnessTagType)
                        {
                            tag.Value.Clear();
                            tag.Value.Add(new TagOptionValue("Unknown", ""));

                        }
                    }

                }
            }
        }
        /// <summary>
        /// Gets the MarkCorrectCommand command.
        /// </summary>
        public Command<MouseEventArgs> ToggleStarCommand { get; private set; }

        private void OnToggleStarCommandExecute(MouseEventArgs e)
        {
            IsStarred = !IsStarred;
                   if(Page.PageTags != null)
                {
                    foreach(Tag tag in Page.PageTags)
                    {
                        if(tag.TagType is StarredTagType)
                        {
                            tag.Value.Clear();
                            if(IsStarred)
                            {
                                tag.Value.Add(new TagOptionValue("Starred", "..\\Images\\Starred.png"));
                            }
                            else
                            {
                                tag.Value.Add(new TagOptionValue("Unstarred", "..\\Images\\Unstarred.png"));
                            }

                        }
                    }
            }
        }
        /// <summary>
        /// Gets the MarkCorrectCommand command.
        /// </summary>
        public Command<MouseEventArgs> ShowTagsCommand { get; private set; }

        private void OnShowTagsCommandExecute(MouseEventArgs e)
        {
        }


        #endregion //Commands

        #region Methods

        public void initializeTags()
        {
         
        }

        #endregion //Methods

    }
}