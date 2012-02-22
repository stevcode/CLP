using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    public interface ICLPPageObject
    {
        string PageObjectType { get; }

        ICLPPageObject Duplicate();
    }
}
