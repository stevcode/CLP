using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.ServiceModel;
using System.Windows.Threading;


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
        void SubmitPage(string page, string userName);

        [OperationContract(IsOneWay = true)]
        void LaserUpdate(Point pt);

        [OperationContract(IsOneWay = true)]
        void TurnOffLaser();

        [OperationContract(IsOneWay = true)]
        void BroadcastInk(List<string> strokesAdded, List<string> strokesRemoved, string pageUniqueID);
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

        public void SubmitPage(string s_page, string userName)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {

                    if (App.CurrentUserMode == App.UserMode.Instructor)
                    {
                        CLPPage page = (ObjectSerializer.ToObject(s_page) as CLPPage);
                        page.IsSubmission = true;
                        page.SubmitterName = userName;
                        CLPService.AddSubmission(page);
                    }
                    else if (App.CurrentUserMode == App.UserMode.Server)
                    {
                        pagecount++;
                        Console.WriteLine("page received");
                        Console.WriteLine("Page Count: " + pagecount.ToString());
                    }

                    return null;
                }, null);
        }

        public void LaserUpdate(Point pt)
        {

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {
                        AppMessages.UpdateLaserPointerPosition.Send(pt);
                    }
                    return null;
                }, null);

        }

        public void TurnOffLaser()
        {

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {
                        AppMessages.TurnOffLaser.Send();
                    }
                    return null;
                }, null);
        }

        public void BroadcastInk(List<string> strokesAdded, List<string> strokesRemoved, string pageUniqueID)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {
                        CLPPageViewModel pageViewModel = App.CurrentNotebookViewModel.GetPageByID(pageUniqueID);
                        if (pageViewModel != null)
                        {
                            foreach (var stringStroke in strokesAdded)
                            {
                                pageViewModel.OtherStrokes.Add(CLPPageViewModel.StringToStroke(stringStroke));
                            }
                            foreach (var stringStroke in strokesRemoved)
                            {
                                pageViewModel.OtherStrokes.Remove(CLPPageViewModel.StringToStroke(stringStroke));
                            }

                        }
                    }

                    return null;
                }, null);

        }
    
    }
}
