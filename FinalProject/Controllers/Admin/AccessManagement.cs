﻿using FinalProject.Data;
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

    }
}
