using Catel.Runtime.Serialization.Binary;

namespace CLP.Entities.Old
{
    [RedirectType("CLP.Entities", "IConnectedPerson")]
    public interface IConnectedPerson
    {
        string CurrentMachineName { get; set; }
        string CurrentMachineAddress { get; set; }
        bool IsConnected { get; set; }
    }
}