using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Threading;
using Catel.Windows;
using CLP.Models;

namespace Classroom_Learning_Partner
{
    [ServiceContract]
    public interface IStudentContract : INotebookContract
    {
        [OperationContract]
        void TogglePenDownMode(bool isPenDownModeEnabled);
    }

    public class StudentService : IStudentContract
    {
        #region IStudentContract Members

        public void TogglePenDownMode(bool isPenDownModeEnabled)
        {
            //TODO: Steve - AutoSave here
            if(isPenDownModeEnabled)
            {
                PleaseWaitHelper.Show("The Teacher has disabled the pen.");
            }
            else
            {
                PleaseWaitHelper.Hide();
            }
        }

        #endregion

        #region INotebookContract Members

        public void ModifyPageInkStrokes(List<List<byte>> strokesAdded, List<List<byte>> strokesRemoved, string pageID)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                    {
                        CLP.Models.CLPPage page = notebook.GetNotebookPageByID(pageID);

                        if(page != null)
                        {
                            StrokeCollection strokesToRemove = CLPPage.BytesToStrokes(new ObservableCollection<List<byte>>(strokesRemoved));

                            var strokes =
                                from externalStroke in strokesToRemove
                                from stroke in page.InkStrokes
                                where stroke.GetStrokeUniqueID() == externalStroke.GetStrokeUniqueID()
                                select stroke;

                            StrokeCollection actualStrokesToRemove = new StrokeCollection(strokes.ToList());

                            page.InkStrokes.Remove(actualStrokesToRemove);

                            StrokeCollection strokesToAdd = CLPPage.BytesToStrokes(new ObservableCollection<List<byte>>(strokesAdded));
                            page.InkStrokes.Add(strokesToAdd);
                            break;
                        }
                    }
                    return null;
                }, null);
        }

        #endregion
    }
}
