using FinalProject.Data;
using FinalProject.Services.Admin;
using FinalProject.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Controllers.Admin
{
    [Authorize]
    public class AccessManagement : Controller
    {
        private readonly AccessManagementService accessManagementService;

        public AccessManagement(AccessManagementService accessManagementService) 
        {
            this.accessManagementService = accessManagementService;
        }

        public IActionResult UserTypeManagement()
        {
            var userTypeList = accessManagementService.GetAllUserType();

            return View(userTypeList);
        }

        public IActionResult EditUserType(int userTypeId)
        {
            var userType = accessManagementService.GetUserTypeById(userTypeId);

            if (userType == null)
            {
                return NotFound();
            }

            return View(userType);
        }

        [HttpPost]
        public IActionResult UpdateUserType(UserTypeVM userTypeVM)
        {
            if (ModelState.IsValid)
            {
                var result = accessManagementService.UpdateUserType(userTypeVM);

                if (result)  
                {
                    TempData["SuccessMessage"] = "Cập nhật nhóm người dùng thành công!";
                    return RedirectToAction("UserTypeManagement");
                }
                else 
                {
                    TempData["FailMessage"] = "Nhóm người dùng không tồn tại!";
                }
            }

            TempData["FailMessage"] = "Dữ liệu không hợp lệ!";
            return RedirectToAction("UserTypeManagement");
        }

        public IActionResult DeleteUserType(int userTypeId)
        {
            if (!accessManagementService.CanDeleteUserType(userTypeId))
            {
                TempData["FailMessage"] = "Không thể xóa nhóm người dùng vì có người dùng đang sử dụng nhóm này!";
                return RedirectToAction("UserTypeManagement");
            }

            if (accessManagementService.DeleteUserType(userTypeId))
            {
                TempData["SuccessMessage"] = "Xóa nhóm người dùng thành công!";
            }
            else
            {
                TempData["FailMessage"] = "Nhóm người dùng không tồn tại!";
            }

            return RedirectToAction("UserTypeManagement");
        }

        public IActionResult CreateUserType()
        {
            return View(new UserTypeVM());
        }

        [HttpPost]
        public IActionResult CreateUserType(UserTypeVM userTypeVM)
        {
            if (ModelState.IsValid)
            {
                if (accessManagementService.AddUserType(userTypeVM))
                {
                    TempData["SuccessMessage"] = "Nhóm người dùng đã được thêm thành công!";
                    return RedirectToAction("UserTypeManagement");
                }

                TempData["FailMessage"] = "Không thể thêm nhóm người dùng!";
            }
            else
            {
                TempData["FailMessage"] = "Dữ liệu không hợp lệ!";
            }

            return View(userTypeVM);
        }

        public IActionResult SearchUserType(string searchQuery = "")
        {
            var results = accessManagementService.SearchUserTypes(searchQuery ?? string.Empty);
            ViewBag.SearchQuery = searchQuery; 

            return View(results ?? new List<UserTypeVM>());
        }

        [HttpPost]
        public IActionResult SearchUserTypePost(string searchQuery)
        {
            var results = accessManagementService.SearchUserTypes(searchQuery ?? string.Empty);
            ViewBag.SearchQuery = searchQuery; 

            return View("SearchUserType", results ?? new List<UserTypeVM>());
        }

        public IActionResult PageAddressManagement(int pageNumber = 1, int pageSize = 7, string searchQuery = "")
        {
            int totalItems;
            List<PageAddressVM> pageAddress;

            if (string.IsNullOrEmpty(searchQuery))
            {
                totalItems = accessManagementService.GetPageAddressCount();
                pageAddress = accessManagementService.GetPageAddresses(pageNumber, pageSize);
            }
            else
            {
                var searchResults = accessManagementService.SearchPageAddress(searchQuery);
                totalItems = searchResults.Count;
                pageAddress = searchResults
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            var paginationModel = new PagedListVM<PageAddressVM>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = pageAddress
            };

            ViewBag.SearchQuery = searchQuery;
            return View("PageAddressManagement", paginationModel);
        }

        public IActionResult CreatePageAddress()
        {
            return View(new PageAddressVM());
        }

        [HttpPost]
        public IActionResult CreatePageAddress(PageAddressVM pageAddressVM)
        {
            if (ModelState.IsValid)
            {
                if (accessManagementService.AddPageAddress(pageAddressVM))
                {
                    TempData["SuccessMessage"] = "Trang đã được thêm thành công!";
                    return RedirectToAction("PageAddressManagement");
                }

                TempData["FailMessage"] = "Không thể thêm trang!";
            }
            else
            {
                TempData["FailMessage"] = "Dữ liệu không hợp lệ!";
            }



            return View(pageAddressVM);
        }

        public IActionResult EditPageAddress(int pageAddressId)
        {
            var userType = accessManagementService.GetPageAddressById(pageAddressId);

            if (userType == null)
            {
                return NotFound();
            }

            return View(userType);
        }

        [HttpPost]
        public IActionResult UpdatePageAddress(PageAddressVM pageAddressVM)
        {
            if (ModelState.IsValid)
            {
                var result = accessManagementService.UpdatePageAddress(pageAddressVM);

                if (result)
                {
                    TempData["SuccessMessage"] = "Cập nhật trang thành công!";
                    return RedirectToAction("EditPageAddress", new { pageAddressId = pageAddressVM.PageAddressId });
                }
                else
                {
                    TempData["FailMessage"] = "Trang không tồn tại!";
                }
            }

            TempData["FailMessage"] = "Dữ liệu không hợp lệ!";
            return RedirectToAction("EditPageAddress", new { pageAddressId = pageAddressVM.PageAddressId });
        }

        public IActionResult DeletePageAddress(int pageAddressId)
        {
            if (!accessManagementService.CanDeletePageAddress(pageAddressId))
            {
                TempData["FailMessage"] = "Không thể xóa trang vì có nhóm người dùng đang dùng trang này!";
                return RedirectToAction("PageAddressManagement");
            }

            if (accessManagementService.DeletePageAdress(pageAddressId))
            {
                TempData["SuccessMessage"] = "Xóa trang thành công!";
            }
            else
            {
                TempData["FailMessage"] = "Trang không tồn tại!";
            }

            return RedirectToAction("PageAddressManagement");
        }

        public IActionResult PrivilegeManagement(int pageNumber = 1, int pageSize = 7, string searchQuery = "")
        {
            int totalItems;
            List<UserTypeVM> userType;

            if (string.IsNullOrEmpty(searchQuery))
            {
                totalItems = accessManagementService.GetUserTypeCount();
                userType = accessManagementService.GetUserType(pageNumber, pageSize);
            }
            else
            {
                var searchResults = accessManagementService.SearchUserTypes(searchQuery);
                totalItems = searchResults.Count;
                userType = searchResults
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            var paginationModel = new PagedListVM<UserTypeVM>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = userType
            };

            ViewBag.SearchQuery = searchQuery;
            return View("PrivilegeManagement", paginationModel);
        }

        public IActionResult UpdatePrivilege(int userTypeId, int pageNumber = 1, int pageSize = 7, string searchQuery = "")
        {
            int totalItems;
            List<PageAddressVM> pageAddresses;

            if (string.IsNullOrEmpty(searchQuery))
            {
                totalItems = accessManagementService.GetPageAddressCount();
                pageAddresses = accessManagementService.GetPageAddresses(pageNumber, pageSize);
            }
            else
            {
                var searchResults = accessManagementService.SearchPageAddress(searchQuery);
                totalItems = searchResults.Count;
                pageAddresses = searchResults
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            // Lấy tất cả quyền của userTypeId một lần
            var userPrivileges = accessManagementService.GetPrivilegesByUserType(userTypeId);

            // Tạo danh sách quyền cho từng trang
            var pagePrivileges = pageAddresses.Select(page => new PrivilegeVM
            {
                PageAddressId = page.PageAddressId,
                PageName = page.PageName,
                IsPrivileged = userPrivileges
                    .Any(priv => priv.PageAddressId == page.PageAddressId && priv.IsPrivileged)
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Tạo đối tượng phân trang
            var paginationModel = new PagedListVM<PrivilegeVM>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = pagePrivileges
            };

            // Truyền `userTypeId` và `searchQuery` vào ViewBag để dùng trong Razor
            ViewBag.UserTypeId = userTypeId;
            ViewBag.SearchQuery = searchQuery;

            return View(paginationModel);
        }

        public IActionResult GrantPermission(int userTypeId, int pageAddressId)
        {
            var userPrivilege = new PrivilegeVM
            {
                UserTypeId = userTypeId,
                PageAddressId = pageAddressId,
                IsPrivileged = true
            };

            if(accessManagementService.GrantPermission(userPrivilege))
            {
                TempData["SuccessMessage"] = "Cấp quyền thành công!";
            }    
            else
            {
                TempData["FailMessage"] = "Cấp quyền thất bại!";
            }    

            return RedirectToAction("UpdatePrivilege", new { userTypeId = userTypeId });
        }

        public IActionResult RevokePermission(int userTypeId, int pageAddressId)
        {
            var privilege = accessManagementService.GetPrivilegeByUserTypeAndPageAddress(userTypeId, pageAddressId);

            if (privilege != null)
            {
                privilege.IsPrivileged = false;

                var result = accessManagementService.UpdatePrivilege(privilege);

                if (result)
                {
                    TempData["SuccessMessage"] = "Thu hồi quyền thành công!";
                }
                else
                {
                    TempData["FailMessage"] = "Thu hồi quyền thất bại!";
                }
            }
            else
            {
                TempData["FailMessage"] = "Không tìm thấy quyền để thu hồi!";
            }

            return RedirectToAction("UpdatePrivilege", new { userTypeId = userTypeId });
        }


    }
}
