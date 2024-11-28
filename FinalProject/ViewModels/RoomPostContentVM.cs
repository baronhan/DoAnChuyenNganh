using System.ComponentModel.DataAnnotations;

namespace FinalProject.ViewModels
{
    public class RoomPostContentVM
    {
        public int PostId { get; set; }
        public string RoomName { get; set; } = "";
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số người ở phải là số nguyên dương.")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "Mô tả là bắt buộc.")]
        [StringLength(500, ErrorMessage = "Mô tả không được quá 500 ký tự.")]
        public string RoomDescription { get; set; }
        [Required(ErrorMessage = "Room price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phòng phải là một số hợp lệ.")]
        public decimal RoomPrice { get; set; }
        [Required(ErrorMessage = "Diện tích là bắt buộc.")]
        [Range(1, 50, ErrorMessage = "Diện tích phải từ 1m² đến 50m².")]
        public decimal RoomSize { get; set; }
        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        public string RoomAddress { get; set; }
        [Required(ErrorMessage = "Loại phòng là bắt buộc.")]
        public int RoomTypeId { get; set; }
        public int StatusId { get; set; }
        public int UserId { get; set; }
        public int RoomCoordinateId { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
