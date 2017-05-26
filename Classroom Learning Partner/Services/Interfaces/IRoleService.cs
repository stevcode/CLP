using System;

namespace Classroom_Learning_Partner.Services
{
    public interface IRoleService
    {
        ProgramRoles Role { get; set; }

        event EventHandler<EventArgs> RoleChanged;


        // OnRoleChange?.Invoke(value); see page 548, 563
        // p 557 for loop that would make for better async event?
    }
}