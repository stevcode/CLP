namespace CLP.Entities
{
    public interface IConnectedPerson
    {
        string CurrentMachineName { get; set; }
        string CurrentMachineAddress { get; set; }
        bool IsConnected { get; set; }
    }
}