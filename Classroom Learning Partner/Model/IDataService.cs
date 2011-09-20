using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classroom_Learning_Partner.Model
{
    public interface IDataService
    {
        void GetData(Action<DataItem, Exception> callback);
    }
}
