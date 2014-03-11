using System.Collections.Generic;
using System.Windows;

namespace Classroom_Learning_Partner.Views.Modal_Windows
{
    /// <summary>
    /// Interaction logic for SimpleTextWindowView.xaml
    /// </summary>
    public partial class SimpleTextWindowView : Window
    {
        private readonly string _windowText;

        public SimpleTextWindowView(string title, string text)
        {
            _windowText = text;
            InitializeComponent();
            Title = title;
            DataContext = this;
        }

        public string WindowText { get{ return _windowText; } }
    }
}
