using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.ViewModels.Displays
{
    public interface IDisplayViewModel
    {
        public string DisplayName { get; }

        public bool IsOnProjector { get; set; }
        public bool IsActive { get; set; }

        public void AddPageToDisplay(CLPPage page);
    }
}
