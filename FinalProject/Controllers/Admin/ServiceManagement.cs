using FinalProject.Services.Admin;
using FinalProject.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Controllers.Admin
{
    public class ServiceManagement : Controller
    {
        private readonly ServiceListService servicelistService;
        public ServiceManagement(ServiceListService servicelistService)
        {
            this.servicelistService = servicelistService;
        }
        public IActionResult ServiceListManagement(int pageNumber = 1, int pageSize = 7, string searchQuery = "")
        {
            int totalItems;
            List<ServiceListVM> services;

            if (string.IsNullOrEmpty(searchQuery))
            {
                totalItems = servicelistService.GetServiceListCount();
                services = servicelistService.GetServices(pageNumber, pageSize);
            }
            else
            {
                var searchResults = servicelistService.SearchService(searchQuery);
                totalItems = searchResults.Count;

                services = searchResults
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            var paginationModel = new PagedListVM<ServiceListVM>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = services
            };

            ViewBag.SearchQuery = searchQuery;

            return View("ServiceListManagement", paginationModel);
        }

        public IActionResult CreateService()
        {
            return View(new ServiceListVM());
        }

        [HttpPost]
        public IActionResult CreateService(ServiceListVM serviceListVM)
        {
            if (ModelState.IsValid)
            {
                if (servicelistService.AddService(serviceListVM))
                {
                    TempData["SuccessMessage"] = "Dịch vụ đã được thêm thành công!";
                    return RedirectToAction("ServiceListManagement");
                }

                TempData["FailMessage"] = "Không thể thêm dịch vụ!";
            }
            else
            {
                TempData["FailMessage"] = "Dữ liệu không hợp lệ!";
            }



            return View(serviceListVM);
        }

        public IActionResult EditService(int serviceId)
        {
            var userType = servicelistService.GetServiceById(serviceId);

            if (userType == null)
            {
                return NotFound();
            }

            return View(userType);
        }

        [HttpPost]
        public IActionResult UpdateService(ServiceListVM serviceListVM)
        {
            if (ModelState.IsValid)
            {
                var result = servicelistService.UpdateService(serviceListVM);

                if (result)
                {
                    TempData["SuccessMessage"] = "Cập nhật dịch vụ thành công!";
                    return RedirectToAction("EditService", new { serviceId = serviceListVM.ServiceId });
                }
                else
                {
                    TempData["FailMessage"] = "Dịch vụ không tồn tại!";
                }
            }

            TempData["FailMessage"] = "Dữ liệu không hợp lệ!";
            return RedirectToAction("EditService", new { serviceId = serviceListVM.ServiceId });
        }

        public IActionResult DeleteService(int serviceId)
        {
            if (!servicelistService.CanDeleteService(serviceId))
            {
                TempData["FailMessage"] = "Không thể xóa dịch vụ vì có bài viết đang dùng dịch vụ này!";
                return RedirectToAction("PageAddressManagement");
            }

            if (servicelistService.DeleteService(serviceId))
            {
                TempData["SuccessMessage"] = "Xóa dịch vụ thành công!";
            }
            else
            {
                TempData["FailMessage"] = "Trang không tồn tại!";
            }

            return RedirectToAction("ServiceListManagement");
        }
    }
}
