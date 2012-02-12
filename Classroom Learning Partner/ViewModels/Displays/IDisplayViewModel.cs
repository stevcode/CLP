using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classroom_Learning_Partner.ViewModels.Displays
{
    public interface IDisplayViewModel
    {
        public string Name { get; }

        public bool IsOnProjector { get; set; }
    }
}
