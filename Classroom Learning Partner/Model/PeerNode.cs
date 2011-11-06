using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;

namespace Classroom_Learning_Partner.Model
{
    public class PeerNode
    {
        public string Id { get; private set; }

        public ICLPMeshNetworkContract Channel;
        public ICLPMeshNetworkContract Host;

        private DuplexChannelFactory<ICLPMeshNetworkContract> _factory;
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

            var endpoint = new ServiceEndpoint(
                ContractDescription.GetContract(typeof(ICLPMeshNetworkContract)),
                binding,
                new EndpointAddress("net.p2p://Classroom_Learning_Partner.Model"));

            Host = new CLPMeshNetworkService();

            _factory = new DuplexChannelFactory<ICLPMeshNetworkContract>(
                new InstanceContext(Host),
                endpoint);

            var channel = _factory.CreateChannel();

            ((ICommunicationObject)channel).Open();

            // wait until after the channel is open to allow access.
            Channel = channel;
        }

        public void Stop()
        {
            _stopFlag.Set();
        }

        public void StopService()
        {
            //Channel.Disconnect()
            ((ICommunicationObject)Channel).Close();
            if (_factory != null)
                _factory.Close();
        }
    }
}
