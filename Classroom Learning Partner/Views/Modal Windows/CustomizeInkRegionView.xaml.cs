﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Classroom_Learning_Partner.Views.Modal_Windows
{
    /// <summary>
    /// Interaction logic for CustomizeInkRegionView.xaml
    /// </summary>
    public partial class CustomizeInkRegionView : Window
    {
        public CustomizeInkRegionView()
        {
            InitializeComponent();
        }

        public CustomizeInkRegionView(int AnalysisType, string CorrectAnswer)
        {
            this.ExpectedType.SelectedIndex = AnalysisType;
            this.CorrectAnswer.Text = CorrectAnswer;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}