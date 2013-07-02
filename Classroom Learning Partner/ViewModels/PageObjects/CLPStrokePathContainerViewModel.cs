using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;
using Microsoft.Ink;
using Stroke = System.Windows.Ink.Stroke;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPStrokePathContainerViewModel : ACLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPStrokePathContainerViewModel"/> class.
        /// </summary>
        public CLPStrokePathContainerViewModel(CLPStrokePathContainer container)
        {
            PageObject = container;

            if (container.InternalPageObject == null)
            {
                InternalType = "Blank";
            }
            else
            {
                InternalType = container.InternalPageObject.PageObjectType;
                container.InternalPageObject.ParentPage = PageObject.ParentPage;
                container.InternalPageObject.IsInternalPageObject = true;
            }

            if(IsStamped)
            {
                ScribblesToStrokePaths();
            }
            
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "StrokePathContainerVM"; } }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ICLPPageObject InternalPageObject
        {
            get { return GetValue<ICLPPageObject>(InternalPageObjectProperty); }
            set { SetValue(InternalPageObjectProperty, value); }
        }

        /// <summary>
        /// Register the InternalPageObject property so it is known in the class.
        /// </summary>
        public static readonly PropertyData InternalPageObjectProperty = RegisterProperty("InternalPageObject", typeof(ICLPPageObject));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsStrokePathsVisible
        {
            get { return GetValue<bool>(IsStrokePathsVisibleProperty); }
            set { SetValue(IsStrokePathsVisibleProperty, value); }
        }

        public static readonly PropertyData IsStrokePathsVisibleProperty = RegisterProperty("IsStrokePathsVisible", typeof(bool), false, (sender, e) => ((CLPStrokePathContainerViewModel)sender).OnStrokePathsVisibilityChanged());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsStamped
        {
            get { return GetValue<bool>(IsStampedProperty); }
            set { SetValue(IsStampedProperty, value); }
        }

        /// <summary>
        /// Register the IsStamped property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsStampedProperty = RegisterProperty("IsStamped", typeof(bool));

        private void OnStrokePathsVisibilityChanged()
        {
            if (IsStrokePathsVisible)
            {
                ScribblesToStrokePaths();
            }
            else
            {
                StrokePathViewModels.Clear();
            }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string InternalType
        {
            get { return GetValue<string>(InternalTypeProperty); }
            set { SetValue(InternalTypeProperty, value); }
        }

        public static readonly PropertyData InternalTypeProperty = RegisterProperty("InternalType", typeof(string));

        #region Methods

        private ObservableCollection<StrokePathViewModel> _strokePathViewModels = new ObservableCollection<StrokePathViewModel>();
        public ObservableCollection<StrokePathViewModel> StrokePathViewModels
        {
            get { return _strokePathViewModels; }
        }

        public void ScribblesToStrokePaths()
        {
            var clpStrokePathContainer = PageObject as CLPStrokePathContainer;
            if(clpStrokePathContainer != null)
            {
                StrokeCollection inkStrokes;

                if(!clpStrokePathContainer.SerializedStrokes.Any() &&
                           clpStrokePathContainer.ByteStrokes.Any())
                {
                    inkStrokes = CLPPage.BytesToStrokes(clpStrokePathContainer.ByteStrokes);
                }
                else
                {
                    inkStrokes = CLPPage.LoadInkStrokes(clpStrokePathContainer.SerializedStrokes);
                }


                foreach (Stroke stroke in inkStrokes)
                {
                    StylusPoint firstPoint = stroke.StylusPoints[0];

                    StreamGeometry geometry = new StreamGeometry();
                    using (StreamGeometryContext geometryContext = geometry.Open())
                    {
                        geometryContext.BeginFigure(new Point(firstPoint.X, firstPoint.Y), true, false);
                        foreach (StylusPoint point in stroke.StylusPoints)
                        {
                            geometryContext.LineTo(new Point(point.X, point.Y), true, true);
                        }
                    }
                    geometry.Freeze();

                    StrokePathViewModel strokePathViewModel = new StrokePathViewModel(geometry, (SolidColorBrush)new BrushConverter().ConvertFromString(stroke.DrawingAttributes.Color.ToString()), stroke.DrawingAttributes.Width);
                    StrokePathViewModels.Add(strokePathViewModel);
                }
            }
        }

        public override bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        {
            if(IsStamped || !isMouseDown)
            {
                if(IsBackground)
                {
                    if(App.MainWindowViewModel.IsAuthoring)
                    {
                        IsMouseOverShowEnabled = true;
                        if(!timerRunning)
                        {
                            timerRunning = true;
                            hoverTimer.Start();
                        }
                    }
                    else
                    {
                        IsMouseOverShowEnabled = false;
                        hoverTimer.Stop();
                        timerRunning = false;
                        hoverTimeElapsed = false;
                    }
                }
                else
                {
                    IsMouseOverShowEnabled = true;
                    if(!timerRunning)
                    {
                        timerRunning = true;
                        hoverTimer.Start();
                    }
                }

                return !hoverTimeElapsed;
            }

            hoverTimer.Stop();
            hoverTimeElapsed = false;
            timerRunning = false;
            IsMouseOverShowEnabled = false;
            return true;
        }

        #endregion //Methods

    }
}
