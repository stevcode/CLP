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
                                          },
                                          () =>
                                          {
                                              return App.CurrentUserMode == App.UserMode.Instructor;
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
                                              CLPService.AddPage(new CLPPage());
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
                                              //change this to send uniqueID
                                              CLPService.RemovePage("blah");
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

        #endregion //Commands
    }
}