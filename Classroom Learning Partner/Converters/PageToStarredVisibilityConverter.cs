using System;
using System.Globalization;
using System.Windows.Data;
using CLP.Models;

namespace Classroom_Learning_Partner.Converters
{
    public class PageToStarredVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var page = (ICLPPage)value;
            foreach(Tag t in page.PageTags) 
            {
                if(t.TagType.Name == "Starred")
                {
                    if(((string)t.Value[0].Value) == "Starred")
                    {
                        return "Visible";
                    }
                    return "Hidden";
                }
            }
            return "Hidden";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
