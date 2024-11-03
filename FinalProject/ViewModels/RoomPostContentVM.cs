using System.ComponentModel.DataAnnotations;

namespace FinalProject.ViewModels
{
    public class RoomPostContentVM
    {
        public int PostId { get; set; }
        public string RoomName { get; set; } = "";
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "Room description is required.")]
        [StringLength(500, ErrorMessage = "Room description cannot exceed 500 characters.")]
        public string RoomDescription { get; set; }
        [Required(ErrorMessage = "Room price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Room price must be a positive number.")]
        public decimal RoomPrice { get; set; }
        [Required(ErrorMessage = "Room size is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Room size must be at least 1 square meter.")]
        public decimal RoomSize { get; set; }
        [Required(ErrorMessage = "Room address is required.")]
        [StringLength(200, ErrorMessage = "Room address cannot exceed 200 characters.")]
        public string RoomAddress { get; set; }
        public int RoomTypeId { get; set; }
        public int StatusId { get; set; }
        public int UserId { get; set; }
        public int RoomCoordinateId { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
