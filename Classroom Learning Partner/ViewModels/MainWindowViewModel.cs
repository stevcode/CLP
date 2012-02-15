using System;
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
using System.Timers;
using Classroom_Learning_Partner.ViewModels.Displays;

namespace Classroom_Learning_Partner.ViewModels
{

    public class MainWindowViewModel : ViewModelBase
    {
        public const string clpText = "Classroom Learning Partner - ";

        /// <summary>
        /// Initializes a new instance of the MainWindowViewModel class.
        /// </summary>
        public MainWindowViewModel()
            : base()
        {
            Console.WriteLine(Title + " created");
            //MainWindow Content
            SetTitleBarText("Starting Up");
            IsAuthoring = false;
            OpenNotebooks = new ObservableCollection<CLPNotebook>();
            CurrentNotebookIndex = -1;

            //MainWindow Commands
            SetInstructorCommand = new Command(OnSetInstructorCommandExecute);
            SetStudentCommand = new Command(OnSetStudentCommandExecute);
            SetProjectorCommand = new Command(OnSetProjectorCommandExecute);
            SetServerCommand = new Command(OnSetServerCommandExecute);

            //Ribbon Content
            CanSendToTeacher = true;
            IsSending = false;
            DrawingAttributes = new DrawingAttributes();
            DrawingAttributes.Height = PEN_RADIUS;
            DrawingAttributes.Width = PEN_RADIUS;
            DrawingAttributes.Color = Colors.Black;
            DrawingAttributes.FitToCurve = true;
            EditingMode = InkCanvasEditingMode.Ink;

            CurrentColorButton = new RibbonButton();
            CurrentColorButton.Background = new SolidColorBrush(Colors.Black);

            foreach (var color in _colors)
            {
                _fontColors.Add(new SolidColorBrush(color));
            }

            CurrentFontColor = new SolidColorBrush(Colors.Black);
            //Steve - Set to New Times Roman
            CurrentFontFamily = Fonts[0];
            CurrentFontSize = 24;
            //Steve - Can this be done in XAML? And switched to toggle button?
            RecordImage = new Uri("..\\Images\\mic_start.png", UriKind.Relative);

            AuthoringTabVisibility = Visibility.Collapsed;
            InstructorVisibility = Visibility.Collapsed;
            StudentVisibility = Visibility.Collapsed;
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

            //Ribbon Commands
            SetPenCommand = new Command(OnSetPenCommandExecute);
            SetMarkerCommand = new Command(OnSetMarkerCommandExecute);
            SetEraserCommand = new Command(OnSetEraserCommandExecute);
            SetStrokeEraserCommand = new Command(OnSetStrokeEraserCommandExecute);
            SetPenColorCommand = new Command<RibbonButton>(OnSetPenColorCommandExecute);

            NewNotebookCommand = new Command(OnNewNotebookCommandExecute);
            OpenNotebookCommand = new Command(OnOpenNotebookCommandExecute);
            EditNotebookCommand = new Command(OnEditNotebookCommandExecute);
            DoneEditingNotebookCommand = new Command(OnDoneEditingNotebookCommandExecute);
            SaveNotebookCommand = new Command(OnSaveNotebookCommandExecute);
            SaveAllNotebooksCommand = new Command(OnSaveAllNotebooksCommandExecute);

            AddNewPageCommand = new Command(OnAddNewPageCommandExecute);
            DeletePageCommand = new Command(OnDeletePageCommandExecute);
            CopyPageCommand = new Command(OnCopyPageCommandExecute);

            InsertTextBoxCommand = new Command(OnInsertTextBoxCommandExecute);
            InsertImageCommand = new Command(OnInsertImageCommandExecute);

            InsertSquareShapeCommand = new Command(OnInsertSquareShapeCommandExecute);


            SubmitPageCommand = new Command(OnSubmitPageCommandExecute);
            ExitCommand = new Command(OnExitCommandExecute);
        }

        public override string Title { get { return "MainWindowVM"; } }

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
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<CLPNotebook> OpenNotebooks
        {
            get { return GetValue<ObservableCollection<CLPNotebook>>(OpenNotebooksProperty); }
            set { SetValue(OpenNotebooksProperty, value); }
        }

        /// <summary>
        /// Register the OpenNotebooks property so it is known in the class.
        /// </summary>
        public static readonly PropertyData OpenNotebooksProperty = RegisterProperty("OpenNotebooks", typeof(ObservableCollection<CLPNotebook>));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int CurrentNotebookIndex
        {
            get { return GetValue<int>(CurrentNotebookIndexProperty); }
            set { SetValue(CurrentNotebookIndexProperty, value); }
        }

        /// <summary>
        /// Register the CurrentNotebook property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CurrentNotebookIndexProperty = RegisterProperty("CurrentNotebookIndex", typeof(int));

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
            string userName = "none";
            if (App.Peer != null)
            {
                if (App.Peer.OnlineStatusHandler != null)
                {
                    if (App.Peer.OnlineStatusHandler.IsOnline)
                    {
                        isOnline = "Connected";
                    }
                }

                userName = App.Peer.UserName;
            }

            TitleBarText = clpText + "Logged In As: " + userName + " Connection Status: " + isOnline + " " + endText;
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
        /// Gets or sets the property value.
        /// </summary>
        public FontFamily CurrentFontFamily
        {
            get { return GetValue<FontFamily>(CurrentFontFamilyProperty); }
            set
            {
                SetValue(CurrentFontFamilyProperty, value);
                Console.WriteLine("fontfamily changed");
                AppMessages.UpdateFont.Send(-1, CurrentFontFamily, null);
            }
        }

        /// <summary>
        /// Register the CurrentFontFamily property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CurrentFontFamilyProperty = RegisterProperty("CurrentFontFamily", typeof(FontFamily));

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
        /// Gets or sets the property value.
        /// </summary>
        public double CurrentFontSize
        {
            get { return GetValue<double>(CurrentFontSizeProperty); }
            set
            {
                SetValue(CurrentFontSizeProperty, value);
                Console.WriteLine("fontsize changed");
                AppMessages.UpdateFont.Send(CurrentFontSize, null, null);
            }
        }

        /// <summary>
        /// Register the CurrentFontSize property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CurrentFontSizeProperty = RegisterProperty("CurrentFontSize", typeof(double));

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
        /// Gets or sets the property value.
        /// </summary>
        public Brush CurrentFontColor
        {
            get { return GetValue<Brush>(CurrentFontColorProperty); }
            set
            {
                SetValue(CurrentFontColorProperty, value);
                Console.WriteLine("fontcolor changed");
                AppMessages.UpdateFont.Send(-1, null, CurrentFontColor);
            }
        }

        /// <summary>
        /// Register the CurrentFontColor property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CurrentFontColorProperty = RegisterProperty("CurrentFontColor", typeof(Brush));

        #endregion //TextBox

        #endregion //Bindings

        #region Commands

        #region Pen Commands

        /// <summary>
        /// Gets the SetPenCommand command.
        /// </summary>
        public Command SetPenCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetPenCommand command is executed.
        /// </summary>
        private void OnSetPenCommandExecute()
        {
            DrawingAttributes.Height = PEN_RADIUS;
            DrawingAttributes.Width = PEN_RADIUS;
            EditingMode = InkCanvasEditingMode.Ink;
        }

        /// <summary>
        /// Gets the SetMarkerCommand command.
        /// </summary>
        public Command SetMarkerCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetMarkerCommand command is executed.
        /// </summary>
        private void OnSetMarkerCommandExecute()
        {
            DrawingAttributes.Height = MARKER_RADIUS;
            DrawingAttributes.Width = MARKER_RADIUS;
            EditingMode = InkCanvasEditingMode.Ink;
        }

        /// <summary>
        /// Gets the SetEraserCommand command.
        /// </summary>
        public Command SetEraserCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetEraserCommand command is executed.
        /// </summary>
        private void OnSetEraserCommandExecute()
        {
            DrawingAttributes.Height = ERASER_RADIUS;
            DrawingAttributes.Width = ERASER_RADIUS;
            EditingMode = InkCanvasEditingMode.EraseByPoint;
        }

                /// <summary>
        /// Gets the SetStrokeEraserCommand command.
        /// </summary>
        public Command SetStrokeEraserCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetStrokeEraserCommand command is executed.
        /// </summary>
        private void OnSetStrokeEraserCommandExecute()
        {
            EditingMode = InkCanvasEditingMode.EraseByStroke;
        }

        /// <summary>
        /// Gets the SetPenColorCommand command.
        /// </summary>
        public Command<RibbonButton> SetPenColorCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetPenColorCommand command is executed.
        /// </summary>
        private void OnSetPenColorCommandExecute(RibbonButton button)
        {
            CurrentColorButton = button as RibbonButton;
            DrawingAttributes.Color = (CurrentColorButton.Background as SolidColorBrush).Color;
        }

        //private RelayCommand _SetSnapTileCommand;

        ///// <summary>
        ///// Gets the SetSnapTileCommand.
        ///// </summary>
        //public RelayCommand SetSnapTileCommand
        //{
        //    get
        //    {
        //        return _SetSnapTileCommand
        //            ?? (_SetSnapTileCommand = new RelayCommand(
        //                                  () =>
        //                                  {

        //                                      EditingMode = InkCanvasEditingMode.None;
        //                                      AppMessages.ChangeInkMode.Send(InkCanvasEditingMode.None);
        //                                      AppMessages.SetSnapTileMode.Send(true);
        //                                  }));
        //    }
        //}

        #endregion //Pen Commands

        #region Notebook Commands

        /// <summary>
        /// Gets the NewNotebookCommand command.
        /// </summary>
        public Command NewNotebookCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the NewNotebookCommand command is executed.
        /// </summary>
        private void OnNewNotebookCommandExecute()
        {
            CLPServiceAgent.Instance.OpenNewNotebook();
        }

        /// <summary>
        /// Gets the OpenNotebookCommand command.
        /// </summary>
        public Command OpenNotebookCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the OpenNotebookCommand command is executed.
        /// </summary>
        private void OnOpenNotebookCommandExecute()
        {
            SelectedWorkspace = new NotebookChooserWorkspaceViewModel();
        }

        /// <summary>
        /// Gets the EditNotebookCommand command.
        /// </summary>
        public Command EditNotebookCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the EditNotebookCommand command is executed.
        /// </summary>
        private void OnEditNotebookCommandExecute()
        {
            IsAuthoring = true;
        }

                /// <summary>
        /// Gets the DoneEditingNotebookCommand command.
        /// </summary>
        public Command DoneEditingNotebookCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the DoneEditingNotebookCommand command is executed.
        /// </summary>
        private void OnDoneEditingNotebookCommandExecute()
        {
            IsAuthoring = false;
            //CLPService.DistributeNotebook(App.CurrentNotebookViewModel, App.Peer.UserName);
        }

        /// <summary>
        /// Gets the SaveNotebookCommand command.
        /// </summary>
        public Command SaveNotebookCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SaveNotebookCommand command is executed.
        /// </summary>
        private void OnSaveNotebookCommandExecute()
        {
            CLPServiceAgent.Instance.SaveNotebook(App.MainWindowViewModel.OpenNotebooks[App.MainWindowViewModel.CurrentNotebookIndex]);
        }

        /// <summary>
        /// Gets the SaveAllNotebooksCommand command.
        /// </summary>
        public Command SaveAllNotebooksCommand { get; private set; }

        // TODO: Move code below to constructor

        // TODO: Move code above to constructor

        /// <summary>
        /// Method to invoke when the SaveAllNotebooksCommand command is executed.
        /// </summary>
        private void OnSaveAllNotebooksCommandExecute()
        {
            foreach (var notebook in App.MainWindowViewModel.OpenNotebooks)
            {
                CLPServiceAgent.Instance.SaveNotebook(notebook);
            }
        }

        //private RelayCommand _convertToXPSCommand;

        ///// <summary>
        ///// Gets the ConvertToXPSCommand.
        ///// </summary>
        //public RelayCommand ConvertToXPSCommand
        //{
        //    get
        //    {
        //        return _convertToXPSCommand
        //            ?? (_convertToXPSCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      FixedDocument doc = new FixedDocument();
        //                                      doc.DocumentPaginator.PageSize = new Size(96 * 11, 96 * 8.5);


        //                                      if (App.CurrentUserMode == App.UserMode.Instructor)
        //                                      {

        //                                      }
        //                                      //foreach page here
        //                                      //foreach (var pageView in A)
        //                                      //{

        //                                      //}
        //                                      int i = 0;
        //                                      foreach (CLPPageViewModel pageVM in App.CurrentNotebookViewModel.PageViewModels)
        //                                      {
        //                                          PageContent pageContent = new PageContent();
        //                                          FixedPage fixedPage = new FixedPage();

        //                                          CLPPagePreviewView currentPage = new CLPPagePreviewView();
        //                                          currentPage.DataContext = pageVM;
        //                                          currentPage.UpdateLayout();
        //                                          //currentPage.Visibility = Visibility.Hidden;

        //                                          RenderTargetBitmap bmp = new RenderTargetBitmap((int)(96 * 8.5), 96 * 11, 96d, 96d, PixelFormats.Pbgra32); //new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        //                                          bmp.Render(currentPage.MainInkCanvas);
        //                                          PngBitmapEncoder encoder = new PngBitmapEncoder();
        //                                          encoder.Frames.Add(BitmapFrame.Create(bmp));
        //                                          using (Stream s = File.Create(@"C:\" + i.ToString() + ".png"))
        //                                          {
        //                                              encoder.Save(s);
        //                                          }
        //                                          i++;

        //                                          //Create first page of document
        //                                          RotateTransform rotate = new RotateTransform(90.0);
        //                                          TranslateTransform translate = new TranslateTransform(816 + 2, -2);
        //                                          TransformGroup transform = new TransformGroup();
        //                                          transform.Children.Add(rotate);
        //                                          transform.Children.Add(translate);
        //                                          currentPage.RenderTransform = transform;





        //                                          fixedPage.Children.Add(currentPage);
        //                                          ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
        //                                          doc.Pages.Add(pageContent);
        //                                      }

        //                                      //Save the document
        //                                      string filename = App.CurrentNotebookViewModel.Notebook.NotebookName + ".xps";
        //                                      string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\" + filename;
        //                                      if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\"))
        //                                      {
        //                                          Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\");
        //                                      }
        //                                      if (File.Exists(path))
        //                                      {
        //                                          File.Delete(path);
        //                                      }


        //                                      XpsDocument xpsd = new XpsDocument(path, FileAccess.ReadWrite);

        //                                      XpsDocumentWriter xw = XpsDocument.CreateXpsDocumentWriter(xpsd);
        //                                      xw.Write(doc);
        //                                      xpsd.Close();
        //                                  }));
        //    }
        //}

        #endregion //Notebook Commands

        #region Page Commands

        /// <summary>
        /// Gets the AddPageCommand command.
        /// </summary>
        public Command AddNewPageCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the AddPageCommand command is executed.
        /// </summary>
        private void OnAddNewPageCommandExecute()
        {
            //Steve - clpserviceagent
            int index = OpenNotebooks[CurrentNotebookIndex].Pages.IndexOf(((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page);
            index++;
            CLPPage page = new CLPPage();
            OpenNotebooks[CurrentNotebookIndex].InsertPageAt(index, page);
            //(SelectedWorkspace as NotebookWorkspaceViewModel).SideBar.Pages.Insert(index, page);
        }

        /// <summary>
        /// Gets the DeletePageCommand command.
        /// </summary>
        public Command DeletePageCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the DeletePageCommand command is executed.
        /// </summary>
        private void OnDeletePageCommandExecute()
        {
            //Steve - clpserviceagent
            int index = OpenNotebooks[CurrentNotebookIndex].Pages.IndexOf(((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page);
            if (index != -1)
            {
                
                OpenNotebooks[CurrentNotebookIndex].RemovePageAt(index);
                //(SelectedWorkspace as NotebookWorkspaceViewModel).SideBar.Pages.RemoveAt(index);
            }
        }

        /// <summary>
        /// Gets the CopyPageCommand command.
        /// </summary>
        public Command CopyPageCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the CopyPageCommand command is executed.
        /// </summary>
        private void OnCopyPageCommandExecute()
        {
            // TODO: Handle command logic here
        }

        #endregion //Page Commands

        #region Insert Commands

        /// <summary>
        /// Gets the InsertTextBoxCommand command.
        /// </summary>
        public Command InsertTextBoxCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertTextBoxCommand command is executed.
        /// </summary>
        private void OnInsertTextBoxCommandExecute()
        {
            CLPTextBox textBox = new CLPTextBox();
            ((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.PageObjects.Add(textBox);
        }

        /// <summary>
        /// Gets the InsertImageCommand command.
        /// </summary>
        public Command InsertImageCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertImageCommand command is executed.
        /// </summary>
        private void OnInsertImageCommandExecute()
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
                //CLPServiceAgent.Instance.AddPageObjectToPage(image);
                ((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.PageObjects.Add(image);
            }
        }

        //private RelayCommand _insertImageStampCommand;

        ///// <summary>
        ///// Gets the InsertImageStampCommand.
        ///// </summary>
        //public RelayCommand InsertImageStampCommand
        //{
        //    get
        //    {
        //        return _insertImageStampCommand
        //            ?? (_insertImageStampCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      // Configure open file dialog box
        //                                      Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        //                                      dlg.Filter = "Images|*.png;*.jpg;*.jpeg;*.gif"; // Filter files by extension

        //                                      // Show open file dialog box
        //                                      Nullable<bool> result = dlg.ShowDialog();

        //                                      // Process open file dialog box results
        //                                      if (result == true)
        //                                      {
        //                                          // Open document
        //                                          string filename = dlg.FileName;
        //                                          CLPImageStamp image = new CLPImageStamp(filename);
        //                                          CLPService.AddPageObjectToPage(image);
        //                                      }
        //                                  }));
        //    }
        //}

        //private RelayCommand _insertBlankStampCommand;

        ///// <summary>
        ///// Gets the InsertBlankStampCommand.
        ///// </summary>
        //public RelayCommand InsertBlankStampCommand
        //{
        //    get
        //    {
        //        return _insertBlankStampCommand
        //            ?? (_insertBlankStampCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      CLPBlankStamp stamp = new CLPBlankStamp();
        //                                      CLPService.AddPageObjectToPage(stamp);
        //                                  }));
        //    }
        //}

        /// <summary>
        /// Gets the InsertSquareShapeCommand command.
        /// </summary>
        public Command InsertSquareShapeCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertSquareShapeCommand command is executed.
        /// </summary>
        private void OnInsertSquareShapeCommandExecute()
        {
            CLPSquareShape square = new CLPSquareShape();
            //CLPServiceAgent.Instance.AddPageObjectToPage(image);
            ((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.PageObjects.Add(square);
        }


        #endregion //Insert Commands

        #region Display Commands

        //private RelayCommand _sendDisplayToProjectorCommand;

        ///// <summary>
        ///// Gets the SendDisplayToProjectorCommand.
        ///// </summary>
        //public RelayCommand SendDisplayToProjectorCommand
        //{
        //    get
        //    {
        //        return _sendDisplayToProjectorCommand
        //            ?? (_sendDisplayToProjectorCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      if (App.Peer.Channel != null)
        //                                      {
        //                                          if ((App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).Display is LinkedDisplayViewModel)
        //                                          {
        //                                              (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).LinkedDisplay.IsOnProjector = true;
        //                                              (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay.IsOnProjector = false;
        //                                              App.Peer.Channel.SwitchProjectorDisplay("LinkedDisplay", new List<string>());
        //                                          }
        //                                          else
        //                                          {
        //                                              (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).LinkedDisplay.IsOnProjector = false;
        //                                              (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay.IsOnProjector = true;
        //                                              List<string> pageList = new List<string>();
        //                                              foreach (var page in (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay.DisplayPages)
        //                                              {
        //                                                  pageList.Add(ObjectSerializer.ToString(page.Page));
        //                                              }

        //                                              App.Peer.Channel.SwitchProjectorDisplay("GridDisplay", pageList);
        //                                          }
        //                                      }
        //                                  }));
        //    }
        //}

        //private RelayCommand _switchToLinkedDisplayCommand;

        ///// <summary>
        ///// Gets the SwitchToLinkedDisplayCommand.
        ///// </summary>
        //public RelayCommand SwitchToLinkedDisplayCommand
        //{
        //    get
        //    {
        //        return _switchToLinkedDisplayCommand
        //            ?? (_switchToLinkedDisplayCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).Display = (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).LinkedDisplay;
        //                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).LinkedDisplay.IsActive = true;
        //                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay.IsActive = false;
        //                                  }));
        //    }
        //}

        //private RelayCommand _createNewGridDisplayCommand;

        ///// <summary>
        ///// Gets the CreateNewGridDisplayCommand.
        ///// </summary>
        //public RelayCommand CreateNewGridDisplayCommand
        //{
        //    get
        //    {
        //        return _createNewGridDisplayCommand
        //            ?? (_createNewGridDisplayCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).Display = (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay;
        //                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).LinkedDisplay.IsActive = false;
        //                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay.IsActive = true;
        //                                  }));
        //    }
        //}

        #endregion //Display Commands

        #region Submission Command

        /// <summary>
        /// Gets the SubmitPageCommand command.
        /// </summary>
        public Command SubmitPageCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SubmitPageCommand command is executed.
        /// </summary>
        private void OnSubmitPageCommandExecute()
        {
            //Steve - change to different thread and do callback to make sure sent page has arrived
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
                    //CLPService.SubmitPage(clpPageViewModel);
                });
            }
            CanSendToTeacher = false;
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool CanSendToTeacher
        {
            get { return GetValue<bool>(CanSendToTeacherProperty); }
            set { SetValue(CanSendToTeacherProperty, value); }
        }

        /// <summary>
        /// Register the CanSendToTeacher property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CanSendToTeacherProperty = RegisterProperty("CanSendToTeacher", typeof(bool));


        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Timer timer = sender as Timer;
            timer.Stop();
            timer.Elapsed -= timer_Elapsed;
            IsSending = false;
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsSending
        {
            get { return GetValue<bool>(IsSendingProperty); }
            set
            {
                SetValue(IsSendingProperty, value);
                if (IsSending)
                {
                    SendButtonVisibility = Visibility.Collapsed;
                    IsSentInfoVisibility = Visibility.Visible;
                }
                else
                {
                    SendButtonVisibility = Visibility.Visible;
                    IsSentInfoVisibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Register the IsSending property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsSendingProperty = RegisterProperty("IsSending", typeof(bool));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Visibility SendButtonVisibility
        {
            get { return GetValue<Visibility>(SendButtonVisibilityProperty); }
            set { SetValue(SendButtonVisibilityProperty, value); }
        }

        /// <summary>
        /// Register the SendButtonVisibility property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SendButtonVisibilityProperty = RegisterProperty("SendButtonVisibility", typeof(Visibility));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Visibility IsSentInfoVisibility
        {
            get { return GetValue<Visibility>(IsSentInfoVisibilityProperty); }
            set { SetValue(IsSentInfoVisibilityProperty, value); }
        }

        /// <summary>
        /// Register the IsSentInfoVisibility property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsSentInfoVisibilityProperty = RegisterProperty("IsSentInfoVisibility", typeof(Visibility));

        #endregion //Submission Command

        /// <summary>
        /// Gets the ExitCommand command.
        /// </summary>
        public Command ExitCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the ExitCommand command is executed.
        /// </summary>
        private void OnExitCommandExecute()
        {
            if (MessageBox.Show("Are you sure you want to exit?",
                                        "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                                      {
                                                          CLPServiceAgent.Instance.Exit();
                                                      }
        }

        #region HistoryCommands
        //private RelayCommand _undoCommand;

        ///// <summary>
        ///// Gets the UndoCommand.
        ///// </summary>
        //public RelayCommand UndoCommand
        //{
        //    get
        //    {
        //        return _undoCommand
        //            ?? (_undoCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      AppMessages.RequestCurrentDisplayedPage.Send((clpPageViewModel) =>
        //                                      {
        //                                          clpPageViewModel.HistoryVM.undo();
        //                                      });
        //                                  }));
        //    }
        //}
        //private RelayCommand _redoCommand;

        ///// <summary>
        ///// Gets the RedoCommand.
        ///// </summary>
        //public RelayCommand RedoCommand
        //{
        //    get
        //    {
        //        return _redoCommand
        //            ?? (_redoCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      AppMessages.RequestCurrentDisplayedPage.Send((clpPageViewModel) =>
        //                                      {
        //                                          clpPageViewModel.HistoryVM.redo();
        //                                      });
        //                                  }));
        //    }
        //}

        //private RelayCommand _audioCommand;
        //private bool recording = false;
        //public RelayCommand AudioCommand
        //{
        //    get
        //    {
        //        return _audioCommand
        //            ?? (_audioCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      AppMessages.Audio.Send("start");
        //                                      recording = !recording;
        //                                      if (recording)
        //                                      {
        //                                          RecordImage = new Uri("..\\Images\\mic_stop.png", UriKind.Relative);
        //                                      }
        //                                      else
        //                                      {
        //                                          RecordImage = new Uri("..\\Images\\mic_start.png", UriKind.Relative);
        //                                      }
        //                                  }));
        //    }
        //}
        //private RelayCommand _playAudioCommand;
        //public RelayCommand PlayAudioCommand
        //{
        //    get
        //    {
        //        return _playAudioCommand
        //            ?? (_playAudioCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      AppMessages.Audio.Send("play");


        //                                  }));
        //    }
        //}
        //private RelayCommand _enablePlaybackCommand;
        //public RelayCommand EnablePlaybackCommand
        //{
        //    get
        //    {
        //        return _enablePlaybackCommand
        //            ?? (_enablePlaybackCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      AppMessages.ChangePlayback.Send(true);

        //                                  }));
        //    }
        //}
        #endregion //History Commands

        #endregion //Commands

        #endregion //Ribbon
    }
}