namespace Classroom_Learning_Partner.Views.PageObjects
{
    using Catel.Windows.Controls;
    using System.Windows;
    using System;
    using Classroom_Learning_Partner.Views.Modal_Windows;
    using Classroom_Learning_Partner.ViewModels.PageObjects;

    /// <summary>
    /// Interaction logic for CLPInkRegionView.xaml.
    /// </summary>
    public partial class CLPInkRegionView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPInkRegionView"/> class.
        /// </summary>
        public CLPInkRegionView()
        {
            InitializeComponent();
            SkipSearchingForInfoBarMessageControl = true;
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPInkRegionViewModel);
        }

        public void EditInkRegion(object sender, RoutedEventArgs e)
        {
            CustomizeInkRegionView optionChooser = new CustomizeInkRegionView();
            optionChooser.Owner = Application.Current.MainWindow;
            optionChooser.ShowDialog();
            if (optionChooser.DialogResult == true)
            {
                string correct_answer = optionChooser.CorrectAnswer.Text;
                int selected_type = optionChooser.ExpectedType.SelectedIndex;

                this.correct_answer.Content = correct_answer;
                this.analysis_type.Content = selected_type.ToString();
            }
        }
    }
}
