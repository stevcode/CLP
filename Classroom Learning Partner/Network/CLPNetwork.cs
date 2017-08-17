using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;
using ServiceModelEx;
using ConnectionStatuses = Classroom_Learning_Partner.ViewModels.ConnectionStatuses;

namespace Classroom_Learning_Partner
{
    public sealed class CLPNetwork : IDisposable
    {
        public Person CurrentUser { get; set; }
        public string CurrentMachineAddress { get; set; }

        public string CurrentMachineName => Environment.MachineName;

        public ObservableCollection<ServiceHost> RunningServices { get; set; }
        public DiscoveredServices<IInstructorContract> DiscoveredInstructors { get; set; }
        public DiscoveredServices<IProjectorContract> DiscoveredProjectors { get; set; }

        public DiscoveredServices<IStudentContract> DiscoveredStudents { get; set; }

        public IInstructorContract InstructorProxy { get; set; }
        public IProjectorContract ProjectorProxy { get; set; }

        private readonly AutoResetEvent _stopFlag = new AutoResetEvent(false);
        public readonly NetTcpBinding DefaultBinding = new NetTcpBinding("ProxyBinding");

        public CLPNetwork()
        {
            CurrentUser = new Person();
            // TODO: Can you make DiscoveredServices async, so that you can be notified as soon as it's populated?
            // Possibly p 759-760, task.ContinueWith(), TaskContinuationOptions.OnlyOnRanToCompletion?
            DiscoveredProjectors = new DiscoveredServices<IProjectorContract>();
            DiscoveredInstructors = new DiscoveredServices<IInstructorContract>();
            DiscoveredStudents = new DiscoveredServices<IStudentContract>();
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
            App.MainWindowViewModel.MajorRibbon.ConnectionStatus = ConnectionStatuses.Offline;

            ServiceHost host = null;
            switch (App.MainWindowViewModel.CurrentProgramMode)
            {
                case ProgramRoles.Researcher:
                case ProgramRoles.Teacher:
                    host = DiscoveryFactory.CreateDiscoverableHost<InstructorService>();
                    App.MainWindowViewModel.MajorRibbon.ConnectionStatus = ConnectionStatuses.Listening;
                    break;
                case ProgramRoles.Projector:
                    host = DiscoveryFactory.CreateDiscoverableHost<ProjectorService>();
                    App.MainWindowViewModel.MajorRibbon.ConnectionStatus = ConnectionStatuses.Listening;
                    break;
                case ProgramRoles.Student:
                    host = DiscoveryFactory.CreateDiscoverableHost<StudentService>();
                    foreach (var endpoint in host.Description.Endpoints.Where(endpoint => endpoint.Name == "NetTcpBinding_IStudentContract"))
                    {
                        CurrentMachineAddress = endpoint.Address.ToString();
                        break;
                    }
                    App.MainWindowViewModel.MajorRibbon.ConnectionStatus = ConnectionStatuses.Listening;
                    break;
            }

            if (host != null)
            {
                host.Open();
                RunningServices.Add(host);
            }
            else
            {
                App.MainWindowViewModel.MajorRibbon.ConnectionStatus = ConnectionStatuses.Offline;
            }

            DiscoverServices();
        }

        public void DiscoverServices()
        {
            switch (App.MainWindowViewModel.CurrentProgramMode)
            {
                case ProgramRoles.Researcher:
                case ProgramRoles.Teacher:
                    DiscoveredProjectors.Open();
                    DiscoveredStudents.Open();
                    new Thread(() =>
                               {
                                   Thread.CurrentThread.IsBackground = true;
                                   while (!DiscoveredProjectors.Addresses.Any())
                                   {
                                       // TODO: Avoid calling Sleep in production code. Can be solved with an await Task.Delay()
                                       // p 777, possible solution, but probably primitive
                                       Thread.Sleep(1000);
                                   }

                                   try
                                   {
                                       ProjectorProxy = ChannelFactory<IProjectorContract>.CreateChannel(DefaultBinding,
                                                                                                         DiscoveredProjectors.Addresses[0]);

                                       App.MainWindowViewModel.MajorRibbon.ConnectionStatus = ConnectionStatuses.Connected;
                                       App.MainWindowViewModel.IsProjectorFrozen = false;

                                       //var notebookService = ServiceLocator.Default.ResolveType<INotebookService>();
                                       //if (notebookService == null)
                                       //{
                                       //    return;
                                       //}

                                       //if (notebookService.CurrentClassPeriod != null)
                                       //{
                                       //    var classPeriodString = ObjectSerializer.ToString(notebookService.CurrentClassPeriod);
                                       //    var classPeriod = CLPServiceAgent.Instance.Zip(classPeriodString);

                                       //    var classSubjectString = ObjectSerializer.ToString(notebookService.CurrentClassPeriod.ClassInformation);
                                       //    var classsubject = CLPServiceAgent.Instance.Zip(classSubjectString);

                                       //    //var newNotebook = App.MainWindowViewModel.OpenNotebooks.First().CopyForNewOwner(App.MainWindowViewModel.CurrentUser);
                                       //    var newNotebookString = ObjectSerializer.ToString(notebookService.OpenNotebooks.First(x => x.ID == notebookService.CurrentClassPeriod.NotebookID && x.OwnerID == App.MainWindowViewModel.CurrentUser.ID));
                                       //    var zippedNotebook = CLPServiceAgent.Instance.Zip(newNotebookString);
                                       //    ProjectorProxy.OpenClassPeriod(classPeriod, classsubject);
                                       //    ProjectorProxy.OpenPartialNotebook(zippedNotebook);
                                       //}
                                   }
                                   catch (Exception)
                                   {
                                       CLogger.AppendToLog("Failed to create Projector Proxy");
                                       App.MainWindowViewModel.MajorRibbon.ConnectionStatus = ConnectionStatuses.Disconnected;
                                   }
                               }).Start();
                    break;
                case ProgramRoles.Projector:
                    break;
                case ProgramRoles.Student:
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
                                       InstructorProxy = ChannelFactory<IInstructorContract>.CreateChannel(DefaultBinding,
                                                                                                           DiscoveredInstructors.Addresses[0]);

                                       var isNotebookOpen = !(App.MainWindowViewModel.Workspace is UserLoginWorkspaceViewModel);
                                       App.MainWindowViewModel.IsBackStageVisible = !isNotebookOpen;

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
                                           App.MainWindowViewModel.IsBackStageVisible = true;
                                           App.MainWindowViewModel.MajorRibbon.ConnectionStatus = ConnectionStatuses.Found;
                                           App.MainWindowViewModel.BackStage.CurrentNavigationPane = NavigationPanes.Open;
                                       }

                                       //               InstructorProxy.SendClassPeriod(CurrentMachineAddress);
                                   }
                                   catch (Exception)
                                   {
                                       CLogger.AppendToLog("Failed to create Instructor Proxy");
                                       App.MainWindowViewModel.MajorRibbon.ConnectionStatus = ConnectionStatuses.Disconnected;
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

        public void Stop() { _stopFlag.Set(); }

        public void StopNetworking()
        {
            if (InstructorProxy != null)
            {
                try
                {
                    (InstructorProxy as ICommunicationObject).Close();
                    InstructorProxy = null;
                }
                catch (Exception)
                {
                    InstructorProxy = null;
                }
            }

            if (ProjectorProxy != null)
            {
                try
                {
                    (ProjectorProxy as ICommunicationObject).Close();
                    ProjectorProxy = null;
                }
                catch (Exception)
                {
                    ProjectorProxy = null;
                }
            }

            foreach (var host in RunningServices)
            {
                host.Close();
            }
            RunningServices.Clear();
        }

        public void Dispose() { _stopFlag.Dispose(); }
    }
}