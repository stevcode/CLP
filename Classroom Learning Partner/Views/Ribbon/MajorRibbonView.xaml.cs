using System.Windows.Input;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for MajorRibbonView.xaml</summary>
    public partial class MajorRibbonView
    {
        public MajorRibbonView()
        {
            InitializeComponent();
        }

        private double _scrollStartX;
        private double _scrollStartXOffset;
        private bool _canScroll;

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (ScrollViewer.IsMouseOver)
            {
                // Save starting point, used later when determining how much to scroll.
                _scrollStartX = e.GetPosition(this).X;
                _scrollStartXOffset = ScrollViewer.HorizontalOffset;

                _canScroll = ScrollViewer.ExtentWidth > ScrollViewer.ViewportWidth;
            }

            base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (ScrollViewer.IsMouseOver && _canScroll || IsMouseCaptured)
            {
                // Get the new scroll position.
                var xPosition = e.GetPosition(this).X;

                // Determine the new amount to scroll.
                var deltaX = (xPosition > _scrollStartX) ? -(xPosition - _scrollStartX) : (_scrollStartX - xPosition);
                if (deltaX > 1.0)
                {
                    CaptureMouse();
                }

                // Scroll to the new position.
                ScrollViewer.ScrollToHorizontalOffset(_scrollStartXOffset + deltaX);
            }

            base.OnPreviewMouseMove(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            _canScroll = false;
            ReleaseMouseCapture();
            base.OnPreviewMouseUp(e);
        }
    }
}