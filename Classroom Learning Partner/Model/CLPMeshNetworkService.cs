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
    [ServiceContract(CallbackContract = typeof(ICLPMeshNetworkContract))]
    public interface ICLPMeshNetworkContract
    {
        [OperationContract(IsOneWay = true)]
        void Connect(string userName);

        [OperationContract(IsOneWay = true)]
        void Disconnect(string userName);

        [OperationContract(IsOneWay = true)]
        void SubmitPage(string page);

        [OperationContract(IsOneWay = true)]
        void LaserUpdate(Point pt);
    }

    public interface ICLPMeshNetworkChannel : ICLPMeshNetworkContract, IClientChannel
    {
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CLPMeshNetworkService : ICLPMeshNetworkContract
    {
        private ICLPServiceAgent CLPService = new CLPServiceAgent();
        int pagecount = 0;

        public void Connect(string userName)
        {
            if (App.CurrentUserMode == App.UserMode.Server)
            {
                Console.WriteLine("Machine Connected: " + userName);
            }
        }

        public void Disconnect(string userName)
        {
            if (App.CurrentUserMode == App.UserMode.Server)
            {
                Console.WriteLine("Machine Disconnected: " + userName);
            }
        }

        public void SubmitPage(string s_page)
        {

            if (App.CurrentUserMode == App.UserMode.Instructor)
            {
                Console.WriteLine("page received");
                Console.WriteLine(s_page);
                CLPPage page = (ObjectSerializer.ToObject(s_page) as CLPPage);
                CLPService.AddSubmission(page);
            }
            else if (App.CurrentUserMode == App.UserMode.Server)
            {
                pagecount++;
                Console.WriteLine("page received");
                Console.WriteLine("Page Count: " + pagecount.ToString());
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
