namespace FinalProject.ViewModels.Admin
{
    public class RoomPostManagementVM
    {
        public int PostId { get; set; }
        public string? RoomImage { get; set; }
        public string Address { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
    }
}
