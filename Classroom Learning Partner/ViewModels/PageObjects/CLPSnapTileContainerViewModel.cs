using System;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPSnapTileContainerViewModel : ACLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPSnapTileViewModel class.
        /// </summary>
        public CLPSnapTileContainerViewModel(CLPSnapTileContainer tile)
        {
            Tiles = new ObservableCollection<string>();
            PageObject = tile;

            SnapCommand = new Command<DragCompletedEventArgs>(OnSnapCommandExecute);
            RemoveTileCommand = new Command(OnRemoveTileCommandExecute);
            DuplicateContainerCommand = new Command(OnDuplicateContainerCommandExecute);

            IsMouseOverShowEnabled = true;
        }

        public override string Title { get { return "SnapTileContainerVM"; } }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<string> Tiles
        {
            get { return GetValue<ObservableCollection<string>>(TilesProperty); }
            set { SetValue(TilesProperty, value); }
        }

        public static readonly PropertyData TilesProperty = RegisterProperty("Tiles", typeof(ObservableCollection<string>));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int NumberOfTiles
        {
            get { return GetValue<int>(NumberOfTilesProperty); }
            set { SetValue(NumberOfTilesProperty, value); }
        }

        public static readonly PropertyData NumberOfTilesProperty = RegisterProperty("NumberOfTiles", typeof(int), 0, (sender, e) => ((CLPSnapTileContainerViewModel)sender).OnNumberOfTilesChanged());

        /// <summary>
        /// Called when the name property has changed.
        /// </summary>
        private void OnNumberOfTilesChanged()
        {
            int diff = NumberOfTiles - Tiles.Count;

            if (diff > 0)
            {
                for (int i = diff - 1; i >= 0; i--)
                {
                    Tiles.Add("SpringGreen");
                }
            }
            else
            {
                diff *= -1;
                for (int i = diff - 1; i >= 0; i--)
                {
                    Tiles.RemoveAt(Tiles.Count - 1);
                }
            }

            Height = CLPSnapTileContainer.TILE_HEIGHT * NumberOfTiles;
        }

        /// <summary>
        /// Gets the SnapCommand command.
        /// </summary>
        public Command<DragCompletedEventArgs> SnapCommand { get; private set; }

        private void OnSnapCommandExecute(DragCompletedEventArgs e)
        {
            CLPPage currentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

            PageObject.ParentPage.PageHistory.Push(new CLPHistoryMoveObject(PageObject.UniqueID, PageObject.XPosition, PageObject.YPosition, PageObject.XPosition, PageObject.YPosition));
            PageObject.ParentPage.updateProgress();

            foreach (var pageObject in currentPage.PageObjects)
            {
                if(!(pageObject is CLPSnapTileContainer))
                {
                    continue;
                }
                var otherTile = pageObject as CLPSnapTileContainer;
                if(PageObject.UniqueID == otherTile.UniqueID)
                {
                    continue;
                }
                var deltaX = Math.Abs(PageObject.XPosition - otherTile.XPosition);
                var deltaYBottomSnap = Math.Abs(PageObject.YPosition - (otherTile.YPosition + otherTile.Height));
                var deltaYTopSnap = Math.Abs(otherTile.YPosition - (PageObject.YPosition + PageObject.Height));
                if(!(deltaX < 50))
                {
                    continue;
                }

                if (deltaYBottomSnap < 55)
                {
                    int oldCount = otherTile.NumberOfTiles;
                    otherTile.NumberOfTiles += NumberOfTiles;
                    PageObject.ParentPage.PageHistory.Push(new CLPTileHeightChanged(otherTile.UniqueID, otherTile.NumberOfTiles,
                                                                                    oldCount));

                    CLPServiceAgent.Instance.RemovePageObjectFromPage(PageObject);
                    break;
                }

                if (deltaYTopSnap < 55)
                {
                    int oldCount = NumberOfTiles;
                    NumberOfTiles += otherTile.NumberOfTiles;
                    PageObject.ParentPage.PageHistory.Push(new CLPTileHeightChanged(PageObject.UniqueID, NumberOfTiles,
                                                                                    oldCount));

                    CLPServiceAgent.Instance.RemovePageObjectFromPage(otherTile);
                    break;
                }
            }

            AddRemovePageObjectFromOtherObjects();
        }

        /// <summary>
        /// Gets the RemoveTileCommand command.
        /// </summary>
        public Command RemoveTileCommand { get; private set; }

        private void OnRemoveTileCommandExecute()
        {
            if(Tiles.Count > 1)
            {
                PageObject.ParentPage.PageHistory.Push(new CLPTileHeightChanged(PageObject.UniqueID, NumberOfTiles - 1,
                                                                                NumberOfTiles));
                NumberOfTiles--;
            }
        }

        /// <summary>
        /// Gets the DuplicateContainerCommand command.
        /// </summary>
        public Command DuplicateContainerCommand { get; private set; }

        private void OnDuplicateContainerCommandExecute()
        {
            CLPSnapTileContainer newSnapTile = PageObject.Duplicate() as CLPSnapTileContainer;
            double x = newSnapTile.XPosition + 80;
            double y = newSnapTile.YPosition;
            if(x > PageObject.ParentPage.PageWidth - PageObject.Width)
            {
                /* Want some distinguishable change in location. 
                 * Check to see if on the edge already or near the edge.
                 * If on the edge, also move down if possible.
                 */
                if(newSnapTile.XPosition == PageObject.ParentPage.PageWidth - PageObject.Width)
                {
                    y = newSnapTile.YPosition + 20;
                    if(y > PageObject.ParentPage.PageHeight - PageObject.Height)
                    {
                        y = PageObject.ParentPage.PageHeight - PageObject.Height;
                    }
                }
                x = PageObject.ParentPage.PageWidth - PageObject.Width;
            }

            newSnapTile.XPosition = x;
            newSnapTile.YPosition = y;

            CLPServiceAgent.Instance.AddPageObjectToPage(PageObject.ParentPage, newSnapTile);
        }

        public override bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        {
            return isMouseDown;
        }

    }
}
