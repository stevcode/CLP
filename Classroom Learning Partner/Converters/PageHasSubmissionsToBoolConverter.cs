using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Classroom_Learning_Partner.ViewModels;
using CLP.Models;

namespace Classroom_Learning_Partner.Converters
{
    public class PageHasSubmissionsToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var page = value as ICLPPage;
            var notebookPagesPanel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if(page == null || notebookPagesPanel == null)
            {
                return false;
            }

            return notebookPagesPanel.Notebook.Submissions[page.UniqueID].Any();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}