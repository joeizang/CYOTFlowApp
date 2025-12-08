using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlowApplicationApp.ViewModels.FlowMembers;
public record UpdateMemberRolesInputModel(Guid MemberId, List<string> Roles);
