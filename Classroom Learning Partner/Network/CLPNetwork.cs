using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using ServiceModelEx;

namespace Classroom_Learning_Partner
{
    public sealed class CLPNetwork : IDisposable
    {
        public string MachineName { get; private set; }
        public string UserName { get; set; }
        public ObservableCollection<ServiceHost> RunningServices { get; set; }
        public DiscoveredServices<IInstructorContract> DiscoveredInstructors { get; set; }
        public DiscoveredServices<IProjectorContract> DiscoveredProjectors { get; set; }

        private readonly AutoResetEvent _stopFlag = new AutoResetEvent(false);

        public CLPNetwork()
        {
            MachineName = Environment.MachineName;
            UserName = MachineName;
            RunningServices = new ObservableCollection<ServiceHost>();
        }

        public void Run()
        {
            StartNetworking();
            _stopFlag.WaitOne();
            StopNetworking();
        }

        public void StartNetworking()
        {
            App.MainWindowViewModel.OnlineStatus = "CONNECTING...";

            //ServiceHost host = DiscoveryFactory.CreateDiscoverableHost<InstructorService>();
            //host.Open();
            //RunningServices.Add(host);

            

            

            //var binding = new NetTcpBinding();

            //// Allow big arguments on messages. Allow ~500 MB message.
            //binding.MaxReceivedMessageSize = 500 * 1024 * 1024;
            //binding.MaxBufferPoolSize = 500 * 1024 * 1024;

            //// Allow unlimited time to send/receive a message. 
            //// It also prevents closing idle sessions.
            //binding.ReceiveTimeout = TimeSpan.MaxValue;
            //binding.SendTimeout = TimeSpan.MaxValue;
            //binding.OpenTimeout = TimeSpan.MaxValue;
            //binding.CloseTimeout = TimeSpan.MaxValue;
            //XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();

            //// Remove quotas limitations
            //quotas.MaxArrayLength = int.MaxValue;
            //quotas.MaxBytesPerRead = int.MaxValue;
            //quotas.MaxDepth = int.MaxValue;
            //quotas.MaxNameTableCharCount = int.MaxValue;
            //quotas.MaxStringContentLength = int.MaxValue;
            //binding.ReaderQuotas = quotas;


            ServiceHost host = null;
            switch(App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    break;
                case App.UserMode.Instructor:
                    host = DiscoveryFactory.CreateDiscoverableHost<InstructorService>();
                    break;
                case App.UserMode.Projector:
                    host = DiscoveryFactory.CreateDiscoverableHost<ProjectorService>();
                    break;
                case App.UserMode.Student:
                    break;
                default:
                    break;
            }

            if(host != null)
            {
                host.Open();
                RunningServices.Add(host);
            }

            DiscoverServices();
        }

        public void DiscoverServices()
        {
            switch(App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    break;
                case App.UserMode.Instructor:
                    DiscoveredProjectors = new DiscoveredServices<IProjectorContract>();
                    DiscoveredProjectors.Open();       
                    break;
                case App.UserMode.Projector:
                    break;
                case App.UserMode.Student:
                    DiscoveredInstructors = new DiscoveredServices<IInstructorContract>();
                    DiscoveredInstructors.Open();

                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        /* run your code here */
                        int n = 0;
                        while(DiscoveredInstructors.Addresses.Count() < 1)
                        {
                            Console.WriteLine("Loop Number: " + n);
                            n++;
                            Thread.Sleep(1000);
                        }
                        foreach(var address in DiscoveredInstructors.Addresses)
                        {
                            Console.WriteLine(address.ToString());
                        }
                    }).Start();
                    break;
                default:
                    break;
            }


            //student
            //instant discover first instructor/projector you find 
            //run DiscoveredServices<ITestingContract> discoveredServices = new DiscoveredServices<ITestingContract>();
            //for IInstructor. updates and adds to list of available instructors
            //run DiscoveredServices<ITestingContract> discoveredServices = new DiscoveredServices<ITestingContract>();
            //for IGroup, keep up to date list of available group members

            //instructor
            //instant discover first projector you find
            //run DiscoveredServices<ITestingContract> discoveredServices = new DiscoveredServices<ITestingContract>();
            //for IProjector. updates list of available projectors
            //either:
            //a) run DiscoveredServices<ITestingContract> discoveredServices = new DiscoveredServices<ITestingContract>();
            //for  IStudent, keep up to date list of all available students, can make proxy to sent to all or specific students.
            //b) ideally, when student logs in, it gives it's IStudent Service address and instructor just keeps that list.

            //projector - later
            //run DiscoveredServices<ITestingContract> discoveredServices = new DiscoveredServices<ITestingContract>();
            //for IStudent, keep up to date (or options B from instructor, whichever method it uses)
            //can give live "projector" feed to students...or possibly move this functionality to instructor. when 
            //instructor sets projector it also sets students' "projector"



            //For instant
            //EndpointAddress address = DiscoveryHelper.DiscoverAddress<ITestingContract>();
        }

        public void Stop()
        {
            _stopFlag.Set();
        }

        public void StopNetworking()
        {
            //host.close()
        }

        public void Dispose()
        {
            _stopFlag.Dispose();
        }
    }
}
