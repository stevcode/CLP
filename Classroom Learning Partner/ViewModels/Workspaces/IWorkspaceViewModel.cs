﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Catel.MVVM;

namespace Classroom_Learning_Partner.ViewModels.Workspaces
{
    public interface IWorkspaceViewModel : IViewModel
    {
        string WorkspaceName { get; }
    }
}
