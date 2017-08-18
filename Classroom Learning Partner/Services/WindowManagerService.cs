using System;
using Catel;

namespace Classroom_Learning_Partner.Services
{
    public enum Panels
    {
        NoPanel,
        NotebookPagesPanel,
        ProgressPanel,
        DisplaysPanel,
        PageInformationPanel
    }

    public class WindowManagerService : IWindowManagerService
    {
        private readonly IRoleService _roleService;
        
        #region Constructor

        public WindowManagerService(IRoleService roleService)
        {
            Argument.IsNotNull(() => roleService);

            _roleService = roleService;
            InitializePanels();
        }

        #endregion // Constructor

        #region Methods

        private void InitializePanels()
        {
            switch (_roleService.Role)
            {
                case ProgramRoles.Researcher:
                    LeftPanel = Panels.NotebookPagesPanel;
                    break;
                case ProgramRoles.Teacher:
                    LeftPanel = Panels.NotebookPagesPanel;
                    break;
                case ProgramRoles.Student:
                    LeftPanel = Panels.NotebookPagesPanel;
                    break;
                case ProgramRoles.Projector:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion // Methods

        #region IWindowManagerService Implementation

        #region Properties

        private Panels _leftPanel = Panels.NoPanel;

        public Panels LeftPanel
        {
            get => _leftPanel;
            set
            {
                if (_leftPanel == value)
                {
                    return;
                }

                _leftPanel = value;
                LeftPanelChanged.SafeInvoke(this);
            }
        }

        private Panels _rightPanel = Panels.NoPanel;

        public Panels RightPanel
        {
            get => _rightPanel;
            set
            {
                if (_rightPanel == value)
                {
                    return;
                }

                _rightPanel = value;
                RightPanelChanged.SafeInvoke(this);
            }
        }

        #endregion // Properties

        #region Events

        public event EventHandler<EventArgs> LeftPanelChanged;
        public event EventHandler<EventArgs> RightPanelChanged;

        #endregion // Events

        #endregion // IWindowManagerService Implementation

        private void TestEvents()
        {
            var numberOfSubscribedEvents = LeftPanelChanged?.GetInvocationList().Length;
        }
    }
}