﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.ViewModels.Displays
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class LinkedDisplayViewModel : ViewModelBase, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the LinkedDisplayViewModel class.
        /// </summary>
        public LinkedDisplayViewModel()
        {
            AppMessages.AddPageToDisplay.Register(this, (pageViewModel) => {
                                                                            if (this.IsActive)
                                                                            {
                                                                                this.PageViewModel = pageViewModel;
                                                                                this.PageViewModel.DefaultDA = App.MainWindowViewModel.Ribbon.DrawingAttributes;
                                                                                this.PageViewModel.EditingMode = App.MainWindowViewModel.Ribbon.EditingMode;
                                                                                if (App.CurrentUserMode == App.UserMode.Instructor && !App.IsAuthoring)
                                                                                {
                                                                                    if (App.Peer.Channel != null)
                                                                                    {
                                                                                        if (this.IsOnProjector)
                                                                                        {
                                                                                            Tuple<bool,string,string> pageID;
                                                                                            if (PageViewModel.Page.IsSubmission)
                                                                                            {
                                                                                                pageID = new Tuple<bool, string, string>(true, PageViewModel.Page.UniqueID, PageViewModel.Page.SubmissionID);
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                pageID = new Tuple<bool, string, string>(false, PageViewModel.Page.UniqueID, "");
                                                                                            }

                                                                                            App.Peer.Channel.AddPageToDisplay(pageID); 
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }                                                   
                                                                        });
            AppMessages.RequestCurrentDisplayedPage.Register(this, (action) => { action.Execute(PageViewModel); });

            bool IsActiveTemp = this.IsActive;
            this.IsActive = true;
            AppMessages.AddPageToDisplay.Send(App.CurrentNotebookViewModel.PageViewModels[0]);
            this.IsActive = IsActiveTemp;
        }

        public bool IsActive { get; set; }
        public bool IsOnProjector { get; set; }

        /// <summary>
        /// The <see cref="PageViewModel" /> property's name.
        /// </summary>
        public const string PageViewModelPropertyName = "PageViewModel";

        private CLPPageViewModel _pageViewModel = new CLPPageViewModel(App.CurrentNotebookViewModel);

        /// <summary>
        /// Sets and gets the PageViewModel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public CLPPageViewModel PageViewModel
        {
            get
            {
                return _pageViewModel;
            }

            set
            {
                if (_pageViewModel == value)
                {
                    return;
                }

                _pageViewModel = value;
                RaisePropertyChanged(PageViewModelPropertyName);
            }
        }

        public override void Cleanup()
        {
            Console.WriteLine("unregistered");
            Messenger.Default.Unregister<NotificationMessageAction<CLPPageViewModel>>(this);
            base.Cleanup();
        }

        public void Dispose()
        {
            this.Cleanup();
        }
    }
}