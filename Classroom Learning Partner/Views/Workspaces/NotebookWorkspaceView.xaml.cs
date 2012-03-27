using Classroom_Learning_Partner.ViewModels.Workspaces;
using Classroom_Learning_Partner.ViewModels;
using System;
using Classroom_Learning_Partner.ViewModels.Displays;

namespace Classroom_Learning_Partner.Views.Workspaces
{

    /// <summary>
    /// Interaction logic for NotebookWorkspaceView.xaml.
    /// </summary>
    public partial class NotebookWorkspaceView : Catel.Windows.Controls.UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookWorkspaceView"/> class.
        /// </summary>
        public NotebookWorkspaceView()
        {
            InitializeComponent();
            SkipSearchingForInfoBarMessageControl = true;

            Display.DataContextChanged += new System.Windows.DependencyPropertyChangedEventHandler(Display_DataContextChanged);
        }

        void Display_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            Console.WriteLine("DataContext for Display changed to display with ID: " + (Display.DataContext as IDisplayViewModel).DisplayID);
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(NotebookWorkspaceViewModel);
        }
    }
}
