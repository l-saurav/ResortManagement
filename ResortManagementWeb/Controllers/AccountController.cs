using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResortManagement.Application.Common.Interfaces;
using ResortManagement.Application.Common.Utility;
using ResortManagement.Domain.Entities;
using ResortManagement.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace ResortManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Login(string returnURL = null)
        {
            //Check if returnURL is null; if not empty it will populate that with the content 
            returnURL??= Url.Content("~/");
            LoginViewModel loginViewModel = new()
            {
                RedirectURL = returnURL
            };
            return View(loginViewModel);
        }
        public IActionResult Register(string returnURL = null)
        {
            //Check if returnURL is null; if not empty it will populate that with the content 
            returnURL ??= Url.Content("~/");
            RegisterViewModel registerViewModel = new()
            {
                RoleList = _roleManager.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                }),
                RedirectURL = returnURL
            };
            return View(registerViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new()
                {
                    Name = registerViewModel.Name,
                    Email = registerViewModel.Email,
                    PhoneNumber = registerViewModel.PhoneNumber,
                    NormalizedEmail = registerViewModel.Email.ToUpper(),
                    EmailConfirmed = true,
                    UserName = registerViewModel.Email,
                    CreatedAt = DateTime.Now
                };

                //Create a new user using helper method not EF Core
                var result = _userManager.CreateAsync(user, registerViewModel.Password).GetAwaiter().GetResult();

                if (result.Succeeded)
                {
                    //Assigning a role using Helper method 
                    if (!string.IsNullOrEmpty(registerViewModel.Role))
                    {
                        await _userManager.AddToRoleAsync(user, registerViewModel.Role);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, SD.Role_Customer);
                    }
                    //Sign in a User
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["success"] = "Congrats! You have been successfully registered!";
                    if (string.IsNullOrEmpty(registerViewModel.RedirectURL))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return LocalRedirect(registerViewModel.RedirectURL);
                    }
                }
                else
                {
                    TempData["error"] = "Sorry! Registration was unsuccessful!";
                }
                //Show error on the Model Summary
                foreach (var error in result.Errors)
                {

                    ModelState.AddModelError("", error.Description);
                }
            }
            //Populate the select item of the roles
            registerViewModel.RoleList = _roleManager.Roles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name
            });
            return View(registerViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if(ModelState.IsValid)
            {
                //Use Helper Method to Sign in 
                var result = await _signInManager.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, loginViewModel.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    TempData["success"] = "Congrats! You have been successfully logged in";
                    var user = await _userManager.FindByEmailAsync(loginViewModel.Email);
                    if(await _userManager.IsInRoleAsync(user,SD.Role_Admin))
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(loginViewModel.RedirectURL))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            return LocalRedirect(loginViewModel.RedirectURL);
                        }
                    }
                }
                else
                {
                    TempData["error"] = "Sorry! Email and Password do not match up";
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid Login Attempt");
            }
            return View(loginViewModel);
        }

        public async Task<IActionResult> Logout()
        {
            //Use Helper method to logout
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
