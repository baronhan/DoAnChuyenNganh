namespace FinalProject.ViewModels.Admin
{
    public class PrivilegeVM
    {
        public int PrivilegeId { get; set; }
        public int UserTypeId { get; set; }
        public int PageAddressId { get; set; }
        public string PageName { get; set; }
        public bool IsPrivileged { get; set; }
    }
}
