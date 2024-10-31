namespace FinalProject.Data;

public partial class PageAddress
{
    public int PageAddressId { get; set; }

    public string PageName { get; set; } = null!;

    public virtual ICollection<Privilege> Privileges { get; set; } = new List<Privilege>();
}
