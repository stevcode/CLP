using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.ViewModels.Displays
{
    public interface IDisplayViewModel
    {
        string DisplayName { get; }

        string DisplayID { get; }

        bool IsOnProjector { get; set; }
        bool IsActive { get; set; }

        void AddPageToDisplay(CLPPageViewModel page);
    }
}
