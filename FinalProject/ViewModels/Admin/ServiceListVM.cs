using System.ComponentModel.DataAnnotations;

namespace FinalProject.ViewModels.Admin
{
    public class ServiceListVM
    {
        public int ServiceId { get; set; }
        [Required(ErrorMessage = "Tên dịch vụ là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Tên dịch vụ không được vượt quá 50 ký tự.")]
        public string ServiceName { get; set; }

        [Required(ErrorMessage = "Mô tả dịch vụ là bắt buộc.")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string ServiceDescription { get; set; }

        [Required(ErrorMessage = "Giá dịch vụ là bắt buộc.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá dịch vụ phải lớn hơn 0.")]
        public decimal ServicePrice { get; set; }

        [Required(ErrorMessage = "Thời lượng dịch vụ là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Thời lượng dịch vụ phải là ít nhất 1 ngày.")]
        public int ServiceTime { get; set; }
        public ServiceListVM () { }
        public ServiceListVM (int ServiceId, string ServiceName, string ServiceDescription, decimal ServicePrice, int ServiceTime)
        {
            this.ServiceId = ServiceId;
            this.ServiceName = ServiceName;
            this.ServiceDescription = ServiceDescription;
            this.ServicePrice = ServicePrice;
            this.ServiceTime = ServiceTime;
        }
    }
}
