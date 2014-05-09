using System;
using System.Globalization;
using System.Windows.Data;
using Catel.IO;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner.Converters
{
    public class PageToThumbnailFilePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CLPPage page = value as CLPPage;
            if(page != null)
            {
                var pagesFolderPath = Path.Combine(App.CurrentNotebookCacheDirectory, "Pages");
                var thumbnailsFolderPath = Path.Combine(pagesFolderPath, "Thumbnails");
                return Path.Combine(thumbnailsFolderPath, "p;" + page.PageNumber + ";" + page.ID + ";" + page.DifferentiationLevel + ";" + page.VersionIndex + ".png");
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}