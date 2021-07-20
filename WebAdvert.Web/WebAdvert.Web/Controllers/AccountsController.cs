using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Accounts;
using Amazon.AspNetCore.Identity.Cognito;

namespace WebAdvert.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;
        public AccountsController(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager, CognitoUserPool pool)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _pool = pool;
        }
        public async Task<IActionResult> SignUp()
        {
            var signupModel = new SignupModel();
            return View(signupModel);
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignupModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _pool.GetUser(model.Email);
                if (user.Status != null)
                {
                    ModelState.AddModelError("UserExists", "User with this email already exists");
                    return View(model);
                }


                user.Attributes.Add(CognitoAttribute.Email.AttributeName, model.Email);
                var createdUser = await _userManager.CreateAsync(user, model.Password);

                if (createdUser.Succeeded)
                {
                    RedirectToAction("Confirm","Accounts");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Confirm()
        {
            var confirmModel = new ConfirmModel();
            return View(confirmModel);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("NotFound", "User is not found with given email.");
                    return View(model);
                }

                try
                {
                    //var result = await _userManager.ConfirmEmailAsync(user, model.Code);
                    var result = await (_userManager as CognitoUserManager<CognitoUser>).ConfirmSignUpAsync(user, model.Code, true).ConfigureAwait(false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        foreach (var item in result.Errors)
                        {
                            ModelState.AddModelError(item.Code, item.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("InternalServerError", ex.Message);
                }
            }

            return View(model);

        }
       
        [HttpGet]
        public async Task<IActionResult> Login(LoginModel model)
        {
            return View(model);
        }

        [HttpPost]
        [ActionName("Login")]
        public async Task<IActionResult> LoginPost(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("LoginError", "Email and password do not match");
                }
            }

            return View("Login",model);
        }
        


        [HttpGet]
        public async Task<IActionResult> ResetPassword(ConfirmModel model)
        {
            return View(model);
        }

        [HttpPost]
        [ActionName("ResetPassword")]
        public async Task<IActionResult> ResetPasswordPost(ConfirmModel model)
        {
            CognitoUser user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("NotFound", "User is not found with given email.");
                return View(model);
            }

            if (model.codeRevieved==false)
            {              

                try
                {
                   // var result = await _userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
                    var result = await (_userManager as CognitoUserManager<CognitoUser>).SendEmailConfirmationTokenAsync(user).ConfigureAwait(false);
                    if (result.Succeeded)
                    {
                        model.codeRevieved = true;
                    }
                    else
                    {
                        foreach (var item in result.Errors)
                        {
                            ModelState.AddModelError(item.Code, item.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("InternalServerError", ex.Message);
                }
            }
            else
            {
                var result = await (_userManager as CognitoUserManager<CognitoUser>).ResetPasswordAsync(user).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                }
            }

            return View("ResetPassword", model);
        }

    }
 }