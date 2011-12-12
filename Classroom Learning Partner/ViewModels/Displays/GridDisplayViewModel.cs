using GalaSoft.MvvmLight;
using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;

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
    public class GridDisplayViewModel : ViewModelBase, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the GridDisplayViewModel class.
        /// </summary>
        public GridDisplayViewModel()
        {
            AppMessages.AddPageToDisplay.Register(this, (pageViewModel) =>
            {
                if (this.IsActive)
                {
                    pageViewModel.DefaultDA = App.MainWindowViewModel.Ribbon.DrawingAttributes;
                    pageViewModel.EditingMode = App.MainWindowViewModel.Ribbon.EditingMode;
                    this.DisplayPages.Add(pageViewModel);
                    Console.WriteLine("page added to grid");
                }
            });
        }

        public bool IsActive { get; set; }
        public bool IsOnProjector { get; set; }

        private ObservableCollection<CLPPageViewModel> _displayPages = new ObservableCollection<CLPPageViewModel>();
        public ObservableCollection<CLPPageViewModel> DisplayPages
        {
            get
            {
                return _displayPages;
            }
        }

        private RelayCommand<CLPPageViewModel> _removePageFromGridDisplayCommand;

        /// <summary>
        /// Gets the RemovePageFromGridDisplayCommand.
        /// </summary>
        public RelayCommand<CLPPageViewModel> RemovePageFromGridDisplayCommand
        {
            get
            {
                return _removePageFromGridDisplayCommand
                    ?? (_removePageFromGridDisplayCommand = new RelayCommand<CLPPageViewModel>(
                                          (pageViewModel) =>
                                          {
                                              DisplayPages.Remove(pageViewModel);
                                          }));
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}