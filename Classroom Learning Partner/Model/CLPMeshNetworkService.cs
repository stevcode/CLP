using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.ServiceModel;



namespace Classroom_Learning_Partner.Model
{
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CLPMeshNetworkService : ICLPMeshNetworkContract
    {
        private ICLPServiceAgent CLPService = new CLPServiceAgent();

        public void Connect(string userName)
        {
            Console.WriteLine("Machine Connected: " + userName);
        }

        public void Disconnect(string userName)
        {
            Console.WriteLine("Machine Disconnected: " + userName);
        }

        public void SubmitPage(string s_page)
        {
            //recieve page
            //App.PeerNode.channel

            if (App.CurrentUserMode == App.UserMode.Instructor)
            {
                Console.WriteLine("page received");
                Console.WriteLine(s_page);
                CLPPage page = (ObjectSerializer.ToObject(s_page) as CLPPage);
                CLPService.AddSubmission(page);
            }
	}

        public void SaveNotebookDB(string s_notebook)
        {
            //recieve notebook
            //App.PeerNode.channel

            if (App.CurrentUserMode == App.UserMode.Instructor)
            {
                Console.WriteLine("Notebook save requtest received");
                Console.WriteLine(s_notebook);
                //DB call
                CLPNotebook notebook = (ObjectSerializer.ToObject(s_notebook) as CLPNotebook);
                CLPService.SaveNotebookDB(notebook);
               
            }
        }

        public void LaserUpdate(Point pt)
        {
            if (App.CurrentUserMode == App.UserMode.Projector)
            {
                //AppMessages.UpdateLaserPointerPosition.Send(pt);
                Console.WriteLine(pt.ToString());
            }
        }
    }
}
