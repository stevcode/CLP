﻿using System;
using System.Globalization;
using System.Windows.Data;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Converters
{
    public class CurrentWorkspaceToButtonEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentWorkspace = value as IWorkspaceViewModel;
            var desiredWorkspace = parameter as Type;
            return currentWorkspace != null && currentWorkspace.GetType() == desiredWorkspace;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}