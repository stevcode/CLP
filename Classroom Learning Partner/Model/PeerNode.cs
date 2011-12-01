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
    public class PeerNode
    {
        public string Id { get; private set; }

        public ICLPMeshNetworkChannel Channel;
        public ICLPMeshNetworkContract Host;
        public IOnlineStatus OnlineStatusHandler;

        private DuplexChannelFactory<ICLPMeshNetworkChannel> _factory;
        private readonly AutoResetEvent _stopFlag = new AutoResetEvent(false);

        public PeerNode()
        {
            Id = new Guid().ToString();
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
            // From MSDN: To prevent the service from aborting idle sessions prematurely increase the Receive timeout on the service endpoint's binding.’
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

            //Host = new InstanceContext(new CLPMeshNetworkService());
            Host = new CLPMeshNetworkService();

            _factory = new DuplexChannelFactory<ICLPMeshNetworkChannel>(
                new InstanceContext(Host),
                endpoint);

            var channel = _factory.CreateChannel();
            OnlineStatusHandler = channel.GetProperty<IOnlineStatus>();
            OnlineStatusHandler.Online += new EventHandler(OnlineStatusHandler_Online);
            OnlineStatusHandler.Offline += new EventHandler(OnlineStatusHandler_Offline);

            

            channel.Open();

            // wait until after the channel is open to allow access.
            Console.WriteLine("channel assigned");
            Channel = channel;
        }

        void OnlineStatusHandler_Offline(object sender, EventArgs e)
        {
            Console.WriteLine("Offline");
        }

        void OnlineStatusHandler_Online(object sender, EventArgs e)
        {
            Console.WriteLine("Online");
           // Channel.Connect(Id);
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
                _factory.Close();
        }
    }
}
