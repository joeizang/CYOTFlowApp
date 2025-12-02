using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace FlowApplicationApp.Data.DomainModels;
    public class FlowRoles : IdentityRole<Guid>
    {
        [StringLength(50, MinimumLength = 5)]
        [Required]
        public string RoleName { get; set; } = string.Empty;
        
        [StringLength(500, MinimumLength = 5)]
        public string RoleDescription { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public bool IsDeleted { get; set; }
    }