using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Catel.Data;
using Catel.MVVM;
using CLP.CustomControls;
using CLP.Entities;
using Shape = CLP.Entities.Shape;

namespace Classroom_Learning_Partner.ViewModels
{
    public class MajorRibbonViewModel : ViewModelBase
    {
        public MainWindowViewModel MainWindow
        {
            get { return App.MainWindowViewModel; }
        }

        public static CLPPage CurrentPage
        {
            get { return NotebookPagesPanelViewModel.GetCurrentPage(); }
        }

        public MajorRibbonViewModel()
        {
            InitializeCommands();
            InitializeButtons();
            SetRibbonButtons();
        }

        private void InitializeCommands()
        {
            ShowBackStageCommand = new Command(OnShowBackStageCommandExecute);
            InsertCircleCommand = new Command(OnInsertCircleCommandExecute);
        }

        private void InitializeButtons()
        {
            var dict = new ResourceDictionary();
            var uri = new Uri(@"pack://application:,,,/Resources/CLPBrushes.xaml");
            dict.Source = uri;
            var grayEdgeColor = dict["GrayEdgeColor"] as Brush;

            //Separater
            _separater = new Line
                         {
                             Y1 = 0,
                             Y2 = 1,
                             Stretch = Stretch.Fill,
                             Margin = new Thickness(2),
                             Stroke = grayEdgeColor,
                             StrokeThickness = 1
                         };

            //PageInteractionMode Toggles
            _setSelectModeButton = new GroupedRibbonButton("Select", "PageInteractionMode", "pack://application:,,,/Resources/Images/Hand32.png", null);
            _setPenModeButton = new GroupedRibbonButton("Pen", "PageInteractionMode", "pack://application:,,,/Resources/Images/Pen32.png", null);
            _setEraserModeButton = new GroupedRibbonButton("Eraser", "PageInteractionMode", "pack://application:,,,/Resources/Images/Hand32.png", null);
            _setLassoModeButton = new GroupedRibbonButton("Lasso", "PageInteractionMode", "pack://application:,,,/Resources/Images/Lasso32.png", null);
            _setCutModeButton = new GroupedRibbonButton("Cut", "PageInteractionMode", "pack://application:,,,/Resources/Images/Scissors32.png", null);
            _setDividerCreationModeButton = new GroupedRibbonButton("Divider", "PageInteractionMode", "pack://application:,,,/Resources/Images/Hand32.png", null);

            //Text
            _insertTextBoxButton = new RibbonButton("Insert Text", "pack://application:,,,/Images/AddSquare.png", InsertCircleCommand);

            //Shapes
            //TODO: Better Icons
            _insertSquareButton = new RibbonButton("Insert Square", "pack://application:,,,/Images/AddSquare.png", InsertCircleCommand);
            _insertCircleButton = new RibbonButton("Insert Circle", "pack://application:,,,/Images/AddCircle.png", InsertCircleCommand);
            _insertHorizontalLineButton = new RibbonButton("Insert Horizontal Line",
                                                           "pack://application:,,,/Images/HorizontalLineIcon.png",
                                                           InsertCircleCommand);
            _insertVerticalLineButton = new RibbonButton("Insert Vertical Line",
                                                         "pack://application:,,,/Images/VerticalLineIcon.png",
                                                         InsertCircleCommand);
            _insertProtractorButton = new RibbonButton("Insert Protractor", "pack://application:,,,/Images/Protractor64.png", InsertCircleCommand);
        }

        #region Buttons

        private Line _separater;

        #region PageInteractionMode Toggle Buttons

        private GroupedRibbonButton _setSelectModeButton;
        private GroupedRibbonButton _setPenModeButton;
        private GroupedRibbonButton _setEraserModeButton;
        private GroupedRibbonButton _setLassoModeButton;
        private GroupedRibbonButton _setCutModeButton;
        private GroupedRibbonButton _setDividerCreationModeButton;

        #endregion //PageInteractionMode Toggles

        #region Insert PageObject Buttons

        //Images
        private RibbonButton _insertImageButton;

        //Stamps
        private RibbonButton _insertStampButton;
        private RibbonButton _insertImageStampButton;
        private RibbonButton _insertGroupStampButton;
        private RibbonButton _insertImageGroupStampButton;
        private RibbonButton _insertPileButton;

        //Arrays
        private RibbonButton _insertArrayButton;
        private RibbonButton _insert10x10ArrayButton;
        private RibbonButton _insertArrayCardButton;
        private RibbonButton _insertFactorCardButton;

        //Division Templates
        private RibbonButton _insertDivisionTemplateButton;
        private RibbonButton _insertDivisionTemplateWithTilesButton;

        //Text
        private RibbonButton _insertTextBoxButton;

        //Shapes
        private RibbonButton _insertSquareButton;
        private RibbonButton _insertCircleButton;
        private RibbonButton _insertHorizontalLineButton;
        private RibbonButton _insertVerticalLineButton;
        private RibbonButton _insertProtractorButton;

        #endregion //Insert PageObject Buttons

        #endregion //Buttons

        #region Bindings

        /// <summary>List of the buttons currently on the Ribbon.</summary>
        public ObservableCollection<UIElement> Buttons
        {
            get { return GetValue<ObservableCollection<UIElement>>(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        public static readonly PropertyData ButtonsProperty = RegisterProperty("Buttons",
                                                                               typeof (ObservableCollection<UIElement>),
                                                                               () => new ObservableCollection<UIElement>());

        #endregion //Bindings

        #region Commands

        /// <summary>Brings up the BackStage.</summary>
        public Command ShowBackStageCommand { get; private set; }

        private void OnShowBackStageCommandExecute() { }

        #region Insert PageObject Commands

        /// <summary>Inserts a circle.</summary>
        public Command InsertCircleCommand { get; private set; }

        private void OnInsertCircleCommandExecute()
        {
            var circle = new Shape(CurrentPage, ShapeType.Ellipse);
            ACLPPageBaseViewModel.AddPageObjectToPage(circle);
        }

        #endregion //Insert PageObject Commands

        #endregion //Commands

        #region Methods

        public void SetRibbonButtons()
        {
            Buttons.Add(_setSelectModeButton);
            Buttons.Add(_setPenModeButton);
            Buttons.Add(_setEraserModeButton);
            Buttons.Add(_setLassoModeButton);
            Buttons.Add(_setCutModeButton);
            Buttons.Add(_setDividerCreationModeButton);

            Buttons.Add(_separater);

            Buttons.Add(_insertSquareButton);
            Buttons.Add(_insertCircleButton);
            Buttons.Add(_insertHorizontalLineButton);
            Buttons.Add(_insertVerticalLineButton);
            Buttons.Add(_insertProtractorButton);

        }

        #endregion //Methods
    }
}