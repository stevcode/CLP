using System;
using Catel;

namespace Classroom_Learning_Partner.Services
{
    public class WindowManagerService : IWindowManagerService
    {
        #region IWindowManagerService Implementation

        #region Properties

        private bool _isPageInformationPanelVisible;

        public bool IsPageInformationPanelVisible
        {
            get => _isPageInformationPanelVisible;
            set
            {
                if (_isPageInformationPanelVisible == value)
                {
                    return;
                }

                _isPageInformationPanelVisible = value;
                PageInformationPanelVisibleChanged.SafeInvoke(this);
            }
        }

        #endregion // Properties

        #region Events

        public event EventHandler<EventArgs> PageInformationPanelVisibleChanged;

        #endregion // Events

        #endregion // IWindowManagerService Implementation
    }
}