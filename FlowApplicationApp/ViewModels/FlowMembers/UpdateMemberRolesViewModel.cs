namespace FlowApplicationApp.ViewModels.FlowMembers;

public class UpdateMemberRolesViewModel
{
    public Guid MemberId { get; set; }
    
    public string MemberName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string ProfileImageUrl { get; set; } = string.Empty;
    
    public List<string> CurrentRoles { get; set; } = new();
    
    public List<RoleSelectionItem> AvailableRoles { get; set; } = new();
    
    public List<string> SelectedRoles { get; set; } = new();
    
    public bool IsSuccess { get; set; }
    
    public string Message { get; set; } = string.Empty;
}

public class RoleSelectionItem
{
    public string RoleName { get; set; } = string.Empty;
    
    public string RoleDescription { get; set; } = string.Empty;
    
    public bool IsSelected { get; set; }
}
