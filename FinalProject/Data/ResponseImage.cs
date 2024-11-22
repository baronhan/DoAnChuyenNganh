namespace FinalProject.Data
{
    public class ResponseImage
    {
        public int ResponseImageId { get; set; }
        public int ResponseId { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual Response Response { get; set; }
    }
}
