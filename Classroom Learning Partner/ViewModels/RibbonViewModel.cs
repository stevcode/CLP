using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model;
using GalaSoft.MvvmLight.Command;
using System.IO;
using Classroom_Learning_Partner.ViewModels.Workspaces;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Controls;
using System;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using Microsoft.Windows.Controls.Ribbon;
using System.Collections.ObjectModel;

namespace Classroom_Learning_Partner.ViewModels
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
    public class RibbonViewModel : ViewModelBase
    {
        public const double PEN_RADIUS = 2;
        public const double MARKER_RADIUS = 5;
        public const double ERASER_RADIUS = 5;

        /// <summary>
        /// Initializes a new instance of the RibbonViewModel class.
        /// </summary>
        public RibbonViewModel()
        {
            CLPService = new CLPServiceAgent();
            _drawingAttributes.Height = PEN_RADIUS;
            _drawingAttributes.Width = PEN_RADIUS;
            _drawingAttributes.Color = Colors.Black;
            _drawingAttributes.FitToCurve = true;

            _currentColorButton.Background = new SolidColorBrush(Colors.Black);

            switch (App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    break;
                case App.UserMode.Instructor:
                    InstructorVisibility = Visibility.Visible;
                    break;
                case App.UserMode.Projector:
                    break;
                case App.UserMode.Student:
                    StudentVisibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private ICLPServiceAgent CLPService { get; set; }

        private DrawingAttributes _drawingAttributes = new DrawingAttributes();
        public DrawingAttributes DrawingAttributes
        {
            get
            {
                return _drawingAttributes;
            }
        }

        private InkCanvasEditingMode _editingMode = InkCanvasEditingMode.Ink;

        /// <summary>
        /// Sets and gets the EditingMode property.
        /// </summary>
        public InkCanvasEditingMode EditingMode
        {
            get
            {
                return _editingMode;
            }

            set
            {
                if (_editingMode == value)
                {
                    return;
                }

                _editingMode = value;
            }
        }

        #region Bindings

        /// <summary>
        /// The <see cref="AuthoringTabVisibility" /> property's name.
        /// </summary>
        public const string AuthoringTabVisibilityPropertyName = "AuthoringTabVisibility";

        private Visibility _authoringTabVisibility = Visibility.Hidden;

        /// <summary>
        /// Sets and gets the AuthoringTabVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility AuthoringTabVisibility
        {
            get
            {
                return _authoringTabVisibility;
            }

            set
            {
                if (_authoringTabVisibility == value)
                {
                    return;
                }

                _authoringTabVisibility = value;
                RaisePropertyChanged(AuthoringTabVisibilityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="InstructorVisibility" /> property's name.
        /// </summary>
        public const string InstructorVisibilityPropertyName = "InstructorVisibility";

        private Visibility _instructorVisibility = Visibility.Collapsed;

        /// <summary>
        /// Sets and gets the InstructorVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility InstructorVisibility
        {
            get
            {
                return _instructorVisibility;
            }

            set
            {
                if (_instructorVisibility == value)
                {
                    return;
                }

                _instructorVisibility = value;
                RaisePropertyChanged(InstructorVisibilityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="StudentVisibility" /> property's name.
        /// </summary>
        public const string StudentVisibilityPropertyName = "StudentVisibility";

        private Visibility _studentVisibility = Visibility.Collapsed;

        /// <summary>
        /// Sets and gets the StudentVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility StudentVisibility
        {
            get
            {
                return _studentVisibility;
            }

            set
            {
                if (_studentVisibility == value)
                {
                    return;
                }

                _studentVisibility = value;
                RaisePropertyChanged(StudentVisibilityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ProjectorVisibility" /> property's name.
        /// </summary>
        public const string RibbonVisibilityPropertyName = "RibbonVisibility";

        private Visibility _ribbonVisibility = Visibility.Visible;

        /// <summary>
        /// Sets and gets the ProjectorVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility RibbonVisibility
        {
            get
            {
                return _ribbonVisibility;
            }

            set
            {
                if (_ribbonVisibility == value)
                {
                    return;
                }

                _ribbonVisibility = value;
                RaisePropertyChanged(RibbonVisibilityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="CurrentColorButton" /> property's name.
        /// </summary>
        public const string CurrentColorButtonPropertyName = "CurrentColorButton";

        private RibbonButton _currentColorButton = new RibbonButton();

        /// <summary>
        /// Sets and gets the CurrentColorButton property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public RibbonButton CurrentColorButton
        {
            get
            {
                return _currentColorButton;
            }

            set
            {
                if (_currentColorButton == value)
                {
                    return;
                }

                _currentColorButton = value;
                RaisePropertyChanged(CurrentColorButtonPropertyName);
            }
        }

        #region TextBox

        private ObservableCollection<FontFamily> _fonts = new ObservableCollection<FontFamily>(System.Windows.Media.Fonts.SystemFontFamilies);
        public ObservableCollection<FontFamily> Fonts
        {
            get
            {
                return _fonts;
            }
        }

        /// <summary>
        /// The <see cref="CurrentFontFamily" /> property's name.
        /// </summary>
        public const string CurrentFontFamilyPropertyName = "CurrentFontFamily";

        private FontFamily _currectFontFamily = new FontFamily("Times New Roman");

        /// <summary>
        /// Sets and gets the CurrentFontFamily property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public FontFamily CurrentFontFamily
        {
            get
            {
                return _currectFontFamily;
            }

            set
            {
                if (_currectFontFamily == value)
                {
                    return;
                }

                _currectFontFamily = value;
                RaisePropertyChanged(CurrentFontFamilyPropertyName);
            }
        }

        private ObservableCollection<double> _fontSizes = new ObservableCollection<double>(){3.0, 4.0, 5.0, 6.0, 6.5, 7.0, 7.5, 8.0, 8.5, 9.0, 9.5, 
		                                                                                    10.0, 10.5, 11.0, 11.5, 12.0, 12.5, 13.0, 13.5, 14.0, 15.0,
		                                                                                    16.0, 17.0, 18.0, 19.0, 20.0, 22.0, 24.0, 26.0, 28.0, 30.0,
		                                                                                    32.0, 34.0, 36.0, 38.0, 40.0, 44.0, 48.0, 52.0, 56.0, 60.0, 64.0, 68.0, 72.0, 76.0,
		                                                                                    80.0, 88.0, 96.0, 104.0, 112.0, 120.0, 128.0, 136.0, 144.0};

        public ObservableCollection<double> FontSizes
        {
            get
            {
                return _fontSizes;
            }
        }

        /// <summary>
        /// The <see cref="CurrentFontSize" /> property's name.
        /// </summary>
        public const string CurrentFontSizePropertyName = "CurrentFontSize";

        private double _currentFontSize = 24;

        /// <summary>
        /// Sets and gets the CurrentFontSize property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double CurrentFontSize
        {
            get
            {
                return _currentFontSize;
            }

            set
            {
                if (_currentFontSize == value)
                {
                    return;
                }

                _currentFontSize = value;
                RaisePropertyChanged(CurrentFontSizePropertyName);
            }
        }


        #endregion //TextBox

        #endregion //Bindings

        #region Commands

        #region Pen Commands

        private RelayCommand _setPenCommand;

        /// <summary>
        /// Gets the SetPenCommand.
        /// </summary>
        public RelayCommand SetPenCommand
        {
            get
            {
                return _setPenCommand
                    ?? (_setPenCommand = new RelayCommand(
                                          () =>
                                          {
                                              DrawingAttributes.Height = PEN_RADIUS;
                                              DrawingAttributes.Width = PEN_RADIUS;
                                              EditingMode = InkCanvasEditingMode.Ink;
                                              AppMessages.ChangeInkMode.Send(InkCanvasEditingMode.Ink);
                                          }));
            }
        }

        private RelayCommand _setMarkerCommand;

        /// <summary>
        /// Gets the SetMarkerCommand.
        /// </summary>
        public RelayCommand SetMarkerCommand
        {
            get
            {
                return _setMarkerCommand
                    ?? (_setMarkerCommand = new RelayCommand(
                                          () =>
                                          {
                                              DrawingAttributes.Height = MARKER_RADIUS;
                                              DrawingAttributes.Width = MARKER_RADIUS;
                                              EditingMode = InkCanvasEditingMode.Ink;
                                              AppMessages.ChangeInkMode.Send(InkCanvasEditingMode.Ink);
                                          }));
            }
        }

        private RelayCommand _setEraserCommand;

        /// <summary>
        /// Gets the SetEraserCommand.
        /// </summary>
        public RelayCommand SetEraserCommand
        {
            get
            {
                return _setEraserCommand
                    ?? (_setEraserCommand = new RelayCommand(
                                          () =>
                                          {
                                              DrawingAttributes.Height = ERASER_RADIUS;
                                              DrawingAttributes.Width = ERASER_RADIUS;
                                              EditingMode = InkCanvasEditingMode.EraseByPoint;
                                              AppMessages.ChangeInkMode.Send(InkCanvasEditingMode.EraseByPoint);
                                          }));
            }
        }

        private RelayCommand _setStrokeEraserCommand;

        /// <summary>
        /// Gets the SetStrokeEraserCommand.
        /// </summary>
        public RelayCommand SetStrokeEraserCommand
        {
            get
            {
                return _setStrokeEraserCommand
                    ?? (_setStrokeEraserCommand = new RelayCommand(
                                          () =>
                                          {
                                              EditingMode = InkCanvasEditingMode.EraseByStroke;
                                              AppMessages.ChangeInkMode.Send(InkCanvasEditingMode.EraseByStroke);
                                          }));
            }
        }

        private RelayCommand<RibbonButton> _setPenColorCommand;

        /// <summary>
        /// Gets the SetPenColorCommand.
        /// </summary>
        public RelayCommand<RibbonButton> SetPenColorCommand
        {
            get
            {
                return _setPenColorCommand
                    ?? (_setPenColorCommand = new RelayCommand<RibbonButton>(
                                          (button) =>
                                          {
                                              CurrentColorButton = button as RibbonButton;
                                              DrawingAttributes.Color = (CurrentColorButton.Background as SolidColorBrush).Color;
                                              _editingMode = InkCanvasEditingMode.Ink;
                                          }));
            }
        }

        private RelayCommand _SetLaserPointerModeCommand;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand SetLaserPointerModeCommand
        {
            get
            {
                return _SetLaserPointerModeCommand
                    ?? (_SetLaserPointerModeCommand = new RelayCommand(
                                          () =>
                                          {
                                              // do work here
                                              // this.editMode, set to none so pen is turned off
                                              // tell messenger to set a flag that we're listening for pen movement, 

                                              EditingMode = InkCanvasEditingMode.None;
                                              AppMessages.ChangeInkMode.Send(InkCanvasEditingMode.None);
                                              AppMessages.SetLaserPointerMode.Send(true);
                                          }));
            }
        }

        #endregion //Pen Commands

        #region Notebook Commands

        private RelayCommand _newNotebookCommand;

        /// <summary>
        /// Gets the NewNotebookCommand.
        /// </summary>
        public RelayCommand NewNotebookCommand
        {
            get
            {
                return _newNotebookCommand
                    ?? (_newNotebookCommand = new RelayCommand(
                                          () =>
                                          {
                                              CLPService.OpenNewNotebook();
                                              AuthoringTabVisibility = Visibility.Visible;
                                          }));
            }
        }



        private RelayCommand _openNotebookCommand;

        /// <summary>
        /// Gets the OpenNotebookCommand.
        /// </summary>
        public RelayCommand OpenNotebookCommand
        {
            get
            {
                return _openNotebookCommand
                    ?? (_openNotebookCommand = new RelayCommand(
                                          () =>
                                          {
                                              App.MainWindowViewModel.Workspace = new NotebookChooserWorkspaceViewModel();
                                          }));
            }
        }

        private RelayCommand _editNotebookCommand;

        /// <summary>
        /// Gets the EditNotebookCommand.
        /// </summary>
        public RelayCommand EditNotebookCommand
        {
            get
            {
                return _editNotebookCommand
                    ?? (_editNotebookCommand = new RelayCommand(
                                          () =>
                                          {
                                              App.IsAuthoring = true;
                                              App.MainWindowViewModel.Workspace = new AuthoringWorkspaceViewModel();
                                              AuthoringTabVisibility = Visibility.Visible;
                                          }));
            }
        }

        private RelayCommand _doneEditingNotebookCommand;

        /// <summary>
        /// Gets the DoneEditingNotebookCommand.
        /// </summary>
        public RelayCommand DoneEditingNotebookCommand
        {
            get
            {
                return _doneEditingNotebookCommand
                    ?? (_doneEditingNotebookCommand = new RelayCommand(
                                          () =>
                                          {
                                              App.IsAuthoring = false;
                                              CLPService.SetWorkspace();
                                          }));
            }
        }

        private RelayCommand _saveNotebookCommand;

        /// <summary>
        /// Gets the SaveNotebookCommand.
        /// </summary>
        public RelayCommand SaveNotebookCommand
        {
            get
            {
                return _saveNotebookCommand
                    ?? (_saveNotebookCommand = new RelayCommand(
                                          () =>
                                          {
                                              CLPService.SaveNotebook(App.CurrentNotebookViewModel);
                                          }));
            }
        }

        private RelayCommand _saveAllNotebooksCommand;

        /// <summary>
        /// Gets the SaveAllNotebooksCommand.
        /// </summary>
        public RelayCommand SaveAllNotebooksCommand
        {
            get
            {
                return _saveAllNotebooksCommand
                    ?? (_saveAllNotebooksCommand = new RelayCommand(
                                          () =>
                                          {
                                              foreach (CLPNotebookViewModel notebookVM in App.NotebookViewModels)
                                              {
                                                  CLPService.SaveNotebook(notebookVM);
                                              }
                                          }));
            }
        }

        private RelayCommand _convertToXPSCommand;

        /// <summary>
        /// Gets the ConvertToXPSCommand.
        /// </summary>
        public RelayCommand ConvertToXPSCommand
        {
            get
            {
                return _convertToXPSCommand
                    ?? (_convertToXPSCommand = new RelayCommand(
                                          () =>
                                          {

                                          }));
            }
        }

        #endregion //Notebook Commands

        #region Page Commands

        private RelayCommand _addNewPageCommand;

        /// <summary>
        /// Gets the AddPageCommand.
        /// </summary>
        public RelayCommand AddNewPageCommand
        {
            get
            {
                return _addNewPageCommand
                    ?? (_addNewPageCommand = new RelayCommand(
                                          () =>
                                          {
                                              int currentPageIndex = -1;
                                              AppMessages.RequestCurrentDisplayedPage.Send((pageViewModel) =>
                                              {
                                                  currentPageIndex = App.CurrentNotebookViewModel.GetNotebookPageIndex(pageViewModel);
                                              });
                                              if (currentPageIndex != -1)
                                              {
                                                  currentPageIndex++;
                                                  CLPService.AddPageAt(new CLPPage(), currentPageIndex, -1);
                                              }
                                              else
                                              {
                                                  Console.WriteLine("[Error] Requested page is a submission, not a notebookpage");
                                              }     
                                          }));
            }
        }

        private RelayCommand _deletePageCommand;

        /// <summary>
        /// Gets the DeletePageCommand.
        /// </summary>
        public RelayCommand DeletePageCommand
        {
            get
            {
                return _deletePageCommand
                    ?? (_deletePageCommand = new RelayCommand(
                                          () =>
                                          {
                                              int currentPageIndex = -1;
                                              AppMessages.RequestCurrentDisplayedPage.Send((callbackMessage) =>
                                              {
                                                  currentPageIndex = App.CurrentNotebookViewModel.PageViewModels.IndexOf(callbackMessage);
                                              });
                                              CLPService.RemovePageAt(currentPageIndex);
                                          }));
            }
        }

        #endregion //Page Commands

        #region Insert Commands

        private RelayCommand _insertTextBoxCommand;

        /// <summary>
        /// Gets the InsertTextBoxCommand.
        /// </summary>
        public RelayCommand InsertTextBoxCommand
        {
            get
            {
                return _insertTextBoxCommand
                    ?? (_insertTextBoxCommand = new RelayCommand(
                                          () =>
                                          {
                                              CLPTextBox textBox = new CLPTextBox();
                                              CLPService.AddPageObjectToPage(textBox);
                                          }));
            }
        }

        private RelayCommand _insertImageCommand;

        /// <summary>
        /// Gets the InsertImageCommand.
        /// </summary>
        public RelayCommand InsertImageCommand
        {
            get
            {
                return _insertImageCommand
                    ?? (_insertImageCommand = new RelayCommand(
                                          () =>
                                          {
                                              // Configure open file dialog box
                                              Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                                              dlg.Filter = "Images|*.png;*.jpg;*.jpeg;*.gif"; // Filter files by extension

                                              // Show open file dialog box
                                              Nullable<bool> result = dlg.ShowDialog();

                                              // Process open file dialog box results
                                              if (result == true)
                                              {
                                                  // Open document
                                                  string filename = dlg.FileName;
                                                  CLPImage image = new CLPImage(filename);
                                                  CLPService.AddPageObjectToPage(image);
                                              }
                                          }));
            }
        }

        private RelayCommand _insertImageStampCommand;

        /// <summary>
        /// Gets the InsertImageStampCommand.
        /// </summary>
        public RelayCommand InsertImageStampCommand
        {
            get
            {
                return _insertImageStampCommand
                    ?? (_insertImageStampCommand = new RelayCommand(
                                          () =>
                                          {
                                              // Configure open file dialog box
                                              Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                                              dlg.Filter = "Images|*.png;*.jpg;*.jpeg;*.gif"; // Filter files by extension

                                              // Show open file dialog box
                                              Nullable<bool> result = dlg.ShowDialog();

                                              // Process open file dialog box results
                                              if (result == true)
                                              {
                                                  // Open document
                                                  string filename = dlg.FileName;
                                                  CLPImageStamp image = new CLPImageStamp(filename);
                                                  CLPService.AddPageObjectToPage(image);
                                              }
                                          }));
            }
        }

        #endregion //Insert Commands

        private RelayCommand _submitPageCommand;

        /// <summary>
        /// Gets the SubmitPageCommand.
        /// </summary>
        public RelayCommand SubmitPageCommand
        {
            get
            {
                return _submitPageCommand
                    ?? (_submitPageCommand = new RelayCommand(
                                          () =>
                                          {
                                              AppMessages.RequestCurrentDisplayedPage.Send( (callbackMessage) =>
                                                  {
                                                      CLPService.SubmitPage(callbackMessage);
                                                  });
                                          }));
            }
        }

        private RelayCommand _exitCommand;

        /// <summary>
        /// Gets the ExitCommand.
        /// </summary>
        public RelayCommand ExitCommand
        {
            get
            {
                return _exitCommand
                    ?? (_exitCommand = new RelayCommand(
                                          () =>
                                          {
                                              if (MessageBox.Show("Are you sure you want to exit?",
                                "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                              {
                                                  CLPService.Exit();
                                              }
                                          }));
            }
        }
        private RelayCommand _undoCommand;

        /// <summary>
        /// Gets the UndoCommand.
        /// </summary>
        public RelayCommand UndoCommand
        {
            get
            {
                return _undoCommand
                    ?? (_undoCommand = new RelayCommand(
                                          () =>
                                          {
                                              CLPHistoryItem historyItem = new CLPHistoryItem(null, "UNDO");
                                              AppMessages.UpdateCLPHistory.Send(historyItem);
                                          }));
            }
        }
        private RelayCommand _redoCommand;

        /// <summary>
        /// Gets the RedoCommand.
        /// </summary>
        public RelayCommand RedoCommand
        {
            get
            {
                return _redoCommand
                    ?? (_redoCommand = new RelayCommand(
                                          () =>
                                          {
                                              CLPHistoryItem historyItem = new CLPHistoryItem(null, "REDO");
                                              AppMessages.UpdateCLPHistory.Send(historyItem);
                                          }));
            }
        }

        #endregion //Commands
    }
}
