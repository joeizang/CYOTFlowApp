using FlowApplicationApp.Data.DomainModels;

namespace FlowApplicationApp.ViewModels.CodeOfConduct;

public class ManageVersionsViewModel
{
    public List<CodeOfConductDocument> Versions { get; set; } = new();
    public Guid? ActiveVersionId { get; set; }
}
