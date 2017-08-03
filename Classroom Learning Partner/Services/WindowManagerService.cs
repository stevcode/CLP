using System;
using Catel;

namespace Classroom_Learning_Partner.Services
{
    public class WindowManagerService : IWindowManagerService
    {
        #region IWindowManagerService Implementation

        #region Properties

        private bool _isNotebookPagesPanelVisible;

        public bool IsNotebookPagesPanelVisible
        {
            get => _isNotebookPagesPanelVisible;
            set
            {
                if (_isNotebookPagesPanelVisible == value)
                {
                    return;
                }

                _isNotebookPagesPanelVisible = value;
                NotebookPagesPanelVisibleChanged.SafeInvoke(this);
            }
        }

        private bool _isProgressPanelVisible;

        public bool IsProgressPanelVisible
        {
            get => _isProgressPanelVisible;
            set
            {
                if (_isProgressPanelVisible == value)
                {
                    return;
                }

                _isProgressPanelVisible = value;
                ProgressPanelVisibleChanged.SafeInvoke(this);
            }
        }

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

        private bool _isDisplaysPanelVisible;

        public bool IsDisplaysPanelVisible
        {
            get => _isDisplaysPanelVisible;
            set
            {
                if (_isDisplaysPanelVisible == value)
                {
                    return;
                }

                _isDisplaysPanelVisible = value;
                DisplaysPanelVisibleChanged.SafeInvoke(this);
            }
        }

        #endregion // Properties

        #region Events

        public event EventHandler<EventArgs> NotebookPagesPanelVisibleChanged;
        public event EventHandler<EventArgs> ProgressPanelVisibleChanged;

        public event EventHandler<EventArgs> PageInformationPanelVisibleChanged;
        public event EventHandler<EventArgs> DisplaysPanelVisibleChanged;

        #endregion // Events

        #endregion // IWindowManagerService Implementation
    }
}