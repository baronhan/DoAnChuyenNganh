using AutoMapper;
using FinalProject.Data;
using FinalProject.Helpers;
using FinalProject.Services;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject.Controllers
{
    public class CustomerController : Controller
    {
        private readonly UserService _userService;
        private readonly IMapper _mapper;

        public CustomerController(IMapper mapper, UserService userService)
        {
            _mapper = mapper;
            _userService = userService;
        }

        #region Register

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var customer = _mapper.Map<User>(model);

                bool kq = await _userService.CreateUserAsync(customer, model);

                if (kq)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Error occurred while creating the user. Please try again.");
                }
            }
            return View(model);
        }

        #endregion

        #region SignIn
        [HttpGet]
        public IActionResult SignIn(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(LoginVM login, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (ModelState.IsValid)
            {
                var customer = await _userService.CheckUserAsync(login);

                if (customer == null)
                {
                    ModelState.AddModelError("Error", "User doesn't exist");
                }
                else
                {
                    if (!customer.IsValid)
                    {
                        ModelState.AddModelError("Error", "The account has been blocked");
                    }
                    else
                    {
                        if (customer.Password != login.password.ToMd5Hash(customer.RandomKey))
                        {
                            ModelState.AddModelError("Error", "The login information is wrong");
                        }
                        else
                        {

                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Email, customer.Email),
                                new Claim(ClaimTypes.Name, customer.Username),
                                new Claim(ClaimTypes.Role, "Khách Thuê"),
                                new Claim(ClaimTypes.NameIdentifier, customer.UserId.ToString())
                            };

                            var claimsIdentity = new ClaimsIdentity(claims, "login");
                            var claimPrincipal = new ClaimsPrincipal(claimsIdentity);

                            await HttpContext.SignInAsync(claimPrincipal);

                            var cookieOptions = new CookieOptions
                            {
                                Expires = DateTimeOffset.UtcNow.AddDays(30),
                                HttpOnly = true,
                                IsEssential = true
                            };

                            Response.Cookies.Append("user_id", customer.UserId.ToString(), cookieOptions);

                            if (Url.IsLocalUrl(returnUrl))
                            {
                                return Redirect(returnUrl);
                            }
                            else
                            {
                                return Redirect("/");
                            }
                        }
                    }
                }
            }

            return View();
        }
        #endregion


        #region Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            if (Request.Cookies.TryGetValue("user_id", out var userIdString) && int.TryParse(userIdString, out var userId))
            {
                var user = await _userService.getUserById(userId);
                var updatePersonalInfoVM = _mapper.Map<UpdatePersonalInformationVM>(user);
                return View(updatePersonalInfoVM);
            }
            return RedirectToAction("SignIn");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Profile(UpdatePersonalInformationVM user, IFormFile? userimage)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            if (Request.Cookies.TryGetValue("user_id", out var userIdString) && int.TryParse(userIdString, out var userId))
            {
                var _user = await _userService.getUserById(userId);
                if (_user == null)
                {
                    return NotFound();
                }

                if (userimage != null)
                {
                    var isUpdated = await _userService.UpdateUserImageAsync(_user, userimage);
                    if (isUpdated != null)
                    {
                        user.userimage = isUpdated;
                    } 
                }
                else
                {
                    user.userimage = _user.UserImage;
                }

                _mapper.Map(user, _user); 

                await _userService.UpdateUserInformationAsync(_user, userimage);

                return RedirectToAction("Profile"); 
            }

            return RedirectToAction("SignIn");
        }

        #endregion

        #region Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Delete("user_id");

            await HttpContext.SignOutAsync();
            return Redirect("/");
        }
        #endregion
    }
}
