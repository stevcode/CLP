using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Catel.Collections;
using Catel.IoC;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;
using ServiceModelEx;

namespace Classroom_Learning_Partner.Services
{
    public enum ConnectionStatuses
    {
        Offline,
        Connecting,
        Listening,
        Found,
        Connected,
        LoggedIn,
        LoggedOut,
        Disconnected
    }

    public sealed class NetworkService : INetworkService
    {
        #region Constants

        private const string PROXY_BINDING_CONFIGURATION_NAME = "ProxyBinding";
        private const string STUDENT_CONTRACT_ENDPOINT_NAME = "NetTcpBinding_IStudentContract";

        #endregion // Constants

        #region Fields

        private readonly IDataService _dataService;
        private readonly AutoResetEvent _stopFlag = new AutoResetEvent(false);
        private static Thread _networkThread;

        #endregion // Fields

        #region Constructors

        public NetworkService()
        {
            _dataService = ServiceLocator.Default.ResolveType<IDataService>();
        }

        #endregion // Constructors

        #region INetworkService Overrides

        #region Properties

        public Person CurrentUser { get; set; }
        public string CurrentMachineAddress { get; private set; }
        public string CurrentMachineName => Environment.MachineName;
        public ConnectionStatuses CurrentConnectionStatus { get; private set; }

        public NetTcpBinding DefaultBinding { get; } = new NetTcpBinding(PROXY_BINDING_CONFIGURATION_NAME);

        public DiscoveredServices<IInstructorContract> DiscoveredInstructors { get; } = new DiscoveredServices<IInstructorContract>();
        public DiscoveredServices<IProjectorContract> DiscoveredProjectors { get; } = new DiscoveredServices<IProjectorContract>();
        public DiscoveredServices<IStudentContract> DiscoveredStudents { get; } = new DiscoveredServices<IStudentContract>();

        public IInstructorContract InstructorProxy { get; set; }
        public IProjectorContract ProjectorProxy { get; set; }

        public ServiceHost HostedService { get; private set; }

        #endregion // Properties

        #region Methods

        public void Connect()
        {
            _networkThread = new Thread(Run)
                             {
                                 IsBackground = true
                             };
            _networkThread.Start();
        }

        public void Disconnect()
        {
            Stop();
            _networkThread.Join();
        }

        public void Reconnect()
        {
            Stop();
            _networkThread.Join();
            _networkThread = null;
            _networkThread = new Thread(Run)
                             {
                                 IsBackground = true
                             };
            _networkThread.Start();
        }

        #endregion // Methods

        #endregion // INetworkService Overrides

        #region Methods

        private void Run()
        {
            StartNetworking();
            _stopFlag.WaitOne();
            StopNetworking();
        }

        private void Stop()
        {
            _stopFlag.Set();
        }

        private void StartNetworking()
        {
            CurrentConnectionStatus = ConnectionStatuses.Offline;

            ServiceHost host = null;
            switch (App.MainWindowViewModel.CurrentProgramMode)
            {
                case ProgramModes.Database:
                    break;
                case ProgramModes.Teacher:
                    host = DiscoveryFactory.CreateDiscoverableHost<InstructorService>();
                    CurrentConnectionStatus = ConnectionStatuses.Listening;
                    break;
                case ProgramModes.Projector:
                    host = DiscoveryFactory.CreateDiscoverableHost<ProjectorService>();
                    CurrentConnectionStatus = ConnectionStatuses.Listening;
                    break;
                case ProgramModes.Student:
                    host = DiscoveryFactory.CreateDiscoverableHost<StudentService>();
                    foreach (var endpoint in host.Description.Endpoints.Where(endpoint => endpoint.Name == STUDENT_CONTRACT_ENDPOINT_NAME))
                    {
                        CurrentMachineAddress = endpoint.Address.ToString();
                        break;
                    }
                    CurrentConnectionStatus = ConnectionStatuses.Listening;
                    break;
            }

            if (host != null)
            {
                host.Open();
                HostedService = host;
            }
            else
            {
                CurrentConnectionStatus = ConnectionStatuses.Offline;
            }

            DiscoverServices();
        }

        private void DiscoverServices()
        {
            switch (App.MainWindowViewModel.CurrentProgramMode)
            {
                case ProgramModes.Database:
                    break;
                case ProgramModes.Teacher:
                    DiscoveredProjectors.Open();
                    DiscoveredStudents.Open();

                    new Thread(() =>
                               {
                                   Thread.CurrentThread.IsBackground = true;
                                   while (!DiscoveredProjectors.Addresses.Any())
                                   {
                                       Thread.Sleep(1000);
                                   }

                                   try
                                   {
                                       ProjectorProxy = ChannelFactory<IProjectorContract>.CreateChannel(DefaultBinding, DiscoveredProjectors.Addresses[0]);

                                       CurrentConnectionStatus = ConnectionStatuses.Connected;
                                       App.MainWindowViewModel.IsProjectorFrozen = false;

                                       // TODO: Send, AllowLogin ping to projector 
                                   }
                                   catch (Exception)
                                   {
                                       CurrentConnectionStatus = ConnectionStatuses.Disconnected;
                                   }
                               }).Start();
                    break;
                case ProgramModes.Projector:
                    break;
                case ProgramModes.Student:
                    DiscoveredInstructors.Open();

                    new Thread(() =>
                               {
                                   Thread.CurrentThread.IsBackground = true;
                                   while (!DiscoveredInstructors.Addresses.Any())
                                   {
                                       Thread.Sleep(1000);
                                   }

                                   try
                                   {
                                       InstructorProxy = ChannelFactory<IInstructorContract>.CreateChannel(DefaultBinding, DiscoveredInstructors.Addresses[0]);

                                       CurrentConnectionStatus = ConnectionStatuses.Connected;
                                       var isNotebookOpen = !(App.MainWindowViewModel.Workspace is UserLoginWorkspaceViewModel);

                                       if (isNotebookOpen)
                                       {
                                           //if (App.Network.InstructorProxy == null)
                                           //{
                                           //    return;
                                           //}

                                           //var connectionString =
                                           //    App.Network.InstructorProxy.StudentLogin(App.MainWindowViewModel.CurrentUser.FullName,
                                           //                                             App.MainWindowViewModel.CurrentUser.ID,
                                           //                                             App.Network.CurrentMachineName,
                                           //                                             App.Network.CurrentMachineAddress);

                                           //if (connectionString == "connected")
                                           //{
                                           //    App.MainWindowViewModel.MajorRibbon.ConnectionStatus = ConnectionStatuses.LoggedIn;
                                           //}
                                       }
                                       else
                                       {
                                           var classRosterXml = InstructorProxy.GetClassRosterXml();
                                           if (string.IsNullOrWhiteSpace(classRosterXml))
                                           {
                                               return;
                                           }

                                           var classRoster = ASerializableBase.FromXmlString<ClassRoster>(classRosterXml);
                                           _dataService.SetCurrentClassRoster(classRoster);
                                           var students = classRoster.ListOfStudents;
                                           var workspace = App.MainWindowViewModel.Workspace as UserLoginWorkspaceViewModel;

                                           UIHelper.RunOnUI(() => workspace.AvailableStudents.AddRange(students));
                                       }
                                   }
                                   catch (Exception)
                                   {
                                       CurrentConnectionStatus = ConnectionStatuses.Disconnected;
                                   }
                               }).Start();
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

        private void StopNetworking()
        {
            if (InstructorProxy != null)
            {
                try
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    (InstructorProxy as ICommunicationObject).Close();
                    InstructorProxy = null;
                }
                catch (Exception)
                {
                    InstructorProxy = null;
                }
            }

            try
            {
                DiscoveredInstructors.Close();
            }
            catch (Exception)
            {
                // ignored
            }

            if (ProjectorProxy != null)
            {
                try
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    (ProjectorProxy as ICommunicationObject).Close();
                    ProjectorProxy = null;
                }
                catch (Exception)
                {
                    ProjectorProxy = null;
                }
            }

            try
            {
                DiscoveredProjectors.Close();
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                DiscoveredStudents.Close();
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                HostedService.Close();
                HostedService = null;
            }
            catch (Exception)
            {
                HostedService = null;
            }

            CurrentConnectionStatus = ConnectionStatuses.Disconnected;
        }

        #endregion // Methods

        #region Static Methods

        public static IStudentContract CreateStudentProxyFromMachineAddress(string machineAddress)
        {
            return CreateStudentProxyFromMachineAddress(new EndpointAddress(machineAddress));
        }

        public static IStudentContract CreateStudentProxyFromMachineAddress(EndpointAddress machineAddress)
        {
            var binding = new NetTcpBinding
            {
                Security = {
                                             Mode = SecurityMode.None
                                         }
            };
            var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(binding, machineAddress);

            return studentProxy;
        }

        #endregion // Static Methods
    }
}