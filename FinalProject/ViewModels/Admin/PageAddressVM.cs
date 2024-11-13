using System.ComponentModel.DataAnnotations;

namespace FinalProject.ViewModels.Admin
{
    public class PageAddressVM
    {
        public int PageAddressId { get; set; }
        [Required(ErrorMessage = "Tên trang không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên trang không được vượt quá 100 ký tự.")]
        public string PageName { get; set; }
        [Required(ErrorMessage = "URL không được để trống.")]
        [StringLength(500, ErrorMessage = "URL không được vượt quá 500 ký tự.")]
        [RegularExpression(@"^\/[A-Za-z0-9\-\/]*$", ErrorMessage = "URL không hợp lệ. URL phải bắt đầu bằng '/' và chỉ chứa các ký tự chữ, số, dấu gạch ngang và dấu '/'.")]
        public string Url { get; set; }
    }
}
