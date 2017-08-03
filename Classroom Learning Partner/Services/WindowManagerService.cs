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

        private bool _iDisplaysPanelVisible;

        public bool IsDisplaysPanelVisible
        {
            get => _iDisplaysPanelVisible;
            set
            {
                if (_iDisplaysPanelVisible == value)
                {
                    return;
                }

                _iDisplaysPanelVisible = value;
                DisplaysPanelVisibleChanged.SafeInvoke(this);
            }
        }

        #endregion // Properties

        #region Events

        public event EventHandler<EventArgs> PageInformationPanelVisibleChanged;
        public event EventHandler<EventArgs> DisplaysPanelVisibleChanged;

        #endregion // Events

        #endregion // IWindowManagerService Implementation
    }
}