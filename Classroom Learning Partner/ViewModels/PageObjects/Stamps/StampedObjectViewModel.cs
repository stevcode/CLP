using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.CustomControls;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>UserControl view model.</summary>
    public class StampedObjectViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="StampedObjectViewModel" /> class.</summary>
        public StampedObjectViewModel(StampedObject stampedObject, INotebookService notebookService)
        {
            PageObject = stampedObject;
            RaisePropertyChanged("IsGroupStampedObject");
            if (App.MainWindowViewModel.ImagePool.ContainsKey(stampedObject.ImageHashID))
            {
                SourceImage = App.MainWindowViewModel.ImagePool[stampedObject.ImageHashID];
            }
            else
            {
                var filePath = string.Empty;
                var imageFilePaths = Directory.EnumerateFiles(notebookService.CurrentImageCacheDirectory);
                foreach (var imageFilePath in from imageFilePath in imageFilePaths
                                              let imageHashID = Path.GetFileNameWithoutExtension(imageFilePath)
                                              where imageHashID == stampedObject.ImageHashID
                                              select imageFilePath)
                {
                    filePath = imageFilePath;
                    break;
                }

                var bitmapImage = CLPImage.GetImageFromPath(filePath);
                if (bitmapImage != null)
                {
                    SourceImage = bitmapImage;
                    App.MainWindowViewModel.ImagePool.Add(stampedObject.ImageHashID, bitmapImage);
                }
            }

            foreach (
                var strokePath in
                    stampedObject.SerializedStrokes.Select(serializedStroke => serializedStroke.ToStroke())
                                 .Select(stroke => new StrokePathDTO(stroke)))
            {
                StrokePaths.Add(strokePath);
            }

            stampedObject.AcceptedPageObjects.Clear();
            foreach (var acceptedPageObjectID in stampedObject.AcceptedPageObjectIDs)
            {
                stampedObject.AcceptedPageObjects.Add(stampedObject.ParentPage.GetPageObjectByID(acceptedPageObjectID));
            }

            ParameterizeStampedObjectCommand = new Command<bool>(OnParameterizeStampedObjectCommandExecute);

            if (IsGroupStampedObject)
            {
                _contextButtons.Add(MajorRibbonViewModel.Separater);

                _contextButtons.Add(new RibbonButton("Create Copies", "pack://application:,,,/Images/AddToDisplay.png", ParameterizeStampedObjectCommand, null, true));
            }
        }

        /// <summary>Gets the title of the view model.</summary>
        /// <value>The title.</value>
        public override string Title
        {
            get { return "StampedObjectVM"; }
        }

        #endregion //Constructor

        #region Model

        /// <summary>Type of <see cref="StampedObject" />.</summary>
        [ViewModelToModel("PageObject")]
        public StampedObjectTypes StampedObjectType
        {
            get { return GetValue<StampedObjectTypes>(StampedObjectTypeProperty); }
            set { SetValue(StampedObjectTypeProperty, value); }
        }

        public static readonly PropertyData StampedObjectTypeProperty = RegisterProperty("StampedObjectType", typeof (StampedObjectTypes));

        /// <summary>Number of parts represented by the StampCopy. Only visible for collection copies.</summary>
        [ViewModelToModel("PageObject")]
        public int Parts
        {
            get { return GetValue<int>(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof (int));

        #endregion //Model

        #region Binding

        public bool IsGroupStampedObject
        {
            get
            {
                var stampedObject = PageObject as StampedObject;
                if (stampedObject == null)
                {
                    return false;
                }

                return StampedObjectType == StampedObjectTypes.GroupStampedObject || StampedObjectType == StampedObjectTypes.EmptyGroupStampedObject;
            }
        }

        /// <summary>The visible image, loaded from the page's ImagePool.</summary>
        public ImageSource SourceImage
        {
            get { return GetValue<ImageSource>(SourceImageProperty); }
            set { SetValue(SourceImageProperty, value); }
        }

        public static readonly PropertyData SourceImageProperty = RegisterProperty("SourceImage", typeof (ImageSource));

        /// <summary>List of <see cref="StrokePathDTO" />s that make up the <see cref="StampedObject" />.</summary>
        public ObservableCollection<StrokePathDTO> StrokePaths
        {
            get { return GetValue<ObservableCollection<StrokePathDTO>>(StrokePathsProperty); }
            set { SetValue(StrokePathsProperty, value); }
        }

        public static readonly PropertyData StrokePathsProperty = RegisterProperty("StrokePaths",
                                                                                   typeof (ObservableCollection<StrokePathDTO>),
                                                                                   () => new ObservableCollection<StrokePathDTO>());

        #endregion //Binding

        #region Commands

        /// <summary>Parameterizes the StampedObject.</summary>
        public Command<bool> ParameterizeStampedObjectCommand { get; private set; }

        private void OnParameterizeStampedObjectCommandExecute(bool isDuplicateAndTake)
        {
            var stampedObject = PageObject as StampedObject;
            if (stampedObject == null)
            {
                return;
            }

            var keyPad = new KeypadWindowView("How many copies?", 21)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Top = 100,
                Left = 100
            };
            keyPad.ShowDialog();
            if (keyPad.DialogResult != true ||
                keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }

            var numberOfCopies = Int32.Parse(keyPad.NumbersEntered.Text);

            var ungroupedStampedObjects = new List<StampedObject>();
            var numberOfAcceptedStampedObjects = stampedObject.AcceptedPageObjects.Count;
            if (isDuplicateAndTake)
            {
                var stampedObjects = stampedObject.ParentPage.PageObjects.OfType<StampedObject>()
                             .Where(x => x.StampedObjectType == StampedObjectTypes.GeneralStampedObject && x.Parts == 1);

                var groupStampedObjects =
                    stampedObject.ParentPage.PageObjects.OfType<StampedObject>()
                              .Where(
                                     x =>
                                     (x.StampedObjectType == StampedObjectTypes.GroupStampedObject ||
                                      x.StampedObjectType == StampedObjectTypes.EmptyGroupStampedObject) && x.Parts > 0)
                              .ToList();

                ungroupedStampedObjects =
                    stampedObjects.Where(c => groupStampedObjects.Count(x => x.AcceptedPageObjectIDs.Contains(c.ID)) == 0).ToList();

                

                if (ungroupedStampedObjects.Count < numberOfAcceptedStampedObjects * numberOfCopies)
                {
                    MessageBox.Show("Not enough objects on page.");
                    return;
                }
            }

            var initialXPosition = XPosition + Width + 10.0;
            var initialYPosition = YPosition;

            var stampCopiesToAdd = new List<IPageObject>();
            var ungroupedStampedObjectsIndex = 0;
            for (var i = 0; i < numberOfCopies; i++)
            {
                var newStampedObject = stampedObject.Duplicate() as StampedObject;

                if (newStampedObject == null)
                {
                    continue;
                }

                newStampedObject.XPosition = initialXPosition;
                newStampedObject.YPosition = initialYPosition;
                newStampedObject.AcceptedPageObjectIDs.Clear();
                newStampedObject.AcceptedPageObjects.Clear();

                stampCopiesToAdd.Add(newStampedObject);
                if (initialXPosition + 2 * stampedObject.Width + 5 < PageObject.ParentPage.Width)
                {
                    initialXPosition += stampedObject.Width + 5;
                }
                else if (initialYPosition + 2 * stampedObject.Height + 5 < PageObject.ParentPage.Height)
                {
                    initialXPosition = 25;
                    initialYPosition += stampedObject.Height + 5;
                }

                if (isDuplicateAndTake)
                {
                    for (var n = 0; n < numberOfAcceptedStampedObjects; n++)
                    {
                        var referenceAcceptedPageObject = stampedObject.AcceptedPageObjects[n];
                        var ungroupedStampObject = ungroupedStampedObjects[ungroupedStampedObjectsIndex];
                        ungroupedStampObject.XPosition = newStampedObject.XPosition + (referenceAcceptedPageObject.XPosition - stampedObject.XPosition);
                        ungroupedStampObject.YPosition = newStampedObject.YPosition + (referenceAcceptedPageObject.YPosition - stampedObject.YPosition);
                        newStampedObject.AcceptedPageObjectIDs.Add(ungroupedStampObject.ID);
                        newStampedObject.AcceptedPageObjects.Add(ungroupedStampObject);

                        ungroupedStampedObjectsIndex++;
                    }
                }
                else
                {
                    foreach (var pageObject in stampedObject.AcceptedPageObjects)
                    {
                        var newPageObject = pageObject.Duplicate();
                        newPageObject.XPosition = newStampedObject.XPosition + (pageObject.XPosition - stampedObject.XPosition);
                        newPageObject.YPosition = newStampedObject.YPosition + (pageObject.YPosition - stampedObject.YPosition);
                        stampCopiesToAdd.Add(newPageObject);
                    } 
                }
            }

            ACLPPageBaseViewModel.AddPageObjectsToPage(stampedObject.ParentPage, stampCopiesToAdd);
        }

        #endregion //Commands
    }
}