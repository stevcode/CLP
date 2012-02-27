using GalaSoft.MvvmLight;
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
                                                                                            //Save the page's history in a temp VM
                                                                                            CLPHistory tempHistory = new CLPHistory();
                                                                                            CLPHistory pageHistory = pageViewModel.HistoryVM.History;
                                                                                            foreach (var key in pageHistory.ObjectReferences.Keys)
                                                                                            {
                                                                                                tempHistory.ObjectReferences.Add(key, pageHistory.ObjectReferences[key]);
                                                                                            }
                                                                                            foreach (var item in pageHistory.HistoryItems)
                                                                                            {
                                                                                                if (item.ObjectID == null)
                                                                                                {
                                                                                                    tempHistory.AddHistoryItem(item);
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    tempHistory.AddHistoryItem(pageHistory.ObjectReferences[item.ObjectID], item);
                                                                                                }
                                                                                            }
                                                                                            foreach (var item in pageHistory.UndoneHistoryItems)
                                                                                            {
                                                                                                if (item.ObjectID == null)
                                                                                                {
                                                                                                    tempHistory.AddUndoneHistoryItem(item);
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    tempHistory.AddUndoneHistoryItem(pageHistory.ObjectReferences[item.ObjectID], item);
                                                                                                }
                                                                                            }

                                                                                            //Clear the page's real history so it doesn't get sent
                                                                                            pageViewModel.HistoryVM.History.HistoryItems.Clear();
                                                                                            pageViewModel.HistoryVM.History.ObjectReferences.Clear();
                                                                                            pageViewModel.HistoryVM.History.UndoneHistoryItems.Clear();

                                                                                            //send the page to the projector
                                                                                            string pageString = ObjectSerializer.ToString(pageViewModel.Page);
                                                                                            App.Peer.Channel.AddPageToDisplay(pageString);

                                                                                            //Put the temp history back into the page
                                                                                            foreach (var key in tempHistory.ObjectReferences.Keys)
                                                                                            {
                                                                                                pageHistory.ObjectReferences.Add(key, tempHistory.ObjectReferences[key]);
                                                                                            }
                                                                                            foreach (var item in tempHistory.HistoryItems)
                                                                                            {
                                                                                                if (item.ObjectID == null)
                                                                                                {
                                                                                                    pageViewModel.HistoryVM.AddHistoryItem(item);
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    pageHistory.AddHistoryItem(tempHistory.ObjectReferences[item.ObjectID], item);
                                                                                                }
                                                                                            }
                                                                                            foreach (var item in tempHistory.UndoneHistoryItems)
                                                                                            {
                                                                                                if (item.ObjectID == null)
                                                                                                {
                                                                                                    pageViewModel.HistoryVM.AddUndoneHistoryItem(item);
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    pageHistory.AddUndoneHistoryItem(tempHistory.ObjectReferences[item.ObjectID], item);
                                                                                                }
                                                                                            }
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