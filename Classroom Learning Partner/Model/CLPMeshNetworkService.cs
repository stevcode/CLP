using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;


namespace Classroom_Learning_Partner.Model
{
    public class CLPMeshNetworkService : ICLPMeshNetworkContract
    {
        public void InitializeMesh()
        {
            throw new NotImplementedException();
        }

        public void Connect(string userName)
        {
            throw new NotImplementedException();
        }

        public void Disconnect(string userName)
        {
            throw new NotImplementedException();
        }

        public void SubmitPage(string s_page)
        {
            if (App.CurrentUserMode == App.UserMode.Instructor)
            {
                Console.WriteLine("page received");
                CLPPage page = (ObjectSerializer.ToObject(s_page) as CLPPage);
                if (!App.CurrentNotebookViewModel.SubmissionViewModels.ContainsKey(page.UniqueID))
                {
                    App.CurrentNotebookViewModel.SubmissionViewModels.Add(page.UniqueID, new ObservableCollection<CLPPageViewModel>());
                }
                App.CurrentNotebookViewModel.SubmissionViewModels[page.UniqueID].Add(new CLPPageViewModel(page));                
	    }
	}

        public void LaserUpdate(Point pt)
        {
            if (App.CurrentUserMode == App.UserMode.Projector)
            {
                AppMessages.UpdateLaserPointerPosition.Send(pt);
            }
        }
    }
}
