﻿using System.Windows;
using Catel.Windows;

namespace Classroom_Learning_Partner.Views
{
    public partial class ReportsView
    {
        public const double WINDOW_WIDTH = 600;

        public ReportsView()
            : base(DataWindowMode.Custom)
        {
            InitializeComponent();
            Top = SystemParameters.FullPrimaryScreenHeight / 2 - (ActualHeight / 2);
            Left = SystemParameters.FullPrimaryScreenWidth / 2 - (ActualWidth / 2);
        }
    }
}