using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;
using ServiceModelEx;

namespace Classroom_Learning_Partner
{
    public sealed class CLPNetwork : IDisposable
    {
        public Person CurrentUser { get; set; }
        public ObservableCollection<ServiceHost> RunningServices { get; set; }
        public DiscoveredServices<IInstructorContract> DiscoveredInstructors { get; set; }
        public DiscoveredServices<IProjectorContract> DiscoveredProjectors { get; set; }

        public IInstructorContract InstructorProxy { get; set; }
        public IProjectorContract ProjectorProxy { get; set; }

        private readonly AutoResetEvent _stopFlag = new AutoResetEvent(false);
        public NetTcpBinding DefaultBinding = new NetTcpBinding("ProxyBinding");

        public CLPNetwork()
        {
            CurrentUser = new Person();
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
                    foreach(var endpoint in host.Description.Endpoints)
                    {
                        if(endpoint.Name == "NetTcpBinding_IStudentContract")
                        {
                     //       CurrentUser.CurrentMachineAddress = endpoint.Address.ToString();
                            break;
                        }
                    }
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
                        while(!DiscoveredProjectors.Addresses.Any())
                        {
                            Thread.Sleep(1000);
                        }
                        
                        try
                        {
                            ProjectorProxy = ChannelFactory<IProjectorContract>.CreateChannel(DefaultBinding, DiscoveredProjectors.Addresses[0]);
                            App.MainWindowViewModel.OnlineStatus = "CONNECTED";
                            App.MainWindowViewModel.Ribbon.IsProjectorOn = true;
                        }
                        catch(Exception)
                        {
                            Logger.Instance.WriteToLog("Failed to create Projector Proxy");
                            App.MainWindowViewModel.OnlineStatus = "FAILED TO CONNECT";
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
                        while(!DiscoveredInstructors.Addresses.Any())
                        {
                            Thread.Sleep(1000);
                        }

                        try
                        {
                            InstructorProxy = ChannelFactory<IInstructorContract>.CreateChannel(DefaultBinding, DiscoveredInstructors.Addresses[0]);
                            App.MainWindowViewModel.OnlineStatus = "CONNECTED - As " + App.MainWindowViewModel.CurrentUser.FullName;
                        }
                        catch(Exception)
                        {
                            Logger.Instance.WriteToLog("Failed to create Instructor Proxy");
                            App.MainWindowViewModel.OnlineStatus = "FAILED TO CONNECT";
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

        public void Stop()
        {
            _stopFlag.Set();
        }

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
	                
                }
            }

            if(ProjectorProxy != null)
            {
                try
                {
                    (ProjectorProxy as ICommunicationObject).Close();
                    ProjectorProxy = null;
                }
                catch (Exception)
                {
	                
                }
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
