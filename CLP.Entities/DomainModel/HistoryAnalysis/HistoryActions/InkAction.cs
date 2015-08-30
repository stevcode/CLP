using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Entities
{
    public static class InkCodedActions
    {
        public enum InkActions
        {
            Change,
            Add,
            Erase,
            Ignore
        }

        public enum InkLocations
        {
            None,
            Over,
            Left,
            Right,
            Above,
            Below
        }

  

    
    }
}
