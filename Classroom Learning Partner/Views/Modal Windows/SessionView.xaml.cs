﻿using System.Windows;
using Catel.Windows;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for SessionView.xaml</summary>
    public partial class SessionView
    {
        public const double WINDOW_HEIGHT = 650;
        public const double WINDOW_WIDTH = 350;

        public SessionView()
            : base(DataWindowMode.Custom)
        {
            InitializeComponent();
            Top = SystemParameters.FullPrimaryScreenHeight / 2 - (ActualHeight / 2);
            Left = SystemParameters.FullPrimaryScreenWidth / 2 - (ActualWidth / 2);
        }
    }
}