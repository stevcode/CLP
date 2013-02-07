using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using CLP.Models;
using ServiceModelEx;

namespace Classroom_Learning_Partner
{
    public sealed class CLPNetwork : IDisposable
    {
        public Person CurrentUser { get; set; }
        public Group CurrentGroup { get; set; }
        public ObservableCollection<Person> ClassList { get; set; }

        public ObservableCollection<ServiceHost> RunningServices { get; set; }
        public DiscoveredServices<IInstructorContract> DiscoveredInstructors { get; set; }
        public DiscoveredServices<IProjectorContract> DiscoveredProjectors { get; set; }

        public IInstructorContract InstructorProxy { get; set; }
        public IProjectorContract ProjectorProxy { get; set; }

        private readonly AutoResetEvent _stopFlag = new AutoResetEvent(false);
        private NetTcpBinding defaultBinding = new NetTcpBinding("ProxyBinding");

        public CLPNetwork()
        {
            CurrentUser = new Person();
            CurrentGroup = new Group();
            ClassList = new ObservableCollection<Person>();
            DiscoveredProjectors = new DiscoveredServices<IProjectorContract>();
            DiscoveredInstructors = new DiscoveredServices<IInstructorContract>();
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
                    App.MainWindowViewModel.OnlineStatus = "LISTENING...";
                    break;
                case App.UserMode.Projector:
                    host = DiscoveryFactory.CreateDiscoverableHost<ProjectorService>();
                    App.MainWindowViewModel.OnlineStatus = "LISTENING...";
                    break;
                case App.UserMode.Student:
                    host = DiscoveryFactory.CreateDiscoverableHost<StudentService>();
                    string blah = host.Description.Endpoints[0].Address.ToString();
                    foreach(var endpoint in host.Description.Endpoints)
                    {
                        if(endpoint.Name == "NetTcpBinding_IStudentContract")
                        {
                            CurrentUser.CurrentMachineAddress = endpoint.Address.ToString();
                            break;
                        }
                    }
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
                    DiscoveredProjectors.Open();
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        while(DiscoveredProjectors.Addresses.Count() < 1)
                        {
                            Thread.Sleep(1000);
                        }
                        App.MainWindowViewModel.OnlineStatus = "CONNECTED";
                        try
                        {
                            ProjectorProxy = ChannelFactory<IProjectorContract>.CreateChannel(defaultBinding, DiscoveredProjectors.Addresses[0]);
                        }
                        catch(System.Exception ex)
                        {
                            Logger.Instance.WriteToLog("Failed to create Projector Proxy");
                        }
                    }).Start();
                    break;
                case App.UserMode.Projector:
                    break;
                case App.UserMode.Student:
                    DiscoveredInstructors.Open();

                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        while(DiscoveredInstructors.Addresses.Count() < 1)
                        {
                            Thread.Sleep(1000);
                        }
                        App.MainWindowViewModel.OnlineStatus = "CONNECTED";
                        try
                        {
                            InstructorProxy = ChannelFactory<IInstructorContract>.CreateChannel(defaultBinding, DiscoveredInstructors.Addresses[0]); 
                        }
                        catch(System.Exception ex)
                        {
                            Logger.Instance.WriteToLog("Failed to create Instructor Proxy");
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
            if (InstructorProxy != null)
            {
	            (InstructorProxy as ICommunicationObject).Close();
	            InstructorProxy = null;
            }

            if(ProjectorProxy != null)
            {
                (ProjectorProxy as ICommunicationObject).Close();
                ProjectorProxy = null;
            }

            foreach(var host in RunningServices)
            {
                host.Close();
            }
            RunningServices.Clear();
        }

        public void Dispose()
        {
            _stopFlag.Dispose();
        }
    }
}
