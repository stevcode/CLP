using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;

namespace Classroom_Learning_Partner.ViewModels
{
    public abstract class APanelBaseViewModel : ViewModelBase, IPanel
    {
        #region Constructor

        public APanelBaseViewModel() { PanelResizeDragCommand = new Command<DragDeltaEventArgs>(OnPanelResizeDragCommandExecute); }

        #endregion //Constructor

        #region IPanel Members

        /// <summary>
        /// Whether the Panel is pinned to the same Z-Index as the Workspace.
        /// </summary>
        public bool IsPinned
        {
            get { return GetValue<bool>(IsPinnedProperty); }
            set { SetValue(IsPinnedProperty, value); }
        }

        public static readonly PropertyData IsPinnedProperty = RegisterProperty("IsPinned", typeof(bool), true);

        /// <summary>
        /// Visibility of Panel, True for Visible, False for Collapsed.
        /// </summary>
        public bool IsVisible
        {
            get { return GetValue<bool>(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public static readonly PropertyData IsVisibleProperty = RegisterProperty("IsVisible", typeof(bool), true);

        /// <summary>
        /// Can the Panel be resized.
        /// </summary>
        public bool IsResizable
        {
            get { return GetValue<bool>(IsResizableProperty); }
            set { SetValue(IsResizableProperty, value); }
        }

        public static readonly PropertyData IsResizableProperty = RegisterProperty("IsResizable", typeof(bool), true);

        /// <summary>
        /// Initial Length of the Panel, before any resizing.
        /// </summary>
        public virtual double InitialLength
        {
            get { return 250.0; }
        }

        /// <summary>
        /// Minimum Length of the Panel.
        /// </summary>
        public double MinLength
        {
            get { return GetValue<double>(MinLengthProperty); }
            set { SetValue(MinLengthProperty, value); }
        }

        public static readonly PropertyData MinLengthProperty = RegisterProperty("MinLength", typeof(double), 100.0);

        public double Length
        {
            get { return GetValue<double>(LengthProperty); }
            set { SetValue(LengthProperty, value); }
        }

        public static readonly PropertyData LengthProperty = RegisterProperty("Length", typeof(double), 250.0);

        /// <summary>
        /// The Panel's Location relative to the Workspace.
        /// </summary>
        public PanelLocations Location
        {
            get { return GetValue<PanelLocations>(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public static readonly PropertyData LocationProperty = RegisterProperty("Location", typeof(PanelLocations), PanelLocations.Left);

        #endregion

        #region Commands

        /// <summary>
        /// Resizes the panel.
        /// </summary>
        public Command<DragDeltaEventArgs> PanelResizeDragCommand { get; private set; }

        private void OnPanelResizeDragCommandExecute(DragDeltaEventArgs e)
        {
            if(!IsResizable ||
               App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Student)
            {
                return;
            }

            var isLeftOrTop = Location == PanelLocations.Left || Location == PanelLocations.Bottom;
            var newLength = isLeftOrTop ? Length + e.HorizontalChange : Length - e.HorizontalChange;
            if(newLength < MinLength)
            {
                newLength = MinLength;
            }

            var isLeftOrRight = Location == PanelLocations.Left || Location == PanelLocations.Right;
            var maxLength = isLeftOrRight ? Application.Current.MainWindow.ActualWidth - 100.0 : Application.Current.MainWindow.ActualHeight - 250.0;
            if(newLength > maxLength)
            {
                newLength = maxLength;
            }

            Length = newLength;
        }

        #endregion //Commands
    }
}