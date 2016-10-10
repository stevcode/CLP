using System;
using System.Globalization;
using System.Windows.Data;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities.Old;

namespace Classroom_Learning_Partner.Converters
{
    public class PageToStarredVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var page = ((StudentProgressInfo)value).Page;
            if(page != null)
            {
                foreach(var t in page.Tags)
                {
                    if(t is StarredTag)
                    {
                        if(t.Value == "Starred")
                        {
                            return "Visible";
                        }
                        return "Hidden";
                    }
                }
            }
            return "Hidden";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}