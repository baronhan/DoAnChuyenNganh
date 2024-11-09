namespace FinalProject.Data
{
    public class RoomPostResponse
    {
        public int? ResponseId { get; set; }
        public int? PostId { get; set; }
        public string? ResponseContent { get; set; }
        public DateTime? CreatedAt { get; set; }
        public virtual RoomPost RoomPost { get; set; }
    }
}
