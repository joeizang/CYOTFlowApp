using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace FlowApplicationApp.Data.DomainModels;
    public class FlowRoles : IdentityRole<Guid>
    {
        public string RoleName { get; set; } = string.Empty;

        public string RoleDescription { get; set; } = string.Empty;

        public DateOnly CreatedOn { get; set; }

        public DateOnly UpdatedOn { get; set; }

        public bool IsDeleted { get; set; }
    }