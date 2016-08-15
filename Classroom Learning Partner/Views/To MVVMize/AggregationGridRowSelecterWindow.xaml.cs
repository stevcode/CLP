using System.Collections.Generic;
using System.Windows;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for AggregationGridRowSelecterWindow.xaml
    /// </summary>
    public partial class AggregationGridRowSelecterWindow : Window
    {
        private readonly string _windowExplanationText =
            "Choose which Row you want to aggregate. Row 0 is the row that includes the headers of all the columns. You cannot aggregate Row 0. Row 1 starts after the column headers. Click the check box below if you want to clear all other auto-generated aggregation tables.";

        public AggregationGridRowSelecterWindow(List<string> choices)
        {
            Choices = choices;
            ClearOtherTables = false;
            DataContext = this;
            InitializeComponent();
        }

        public List<string> Choices { get; set; }
        public bool ClearOtherTables { get; set; }
        public string WindowExplanationText
        {
            get { return _windowExplanationText; }
        }
        public int SelectedRowIndex { get; set; }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
