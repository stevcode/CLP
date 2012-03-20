using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.ServiceModel.PeerResolvers;
using System.Xml;

namespace Classroom_Learning_Partner.Model
{
    public sealed class PeerNode : IDisposable
    {
        public string MachineName { get; private set; }
        public string UserName { get; set; }

        public ICLPMeshNetworkChannel Channel;
        public ICLPMeshNetworkContract Host;
        public IOnlineStatus OnlineStatusHandler;

        private DuplexChannelFactory<ICLPMeshNetworkChannel> _factory;
        private readonly AutoResetEvent _stopFlag = new AutoResetEvent(false);

        public PeerNode()
        {
            MachineName = Environment.MachineName;
            UserName = MachineName;
            App.MainWindowViewModel.SetTitleBarText("Connecting...");
        }

        public void Run()
        {
            StartService();
            _stopFlag.WaitOne();
            StopService();
        }

        public void StartService()
        {
            var binding = new NetPeerTcpBinding();
            binding.Security.Mode = SecurityMode.None;

            // Allow big arguments on messages. Allow ~500 MB message.
            binding.MaxReceivedMessageSize = 500 * 1024 * 1024;
            binding.MaxBufferPoolSize = 500 * 1024 * 1024;

            // Allow unlimited time to send/receive a message. 
            // It also prevents closing idle sessions.
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.SendTimeout = TimeSpan.MaxValue;

            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();

            // Remove quotas limitations
            quotas.MaxArrayLength = int.MaxValue;
            quotas.MaxBytesPerRead = int.MaxValue;
            quotas.MaxDepth = int.MaxValue;
            quotas.MaxNameTableCharCount = int.MaxValue;
            quotas.MaxStringContentLength = int.MaxValue;
            binding.ReaderQuotas = quotas;

            //binding.Resolver.Mode = PeerResolverMode.Pnrp;

            var endpoint = new ServiceEndpoint(
                ContractDescription.GetContract(typeof(ICLPMeshNetworkChannel)),
                binding,
                new EndpointAddress("net.p2p://Classroom_Learning_Partner.Model"));

            Host = new CLPMeshNetworkService();

            _factory = new DuplexChannelFactory<ICLPMeshNetworkChannel>(
                new InstanceContext(Host),
                endpoint);

            var channel = _factory.CreateChannel();

            channel.Open();

            // wait until after the channel is open to allow access.
            Channel = channel;

            OnlineStatusHandler = Channel.GetProperty<IOnlineStatus>();
            OnlineStatusHandler.Online += new EventHandler(OnlineStatusHandler_Online);
            OnlineStatusHandler.Offline += new EventHandler(OnlineStatusHandler_Offline);
            if (OnlineStatusHandler.IsOnline)
            {
                //Writing line to log is down below
                //This caused a race condition where two threads tried to write at the same time
                //Logger.Instance.WriteToLog("Connected to Mesh: " + DateTime.Now.ToLongTimeString());
                App.MainWindowViewModel.SetTitleBarText("");
                if (App.CurrentUserMode == App.UserMode.Student || App.CurrentUserMode == App.UserMode.Instructor)
                {
                    Channel.Connect(MachineName);  
                }
            }
        }

        void OnlineStatusHandler_Offline(object sender, EventArgs e)
        {
            Logger.Instance.WriteToLog("Disconnected from Mesh: " + DateTime.Now.ToLongTimeString());
            App.MainWindowViewModel.SetTitleBarText("");
        }

        void OnlineStatusHandler_Online(object sender, EventArgs e)
        {
            Logger.Instance.WriteToLog("Connected to Mesh: " + DateTime.Now.ToLongTimeString());
            App.MainWindowViewModel.SetTitleBarText("");
            if (App.CurrentUserMode == App.UserMode.Student || App.CurrentUserMode == App.UserMode.Instructor)
            {
                Channel.Connect(MachineName);
            }
        }

        public void Stop()
        {
            _stopFlag.Set();
        }

        public void StopService()
        {
            //Channel.Disconnect()
            Channel.Close();
            if (_factory != null)
            {
                _factory.Close();
            }
        }

        public void Dispose()
        {
            //steve - both of these added b/c of warning. remove if causes problems
            _stopFlag.Dispose();
            Channel.Dispose();
            _factory.Close();
        }
    }
}
