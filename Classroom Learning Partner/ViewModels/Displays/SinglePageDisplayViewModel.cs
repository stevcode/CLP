using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;

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
    public class SinglePageDisplayViewModel : ViewModelBase, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the SinglePageDisplayViewModel class.
        /// </summary>
        public SinglePageDisplayViewModel()
        {
            AppMessages.AddPageToDisplay.Register(this, (pageViewModel) => {
                                                                        this.PageViewModel = pageViewModel;
                                                                        this.PageViewModel.DefaultDA = App.MainWindowViewModel.Ribbon.DrawingAttributes;
                                                                        this.PageViewModel.EditingMode = App.MainWindowViewModel.Ribbon.EditingMode;
                                                                        });
            AppMessages.RequestCurrentDisplayedPage.Register(this, (action) => { action.Execute(PageViewModel); });
            Console.WriteLine("registered");
            AppMessages.AddPageToDisplay.Send(App.CurrentNotebookViewModel.PageViewModels[0]);
        }

        /// <summary>
        /// The <see cref="PageViewModel" /> property's name.
        /// </summary>
        public const string PageViewModelPropertyName = "PageViewModel";

        private CLPPageViewModel _pageViewModel = new CLPPageViewModel();

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