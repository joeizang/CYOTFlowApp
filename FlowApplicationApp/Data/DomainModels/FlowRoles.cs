using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlowApplicationApp.Data.DomainModels;
    public class FlowRoles
    {
        public Guid Id { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public string RoleDescription { get; set; } = string.Empty;
    }