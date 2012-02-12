﻿using System;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.ViewModels.Workspaces;
using Classroom_Learning_Partner.Model;
using System.Windows.Media;
using Classroom_Learning_Partner.Views.PageObjects;
using System.Windows.Ink;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Windows.Controls.Ribbon;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Classroom_Learning_Partner.Model.CLPPageObjects;

namespace Classroom_Learning_Partner.ViewModels
{

    public class MainWindowViewModel : ViewModelBase
    {
        public const string clpText = "Classroom Learning Partner - ";

        /// <summary>
        /// Initializes a new instance of the MainWindowViewModel class.
        /// </summary>
        public MainWindowViewModel()
        {
            CLPService = new CLPServiceAgent();

            //MainWindow Content
            SetTitleBarText("Starting Up");
            IsAuthoring = false;

            //Commands
            SetInstructorCommand = new Command(OnSetInstructorCommandExecute);
            SetStudentCommand = new Command(OnSetStudentCommandExecute);
            SetProjectorCommand = new Command(OnSetProjectorCommandExecute);
            SetServerCommand = new Command(OnSetServerCommandExecute);

            //Ribbon Content
            CanSendToTeacher = true;
            DrawingAttributes = new DrawingAttributes();
            DrawingAttributes.Height = PEN_RADIUS;
            DrawingAttributes.Width = PEN_RADIUS;
            DrawingAttributes.Color = Colors.Black;
            DrawingAttributes.FitToCurve = true;
            EditingMode = InkCanvasEditingMode.Ink;

            _currentColorButton.Background = new SolidColorBrush(Colors.Black);

            foreach (var color in _colors)
            {
                _fontColors.Add(new SolidColorBrush(color));
            }

            CurrentFontColor = new SolidColorBrush(Colors.Black);
            //Steve - Can this be done in XAML? And switched to toggle button?
            RecordImage = new Uri("..\\Images\\mic_start.png", UriKind.Relative);

            AuthoringTabVisibility = Visibility.Hidden;
            InstructorVisibility = Visibility.Hidden;
            StudentVisibility = Visibility.Hidden;
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

        #region NonRibbon Items

        #region Bindings

        /// <summary>
        /// Gets or sets the Title Bar text of the window.
        /// </summary>
        public string TitleBarText
        {
            get { return GetValue<string>(TitleBarTextProperty); }
            private set { SetValue(TitleBarTextProperty, value); }
        }

        /// <summary>
        /// Register the TitleBarText property so it is known in the class.
        /// </summary>
        public static readonly PropertyData TitleBarTextProperty = RegisterProperty("TitleBarText", typeof(string));

        /// <summary>
        /// Gets or sets the current Workspace.
        /// </summary>
        public IWorkspaceViewModel SelectedWorkspace
        {
            get { return GetValue<IWorkspaceViewModel>(SelectedWorkspaceProperty); }
            set { SetValue(SelectedWorkspaceProperty, value); }
        }

        /// <summary>
        /// Register the SelectedWorkspace property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SelectedWorkspaceProperty = RegisterProperty("SelectedWorkspace", typeof(IWorkspaceViewModel));

        #endregion //Bindings

        #region Properties

        /// <summary>
        /// Gets or sets the Authoring flag.
        /// </summary>
        public bool IsAuthoring
        {
            get { return GetValue<bool>(IsAuthoringProperty); }
            set { SetValue(IsAuthoringProperty, value); }
        }

        /// <summary>
        /// Register the IsAuthoring property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsAuthoringProperty = RegisterProperty("IsAuthoring", typeof(bool));

        #endregion //Properties

        #region Methods

        //Sets the text in the title bar of the window, endText can add optional information
        public void SetTitleBarText(string endText)
        {
            string isOnline = "Disconnected";
            if (App.Peer.OnlineStatusHandler.IsOnline)
            {
                isOnline = "Connected";
            }
            TitleBarText = clpText + "Logged In As: " + App.Peer.UserName + " Connection Status: " + isOnline + " " + endText;
        }

        public void SetWorkspace()
        {
            IsAuthoring = false;

            switch (App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    SelectedWorkspace = new ServerWorkspaceViewModel();
                    break;
                case App.UserMode.Instructor:
                    SelectedWorkspace = new NotebookChooserWorkspaceViewModel();
                    break;
                case App.UserMode.Projector:
                    SelectedWorkspace = new NotebookChooserWorkspaceViewModel();
                    break;
                case App.UserMode.Student:
                    SelectedWorkspace = new UserLoginWorkspaceViewModel();
                    break;
            }
        }

        #endregion //Methods

        #region Commands

        /// <summary>
        /// Gets the SetInstructorCommand command.
        /// </summary>
        public Command SetInstructorCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetInstructorCommand command is executed.
        /// </summary>
        private void OnSetInstructorCommandExecute()
        {
            App.CurrentUserMode = App.UserMode.Instructor;
            SetWorkspace();
        }

        /// <summary>
        /// Gets the SetStudentCommand command.
        /// </summary>
        public Command SetStudentCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetStudentCommand command is executed.
        /// </summary>
        private void OnSetStudentCommandExecute()
        {
            App.CurrentUserMode = App.UserMode.Student;
            SetWorkspace();
        }

        /// <summary>
        /// Gets the SetProjectorCommand command.
        /// </summary>
        public Command SetProjectorCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetProjectorCommand command is executed.
        /// </summary>
        private void OnSetProjectorCommandExecute()
        {
            App.CurrentUserMode = App.UserMode.Projector;
            SetWorkspace();
        }

        /// <summary>
        /// Gets the SetServerCommand command.
        /// </summary>
        public Command SetServerCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetServerCommand command is executed.
        /// </summary>
        private void OnSetServerCommandExecute()
        {
            App.CurrentUserMode = App.UserMode.Server;
            SetWorkspace();
        }

        #endregion //Commands

        #endregion //NonRibbon Items

        #region Ribbon

        public const double PEN_RADIUS = 2;
        public const double MARKER_RADIUS = 5;
        public const double ERASER_RADIUS = 5;

        #region Properties

        //Steve - Dont' want Views in ViewModels, can this be fixed?
        public CLPTextBoxView LastFocusedTextBox = null;

        /// <summary>
        /// Gets the DrawingAttributes of the Ribbon.
        /// </summary>
        public DrawingAttributes DrawingAttributes
        {
            get { return GetValue<DrawingAttributes>(DrawingAttributesProperty); }
            private set { SetValue(DrawingAttributesProperty, value); }
        }

        /// <summary>
        /// Register the DrawingAttributes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DrawingAttributesProperty = RegisterProperty("DrawingAttributes", typeof(DrawingAttributes));

        /// <summary>
        /// Gets or sets the EditingMode for the InkCanvas.
        /// </summary>
        public InkCanvasEditingMode EditingMode
        {
            get { return GetValue<InkCanvasEditingMode>(EditingModeProperty); }
            set { SetValue(EditingModeProperty, value); }
        }

        /// <summary>
        /// Register the EditingMode property so it is known in the class.
        /// </summary>
        public static readonly PropertyData EditingModeProperty = RegisterProperty("EditingMode", typeof(InkCanvasEditingMode));

        #endregion //Properties

        #region Bindings

        #region Convert to XAMLS?

        //Steve - Switch these visibility tags into XAML getters/setters
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Visibility AuthoringTabVisibility
        {
            get { return GetValue<Visibility>(AuthoringTabVisibilityProperty); }
            set { SetValue(AuthoringTabVisibilityProperty, value); }
        }

        /// <summary>
        /// Register the AuthoringTabVisibility property so it is known in the class.
        /// </summary>
        public static readonly PropertyData AuthoringTabVisibilityProperty = RegisterProperty("AuthoringTabVisibility", typeof(Visibility));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Visibility InstructorVisibility
        {
            get { return GetValue<Visibility>(InstructorVisibilityProperty); }
            set { SetValue(InstructorVisibilityProperty, value); }
        }

        /// <summary>
        /// Register the InstructorVisibility property so it is known in the class.
        /// </summary>
        public static readonly PropertyData InstructorVisibilityProperty = RegisterProperty("InstructorVisibility", typeof(Visibility));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Visibility StudentVisibility
        {
            get { return GetValue<Visibility>(StudentVisibilityProperty); }
            set { SetValue(StudentVisibilityProperty, value); }
        }

        /// <summary>
        /// Register the StudentVisibility property so it is known in the class.
        /// </summary>
        public static readonly PropertyData StudentVisibilityProperty = RegisterProperty("StudentVisibility", typeof(Visibility));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Uri RecordImage
        {
            get { return GetValue<Uri>(RecordImageProperty); }
            set { SetValue(RecordImageProperty, value); }
        }

        /// <summary>
        /// Register the RecordImage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData RecordImageProperty = RegisterProperty("RecordImage", typeof(Uri));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public RibbonButton CurrentColorButton
        {
            get { return GetValue<RibbonButton>(CurrentColorButtonProperty); }
            set { SetValue(CurrentColorButtonProperty, value); }
        }

        /// <summary>
        /// Register the CurrentColorButton property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CurrentColorButtonProperty = RegisterProperty("CurrentColorButton", typeof(RibbonButton));

        #endregion //Convert to XAMLS?

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

        private FontFamily _currentFontFamily = new FontFamily("Times New Roman");

        /// <summary>
        /// Sets and gets the CurrentFontFamily property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public FontFamily CurrentFontFamily
        {
            get
            {
                return _currentFontFamily;
            }

            set
            {
                if (_currentFontFamily == value)
                {
                    return;
                }

                _currentFontFamily = value;
                RaisePropertyChanged(CurrentFontFamilyPropertyName);
                Console.WriteLine("fontfamily changed");
                AppMessages.UpdateFont.Send(-1, _currentFontFamily, null);
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
                Console.WriteLine("fontsize changed");
                AppMessages.UpdateFont.Send(_currentFontSize, null, null);
            }
        }

        private List<Color> _colors = new List<Color>() { Colors.Black, Colors.Red, Colors.Blue, Colors.Purple, Colors.Brown, Colors.Green };
        private ObservableCollection<Brush> _fontColors = new ObservableCollection<Brush>();

        public ObservableCollection<Brush> FontColors
        {
            get
            {
                return _fontColors;
            }
        }

        /// <summary>
        /// The <see cref="CurrentFontColor" /> property's name.
        /// </summary>
        public const string CurrentFontColorPropertyName = "CurrentFontColor";

        private Brush _currentFontColor;

        /// <summary>
        /// Sets and gets the CurrentFontColor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Brush CurrentFontColor
        {
            get
            {
                return _currentFontColor;
            }

            set
            {
                if (_currentFontColor == value)
                {
                    return;
                }

                _currentFontColor = value;
                RaisePropertyChanged(CurrentFontColorPropertyName);
                Console.WriteLine("fontcolor changed");
                AppMessages.UpdateFont.Send(-1, null, _currentFontColor);
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
                                              if (EditingMode == InkCanvasEditingMode.None)
                                              {
                                                  AppMessages.SetLaserPointerMode.Send(false);
                                                  AppMessages.SetSnapTileMode.Send(false);
                                              }
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
        /// Gets the SetLaserPointerModeCommand.
        /// </summary>
        public RelayCommand SetLaserPointerModeCommand
        {
            get
            {
                return _SetLaserPointerModeCommand
                    ?? (_SetLaserPointerModeCommand = new RelayCommand(
                                          () =>
                                          {
                                              EditingMode = InkCanvasEditingMode.None;
                                              AppMessages.ChangeInkMode.Send(InkCanvasEditingMode.None);
                                              AppMessages.SetLaserPointerMode.Send(true);
                                          }));
            }
        }

        private RelayCommand _SetSnapTileCommand;

        /// <summary>
        /// Gets the SetSnapTileCommand.
        /// </summary>
        public RelayCommand SetSnapTileCommand
        {
            get
            {
                return _SetSnapTileCommand
                    ?? (_SetSnapTileCommand = new RelayCommand(
                                          () =>
                                          {

                                              EditingMode = InkCanvasEditingMode.None;
                                              AppMessages.ChangeInkMode.Send(InkCanvasEditingMode.None);
                                              AppMessages.SetSnapTileMode.Send(true);
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
                                              CLPService.DistributeNotebook(App.CurrentNotebookViewModel, App.Peer.UserName);
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
                                              FixedDocument doc = new FixedDocument();
                                              doc.DocumentPaginator.PageSize = new Size(96 * 11, 96 * 8.5);


                                              if (App.CurrentUserMode == App.UserMode.Instructor)
                                              {

                                              }
                                              //foreach page here
                                              //foreach (var pageView in A)
                                              //{

                                              //}
                                              int i = 0;
                                              foreach (CLPPageViewModel pageVM in App.CurrentNotebookViewModel.PageViewModels)
                                              {
                                                  PageContent pageContent = new PageContent();
                                                  FixedPage fixedPage = new FixedPage();

                                                  CLPPagePreviewView currentPage = new CLPPagePreviewView();
                                                  currentPage.DataContext = pageVM;
                                                  currentPage.UpdateLayout();
                                                  //currentPage.Visibility = Visibility.Hidden;

                                                  RenderTargetBitmap bmp = new RenderTargetBitmap((int)(96 * 8.5), 96 * 11, 96d, 96d, PixelFormats.Pbgra32); //new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                                                  bmp.Render(currentPage.MainInkCanvas);
                                                  PngBitmapEncoder encoder = new PngBitmapEncoder();
                                                  encoder.Frames.Add(BitmapFrame.Create(bmp));
                                                  using (Stream s = File.Create(@"C:\" + i.ToString() + ".png"))
                                                  {
                                                      encoder.Save(s);
                                                  }
                                                  i++;

                                                  //Create first page of document
                                                  RotateTransform rotate = new RotateTransform(90.0);
                                                  TranslateTransform translate = new TranslateTransform(816 + 2, -2);
                                                  TransformGroup transform = new TransformGroup();
                                                  transform.Children.Add(rotate);
                                                  transform.Children.Add(translate);
                                                  currentPage.RenderTransform = transform;





                                                  fixedPage.Children.Add(currentPage);
                                                  ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
                                                  doc.Pages.Add(pageContent);
                                              }

                                              //Save the document
                                              string filename = App.CurrentNotebookViewModel.Notebook.NotebookName + ".xps";
                                              string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\" + filename;
                                              if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\"))
                                              {
                                                  Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\");
                                              }
                                              if (File.Exists(path))
                                              {
                                                  File.Delete(path);
                                              }


                                              XpsDocument xpsd = new XpsDocument(path, FileAccess.ReadWrite);

                                              XpsDocumentWriter xw = XpsDocument.CreateXpsDocumentWriter(xpsd);
                                              xw.Write(doc);
                                              xpsd.Close();
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

        private RelayCommand _insertBlankStampCommand;

        /// <summary>
        /// Gets the InsertBlankStampCommand.
        /// </summary>
        public RelayCommand InsertBlankStampCommand
        {
            get
            {
                return _insertBlankStampCommand
                    ?? (_insertBlankStampCommand = new RelayCommand(
                                          () =>
                                          {
                                              CLPBlankStamp stamp = new CLPBlankStamp();
                                              CLPService.AddPageObjectToPage(stamp);
                                          }));
            }
        }

        #endregion //Insert Commands

        #region Display Commands

        private RelayCommand _sendDisplayToProjectorCommand;

        /// <summary>
        /// Gets the SendDisplayToProjectorCommand.
        /// </summary>
        public RelayCommand SendDisplayToProjectorCommand
        {
            get
            {
                return _sendDisplayToProjectorCommand
                    ?? (_sendDisplayToProjectorCommand = new RelayCommand(
                                          () =>
                                          {
                                              if (App.Peer.Channel != null)
                                              {
                                                  if ((App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).Display is LinkedDisplayViewModel)
                                                  {
                                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).LinkedDisplay.IsOnProjector = true;
                                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay.IsOnProjector = false;
                                                      App.Peer.Channel.SwitchProjectorDisplay("LinkedDisplay", new List<string>());
                                                  }
                                                  else
                                                  {
                                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).LinkedDisplay.IsOnProjector = false;
                                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay.IsOnProjector = true;
                                                      List<string> pageList = new List<string>();
                                                      foreach (var page in (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay.DisplayPages)
                                                      {
                                                          pageList.Add(ObjectSerializer.ToString(page.Page));
                                                      }

                                                      App.Peer.Channel.SwitchProjectorDisplay("GridDisplay", pageList);
                                                  }
                                              }
                                          }));
            }
        }

        private RelayCommand _switchToLinkedDisplayCommand;

        /// <summary>
        /// Gets the SwitchToLinkedDisplayCommand.
        /// </summary>
        public RelayCommand SwitchToLinkedDisplayCommand
        {
            get
            {
                return _switchToLinkedDisplayCommand
                    ?? (_switchToLinkedDisplayCommand = new RelayCommand(
                                          () =>
                                          {
                                              (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).Display = (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).LinkedDisplay;
                                              (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).LinkedDisplay.IsActive = true;
                                              (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay.IsActive = false;
                                          }));
            }
        }

        private RelayCommand _createNewGridDisplayCommand;

        /// <summary>
        /// Gets the CreateNewGridDisplayCommand.
        /// </summary>
        public RelayCommand CreateNewGridDisplayCommand
        {
            get
            {
                return _createNewGridDisplayCommand
                    ?? (_createNewGridDisplayCommand = new RelayCommand(
                                          () =>
                                          {
                                              (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).Display = (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay;
                                              (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).LinkedDisplay.IsActive = false;
                                              (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay.IsActive = true;
                                          }));
            }
        }

        #endregion //Display Commands

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

                                              IsSending = true;
                                              Timer timer = new Timer();
                                              timer.Interval = 1000;
                                              timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                                              timer.Enabled = true;

                                              if (CanSendToTeacher)
                                              {
                                                  Console.WriteLine("actual send");
                                                  AppMessages.RequestCurrentDisplayedPage.Send((clpPageViewModel) =>
                                                  {
                                                      CLPService.SubmitPage(clpPageViewModel);
                                                  });
                                              }
                                              CanSendToTeacher = false;
                                          }));
            }
        }

        public bool CanSendToTeacher { get; set; }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Timer timer = sender as Timer;
            timer.Stop();
            timer.Elapsed -= timer_Elapsed;
            IsSending = false;
        }

        public const string IsSendingPropertyName = "IsSending";
        private bool _isSending;
        public bool IsSending
        {
            get
            {
                return _isSending;
            }
            set
            {
                _isSending = value;
                RaisePropertyChanged(IsSendingPropertyName);
                RaisePropertyChanged(SendButtonPropertyName);
                RaisePropertyChanged(IsSentInfoVisibilityPropertyName);
            }
        }

        public const string SendButtonPropertyName = "SendButtonVisibility";
        public Visibility SendButtonVisibility
        {
            get { return (IsSending ? Visibility.Collapsed : Visibility.Visible); }
        }

        public const string IsSentInfoVisibilityPropertyName = "IsSentInfoVisibility";
        public Visibility IsSentInfoVisibility
        {
            get { return (IsSending ? Visibility.Visible : Visibility.Collapsed); }
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
        #region HistoryCommands
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
                                              AppMessages.RequestCurrentDisplayedPage.Send((clpPageViewModel) =>
                                              {
                                                  clpPageViewModel.HistoryVM.undo();
                                              });
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
                                              AppMessages.RequestCurrentDisplayedPage.Send((clpPageViewModel) =>
                                              {
                                                  clpPageViewModel.HistoryVM.redo();
                                              });
                                          }));
            }
        }

        private RelayCommand _audioCommand;
        private bool recording = false;
        public RelayCommand AudioCommand
        {
            get
            {
                return _audioCommand
                    ?? (_audioCommand = new RelayCommand(
                                          () =>
                                          {
                                              AppMessages.Audio.Send("start");
                                              recording = !recording;
                                              if (recording)
                                              {
                                                  RecordImage = new Uri("..\\Images\\mic_stop.png", UriKind.Relative);
                                              }
                                              else
                                              {
                                                  RecordImage = new Uri("..\\Images\\mic_start.png", UriKind.Relative);
                                              }
                                          }));
            }
        }
        private RelayCommand _playAudioCommand;
        public RelayCommand PlayAudioCommand
        {
            get
            {
                return _playAudioCommand
                    ?? (_playAudioCommand = new RelayCommand(
                                          () =>
                                          {
                                              AppMessages.Audio.Send("play");


                                          }));
            }
        }
        private RelayCommand _enablePlaybackCommand;
        public RelayCommand EnablePlaybackCommand
        {
            get
            {
                return _enablePlaybackCommand
                    ?? (_enablePlaybackCommand = new RelayCommand(
                                          () =>
                                          {
                                              AppMessages.ChangePlayback.Send(true);

                                          }));
            }
        }
        #endregion //History Commands
        #endregion //Commands

        #endregion //Ribbon
    }
}