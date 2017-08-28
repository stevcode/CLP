using System;
using Catel;

namespace Classroom_Learning_Partner.Services
{
    public class RoleService : IRoleService
    {
        #region Constructor

        public RoleService(ProgramRoles role)
        {
            _role = role;
        }

        #endregion // Constructor

        #region IRoleService Implementation 

        private ProgramRoles _role;

        public ProgramRoles Role
        {
            get => _role;
            set
            {
                if (_role == value)
                {
                    return;
                }

                _role = value;
                RoleChanged.SafeInvoke(this);
            }
        }

        public event EventHandler<EventArgs> RoleChanged;

        #endregion // IRoleService Implementation
    }
}