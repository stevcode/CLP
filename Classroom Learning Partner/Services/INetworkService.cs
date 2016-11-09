using System.ServiceModel;
using CLP.Entities;
using ServiceModelEx;

namespace Classroom_Learning_Partner.Services
{
    public interface INetworkService
    {
        Person CurrentUser { get; set; }
        string CurrentMachineAddress { get; }
        string CurrentMachineName { get; }
        ConnectionStatuses CurrentConnectionStatus { get; }

        NetTcpBinding DefaultBinding { get; }

        DiscoveredServices<IInstructorContract> DiscoveredInstructors { get; }
        DiscoveredServices<IProjectorContract> DiscoveredProjectors { get; }
        DiscoveredServices<IStudentContract> DiscoveredStudents { get; }

        IInstructorContract InstructorProxy { get; set; }
        IProjectorContract ProjectorProxy { get; set; }

        ServiceHost HostedService { get; }

        void Connect();
        void Disconnect();
        void Reconnect();
    }
}