﻿using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Shapes;
using Catel.Data;
using Catel.MVVM;
using CLP.CustomControls;
using CLP.Entities;

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

            PenSize = 2;
            DrawingAttributes = new DrawingAttributes
            {
                Height = PenSize,
                Width = PenSize,
                Color = Colors.Black,
                FitToCurve = true
            };

            PageInteractionMode = PageInteractionModes.Pen;
        }

        private void InitializeCommands()
        {
            ShowBackStageCommand = new Command(OnShowBackStageCommandExecute);
            AddPageObjectToPageCommand = new Command<string>(OnAddPageObjectToPageCommandExecute, OnAddPageObjectToPageCanExecute);
        }

        private void InitializeButtons()
        {
            //PageInteractionMode Toggles
            _setSelectModeButton = new GroupedRibbonButton("Select",
                                                           "PageInteractionMode",
                                                           "pack://application:,,,/Resources/Images/Hand32.png",
                                                           PageInteractionModes.Select.ToString());
            _setSelectModeButton.Checked += _button_Checked;
            _setPenModeButton = new GroupedRibbonButton("Pen",
                                                        "PageInteractionMode",
                                                        "pack://application:,,,/Resources/Images/Pen32.png",
                                                        PageInteractionModes.Pen.ToString());
            _setPenModeButton.Checked += _button_Checked;
            _setEraserModeButton = new GroupedRibbonButton("Eraser",
                                                           "PageInteractionMode",
                                                           "pack://application:,,,/Resources/Images/PointEraser32.png",
                                                           PageInteractionModes.Eraser.ToString());
            _setEraserModeButton.Checked += _button_Checked;
            _setLassoModeButton = new GroupedRibbonButton("Lasso",
                                                          "PageInteractionMode",
                                                          "pack://application:,,,/Resources/Images/Lasso32.png",
                                                          PageInteractionModes.Lasso.ToString());
            _setLassoModeButton.Checked += _button_Checked;
            _setCutModeButton = new GroupedRibbonButton("Cut",
                                                        "PageInteractionMode",
                                                        "pack://application:,,,/Resources/Images/Scissors32.png",
                                                        PageInteractionModes.Cut.ToString());
            _setCutModeButton.Checked += _button_Checked;
            _setDividerCreationModeButton = new GroupedRibbonButton("Create Divider",
                                                                    "PageInteractionMode",
                                                                    "pack://application:,,,/Resources/Images/InkArray32.png",
                                                                    PageInteractionModes.DividerCreation.ToString());
            _setDividerCreationModeButton.Checked += _button_Checked;

            //Images
            //TODO: Better Icons
            _insertImageButton = new RibbonButton("Text", "pack://application:,,,/Images/AddImage.png", AddPageObjectToPageCommand, "IMAGE");

            //Stamps
            _insertGeneralStampButton = new RibbonButton("Stamp",
                                                         "pack://application:,,,/Resources/Images/Stamp32.png",
                                                         AddPageObjectToPageCommand,
                                                         "BLANK_GENERAL_STAMP");
            _insertGroupStampButton = new RibbonButton("Group Stamp",
                                                       "pack://application:,,,/Resources/Images/CollectionStamp32.png",
                                                       AddPageObjectToPageCommand,
                                                       "BLANK_GROUP_STAMP");
            _insertImageGeneralStampButton = new RibbonButton("Image Stamp",
                                                              "pack://application:,,,/Images/PictureStamp.png",
                                                              AddPageObjectToPageCommand,
                                                              "IMAGE_GENERAL_STAMP"); //TODO: Better Icon
            _insertImageGroupStampButton = new RibbonButton("Image Group Stamp",
                                                            "pack://application:,,,/Images/PictureStamp.png",
                                                            AddPageObjectToPageCommand,
                                                            "IMAGE_GROUP_STAMP"); //TODO: Better Icon
            _insertPileButton = new RibbonButton("Pile", "pack://application:,,,/Resources/Images/Pile32.png", AddPageObjectToPageCommand, "PILE");

            //Arrays
            _insertArrayButton = new RibbonButton("Array", "pack://application:,,,/Resources/Images/Array32.png", AddPageObjectToPageCommand, "ARRAY");
            _insert10x10ArrayButton = new RibbonButton("10x10 Array",
                                                       "pack://application:,,,/Resources/Images/PresetArray32.png",
                                                       AddPageObjectToPageCommand,
                                                       "10X10");
            _insertArrayCardButton = new RibbonButton("Array Card",
                                                      "pack://application:,,,/Resources/Images/ArrayCard32.png",
                                                      AddPageObjectToPageCommand,
                                                      "ARRAYCARD");
            _insertFactorCardButton = new RibbonButton("Factor Card",
                                                       "pack://application:,,,/Resources/Images/FactorCard32.png",
                                                       AddPageObjectToPageCommand,
                                                       "FACTORCARD");

            //Division Templates
            _insertDivisionTemplateButton = new RibbonButton("Division Tool",
                                                             "pack://application:,,,/Resources/Images/FuzzyFactorCard32.png",
                                                             AddPageObjectToPageCommand,
                                                             "DIVISIONTEMPLATE");
            _insertDivisionTemplateWithTilesButton = new RibbonButton("Division Tool with Tiles",
                                                                      "pack://application:,,,/Resources/Images/FuzzyFactorCard32.png",
                                                                      AddPageObjectToPageCommand,
                                                                      "DIVISIONTEMPLATEWITHTILES");

            //NumberLine
            _insertNumberLineButton = new RibbonButton("Number Line",
                                                       "pack://application:,,,/Resources/Images/NumberLine32.png",
                                                       AddPageObjectToPageCommand,
                                                       "NUMBERLINE");

            //Text
            //TODO: Better Icons
            _insertTextBoxButton = new RibbonButton("Text", "pack://application:,,,/Images/AddText.png", AddPageObjectToPageCommand, "TEXTBOX");

            //Shapes
            //TODO: Better Icons
            _insertSquareButton = new RibbonButton("Square", "pack://application:,,,/Images/AddSquare.png", AddPageObjectToPageCommand, "SQUARE");
            _insertCircleButton = new RibbonButton("Circle", "pack://application:,,,/Images/AddCircle.png", AddPageObjectToPageCommand, "CIRCLE");
            _insertHorizontalLineButton = new RibbonButton("Horizontal Line",
                                                           "pack://application:,,,/Images/HorizontalLineIcon.png",
                                                           AddPageObjectToPageCommand,
                                                           "HORIZONTALLINE");
            _insertVerticalLineButton = new RibbonButton("Vertical Line",
                                                         "pack://application:,,,/Images/VerticalLineIcon.png",
                                                         AddPageObjectToPageCommand,
                                                         "VERTICALLINE");
            _insertProtractorButton = new RibbonButton("Protractor",
                                                       "pack://application:,,,/Images/Protractor64.png",
                                                       AddPageObjectToPageCommand,
                                                       "PROTRACTOR");
        }

        private bool _isCheckedEventRunning = false;

        private void _button_Checked(object sender, RoutedEventArgs e)
        {
            _isCheckedEventRunning = true;
            var checkedButton = sender as GroupedRibbonButton;
            if (checkedButton == null)
            {
                return;
            }

            switch (checkedButton.GroupName)
            {
                case "PageInteractionMode":
                    PageInteractionMode = (PageInteractionModes)Enum.Parse(typeof (PageInteractionModes), checkedButton.AssociatedEnumValue);

                    if (App.MainWindowViewModel == null)
                    {
                        break;
                    }
                    var contextRibbon = NotebookWorkspaceViewModel.GetContextRibbon();
                    if (contextRibbon == null)
                    {
                        break;
                    }

                    switch (PageInteractionMode)
                    {
                        case PageInteractionModes.None:
                            contextRibbon.Buttons.Clear();
                            break;
                        case PageInteractionModes.Select:
                            contextRibbon.Buttons.Clear();
                            break;
                        case PageInteractionModes.Pen:
                            contextRibbon.SetPenContextButtons();
                            break;
                        case PageInteractionModes.Eraser:
                            contextRibbon.SetEraserContextButtons();
                            break;
                        case PageInteractionModes.Lasso:
                            contextRibbon.Buttons.Clear();
                            break;
                        case PageInteractionModes.Cut:
                            contextRibbon.Buttons.Clear();
                            break;
                        case PageInteractionModes.DividerCreation:
                            contextRibbon.Buttons.Clear();
                            break;
                    }

                    break;
            }
            _isCheckedEventRunning = false;
        }

        #region Buttons

        public static Line Separater
        {
            get
            {
                var dict = new ResourceDictionary();
                var uri = new Uri(@"pack://application:,,,/Resources/CLPBrushes.xaml");
                dict.Source = uri;
                var grayEdgeColor = dict["GrayBorderColor"] as Brush;

                //Separater
                return new Line
                       {
                           Y1 = 0,
                           Y2 = 1,
                           Stretch = Stretch.Fill,
                           Margin = new Thickness(2),
                           Stroke = grayEdgeColor,
                           UseLayoutRounding = false,
                           SnapsToDevicePixels = false,
                           StrokeThickness = 1
                       };
            }
        }

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
        private RibbonButton _insertGeneralStampButton;
        private RibbonButton _insertGroupStampButton;
        private RibbonButton _insertImageGeneralStampButton;
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

        //NumberLine
        private RibbonButton _insertNumberLineButton;

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

        #region Properties

        /// <summary>Interaction Mode for the current page.</summary>
        public PageInteractionModes PageInteractionMode
        {
            get { return GetValue<PageInteractionModes>(PageInteractionModeProperty); }
            set
            {
                SetValue(PageInteractionModeProperty, value);
                if (_isCheckedEventRunning)
                {
                    return;
                }

                switch (value)
                {
                    case PageInteractionModes.None:
                        break;
                    case PageInteractionModes.Select:
                        _setSelectModeButton.IsChecked = true;
                        break;
                    case PageInteractionModes.Pen:
                        _setPenModeButton.IsChecked = true;
                        break;
                    case PageInteractionModes.Eraser:
                        _setEraserModeButton.IsChecked = true;
                        break;
                    case PageInteractionModes.Lasso:
                        _setLassoModeButton.IsChecked = true;
                        break;
                    case PageInteractionModes.Cut:
                        _setCutModeButton.IsChecked = true;
                        break;
                    case PageInteractionModes.DividerCreation:
                        _setDividerCreationModeButton.IsChecked = true;
                        break;
                }
            }
        }

        public static readonly PropertyData PageInteractionModeProperty = RegisterProperty("PageInteractionMode",
                                                                                           typeof (PageInteractionModes),
                                                                                           PageInteractionModes.Pen);

        /// <summary>Gets or sets the property value.</summary>
        public InkCanvasEditingMode EraserMode
        {
            get { return GetValue<InkCanvasEditingMode>(EraserModeProperty); }
            set { SetValue(EraserModeProperty, value); }
        }

        public static readonly PropertyData EraserModeProperty = RegisterProperty("EraserMode",
                                                                                  typeof (InkCanvasEditingMode),
                                                                                  InkCanvasEditingMode.EraseByStroke);

        /// <summary>Size of the Pen.</summary>
        public double PenSize
        {
            get { return GetValue<double>(PenSizeProperty); }
            set { SetValue(PenSizeProperty, value); }
        }

        public static readonly PropertyData PenSizeProperty = RegisterProperty("PenSize", typeof (double), 3);

        /// <summary>Gets the DrawingAttributes of the Ribbon.</summary>
        public DrawingAttributes DrawingAttributes
        {
            get { return GetValue<DrawingAttributes>(DrawingAttributesProperty); }
            set { SetValue(DrawingAttributesProperty, value); }
        }

        public static readonly PropertyData DrawingAttributesProperty = RegisterProperty("DrawingAttributes", typeof (DrawingAttributes));

        #endregion //Properties

        #region Commands

        /// <summary>Brings up the BackStage.</summary>
        public Command ShowBackStageCommand { get; private set; }

        private void OnShowBackStageCommandExecute()
        {
            MainWindow.BackStage.CurrentNavigationPane = NavigationPanes.Info;
            MainWindow.IsBackStageVisible = true;
        }

        #region Insert PageObject Commands

        /// <summary>Adds pageObject to the Current Page.</summary>
        public Command<string> AddPageObjectToPageCommand { get; private set; }

        private void OnAddPageObjectToPageCommandExecute(string pageObjectType)
        {
            switch (pageObjectType)
            {
                    //Image
                case "IMAGE":
                    CLPImageViewModel.AddImageToPage(CurrentPage);
                    break;

                    //Stamps
                case "BLANK_GENERAL_STAMP":
                    StampViewModel.AddBlankGeneralStampToPage(CurrentPage);
                    break;
                case "BLANK_GROUP_STAMP":
                    StampViewModel.AddBlankGroupStampToPage(CurrentPage);
                    break;
                case "IMAGE_GENERAL_STAMP":
                    StampViewModel.AddImageGeneralStampToPage(CurrentPage);
                    break;
                case "IMAGE_GROUP_STAMP":
                    StampViewModel.AddImageGroupStampToPage(CurrentPage);
                    break;
                case "PILE":
                    StampViewModel.AddPileToPage(CurrentPage);
                    break;

                    //Arrays
                case "ARRAY":
                    CLPArrayViewModel.AddArrayToPage(CurrentPage);
                    break;

                    //Number Line
                case "NUMBERLINE":
                    NumberLineViewModel.AddNumberLineToPage(CurrentPage);
                    break;

                    //Division Template
                case "DIVISIONTEMPLATE":
                    FuzzyFactorCardViewModel.AddDivisionTemplateToPage(CurrentPage);
                    break;
            }

            PageInteractionMode = PageInteractionModes.Select;
        }

        private bool OnAddPageObjectToPageCanExecute(string pageObjectType) { return CurrentPage != null; }

        #endregion //Insert PageObject Commands

        #endregion //Commands

        #region Methods

        public void SetRibbonButtons()
        {
            Buttons.Add(_setSelectModeButton);
            Buttons.Add(_setPenModeButton);
            Buttons.Add(_setEraserModeButton);
            Buttons.Add(Separater);
            Buttons.Add(_setLassoModeButton);
            Buttons.Add(_setCutModeButton);
            Buttons.Add(_setDividerCreationModeButton);

            Buttons.Add(Separater);

            Buttons.Add(_insertGeneralStampButton);
            Buttons.Add(_insertGroupStampButton);
            Buttons.Add(_insertPileButton);
            Buttons.Add(_insertNumberLineButton);
            Buttons.Add(_insertArrayButton);
            Buttons.Add(_insertDivisionTemplateButton);
         //   Buttons.Add(_insertDivisionTemplateWithTilesButton);
        }

        #endregion //Methods
    }
}