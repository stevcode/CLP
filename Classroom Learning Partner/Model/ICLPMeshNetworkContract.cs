using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Classroom_Learning_Partner.Model
{
    [ServiceContract(CallbackContract = typeof(ICLPMeshNetworkContract))]
    public interface ICLPMeshNetworkContract
    {
        [OperationContract(IsOneWay = true)]
        void InitializeMesh();

        [OperationContract(IsOneWay = true)]
        void Connect(string userName);

        [OperationContract(IsOneWay = true)]
        void Disconnect(string userName);
    }
}
