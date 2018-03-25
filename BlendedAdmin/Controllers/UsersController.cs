using System;
using Microsoft.AspNetCore.Mvc;
using BlendedAdmin.DomainModel;
using BlendedAdmin.Models.Environments;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Environment = BlendedAdmin.DomainModel.Environments.Environment;
using BlendedAdmin.DomainModel.Users;
using BlendedAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using BlendedAdmin.Services;

namespace BlendedAdmin.Controllers
{
    public class UsersController : Controller
    {
        private IDomainContext _domainContext;
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private IEmailService _emailService;
        private ILogger _logger;

        public UsersController(IDomainContext domainContext, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            ILoggerFactory loggerFactory)
        {
            _domainContext = domainContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _logger = loggerFactory.CreateLogger<UsersController>();
        }

        [HttpGet]
        [Route("{environment}/users")]
        public async Task<IActionResult> Index()
        {
            List<ApplicationUser> users = await _domainContext.Users.GetAll();
            var model = new UserModelAssembler().ToModel(users);
            return View(model);
        }

        [HttpGet]
        [Route("{environment}/users/create")]
        public IActionResult Create()
        {
            return View(new CreateModel());
        }

        [HttpPost]
        [Route("{environment}/users/create")]
        public async Task<IActionResult> Create(CreateModel model)
        {
            if (ModelState.IsValid == false)
                return View("Edit", model);

            ApplicationUser entity = new ApplicationUser();
            new UserModelAssembler().Apply(entity, model);
            var result = await this._userManager.CreateAsync(entity, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("Edit", "Users", new { id = entity.Id });
            }
            else
            {
                result.Errors.ToList().ForEach(x => ModelState.AddModelError(string.Empty, x.Description));
                return View(model);
            }
        }

        [HttpGet]
        [Route("{environment}/users/{id}/edit")]
        public async Task<IActionResult> Edit(string id)
        {
            var entity = await this._domainContext.Users.Get(id);
            if (entity == null)
            {
                this.ModelState.AddModelError("", "Sorry, we cannot find the user.");
                return View(new ApplicationUser());
            }
            var model = new UserModelAssembler().ToModel(entity);
            return View(model);
        }

        [HttpPost]
        [Route("{environment}/users/{id}/edit")]
        public async Task<IActionResult> Edit(string id, EditModel model)
        {
            if (ModelState.IsValid == false)
                return View(model);

            var entity = await this._domainContext.Users.Get(id);
            new UserModelAssembler().Apply(entity, model);
            var result = await this._userManager.UpdateAsync(entity);
            if (result.Succeeded)
            {
                return RedirectToAction("Edit", "Users", new { id = entity.Id });
            }
            else
            {
                result.Errors.ToList().ForEach(x => ModelState.AddModelError(string.Empty, x.Description));
                return View(model);
            }
        }

        [HttpGet]
        [Route("{environment}/users/{id}/changepassword")]
        public async Task<IActionResult> ChangePassword(string id)
        {
            var entity = await this._domainContext.Users.Get(id);
            if (entity == null)
            {
                this.ModelState.AddModelError("", "Sorry, we cannot find the user.");
                return View(new ApplicationUser());
            }
            var model = new UserModelAssembler().ToChangePasswordModel(entity);
            return View(model);
        }

        [HttpPost]
        [Route("{environment}/users/{id}/changepassword")]
        public async Task<IActionResult> ChangePassword(string id, ChangePassowrdModel model)
        {
            if (ModelState.IsValid == false)
                return View(model);

            var entity = await this._domainContext.Users.Get(id);
            var results = await _userManager.RemovePasswordAsync(entity);
            results.Errors.ToList().ForEach(x => ModelState.AddModelError(string.Empty, x.Description));
            results = await _userManager.AddPasswordAsync(entity, model.Password);
            model.Succeeded = results.Succeeded;
            results.Errors.ToList().ForEach(x => ModelState.AddModelError(string.Empty, x.Description));
            return View(model);
        }

        [Route("{environment}/users/{id}/delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var entity = await this._domainContext.Users.Get(id);
            await _userManager.DeleteAsync(entity);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("{environment}/login")]
        public async Task<IActionResult> LogIn(string returnUrl = null)
        {
            //await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            await HttpContext.SignOutAsync();
            //ViewData["ReturnUrl"] = returnUrl;
            return View(new LogInModel());
        }

        [HttpPost]
        [Route("{environment}/login")]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> LogIn(LogInModel model, string returnUrl = null)
        {
            //ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Name, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation(1, "User logged in.");
                    return RedirectToLocal(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning(2, "User account locked out.");
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }

        [Route("{environment}/logoff")]
        public async Task<IActionResult> LogOff(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            //ViewData["ReturnUrl"] = returnUrl;
            return View(new LogInModel());
        }

        [HttpGet]
        [Route("{environment}/accessdenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
        
        [HttpGet]
        [AllowAnonymous]
        [Route("{environment}/forgotpassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        [Route("{environment}/forgotpassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)// || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return View("ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Users", new { userId = user.Id, Code = code }, protocol: HttpContext.Request.Scheme);
                //ViewData["callbackUrl"] = callbackUrl;
                await _emailService.SendEmailAsync(model.Email, "Reset Password",
                   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                return View("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("{environment}/forgotpasswordconfirmation")]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("{environment}/resetpassword")]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        [Route("{environment}/resetpassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Users");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Users");
            }
            result.Errors.ToList().ForEach(x => ModelState.AddModelError("", x.Description));
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("{environment}/resetpasswordconfirmation")]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}