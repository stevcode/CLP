using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for AdvancedStagingView.xaml
    /// </summary>
    public partial class AdvancedStagingView
    {
        public enum StagingTypes
        {
            Starred,
            Correct,
            Incorrect,
            All,
            Teacher,
            None
        }

        public StagingTypes StagingType;

        public AdvancedStagingView()
        {
            StagingType = StagingTypes.None;
            InitializeComponent();
        }

        public void StarredSubmissions_Click(object sender, RoutedEventArgs e)
        {
            StagingType = StagingTypes.Starred;
            DialogResult = true;
        }

        public void CorrectSubmissions_Click(object sender, RoutedEventArgs e)
        {
            StagingType = StagingTypes.Correct;
            DialogResult = true;
        }

        public void IncorrectSubmissions_Click(object sender, RoutedEventArgs e)
        {
            StagingType = StagingTypes.Incorrect;
            DialogResult = true;
        }

        public void AllSubmissions_Click(object sender, RoutedEventArgs e)
        {
            StagingType = StagingTypes.All;
            DialogResult = true;
        }

        public void TeacherPage_Click(object sender, RoutedEventArgs e)
        {
            StagingType = StagingTypes.Teacher;
            DialogResult = true;
        }
    }
}
