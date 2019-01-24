using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Invento.Models;
using Invento.Models.AccountViewModels;
using Invento.Services;
using Invento.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Invento.Areas.Finance.Models;

namespace Invento.Controllers
{
    [Authorize]
    [RequireHttps]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory,
            ApplicationDbContext context)
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _emailSender = emailSender;
                _smsSender = smsSender;
                _context = context;
                _logger = loggerFactory.CreateLogger<AccountController>();
            }
  
        #region RegisterFunctions
       
        [HttpGet]        
        [Authorize(Roles = "SiteAdmin")]
        public IActionResult RegisterCompanyAdmin(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]       
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SiteAdmin")]
        public async Task<IActionResult> RegisterCompanyAdmin(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {               
                    var user = new ApplicationUser { UserName = model.Email, Email = model.Email, CompanyID = model.CompanyID, AccountActive = false };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        string CurrentRole = "CompanyAdmin";
                        await this._userManager.AddToRoleAsync(user, CurrentRole);
                        string CurrentRole1 = "BiznsBook";
                        await this._userManager.AddToRoleAsync(user, CurrentRole1);

                    await _userManager.AddClaimAsync(user, new Claim("CompanyID", user.CompanyID.ToString()));

                    // Insert statements on New user creation *** STATEMENT OF ACCOUNTS ***

                            DateTime CurrentDate = DateTime.Now;
                            string CurrentUser = User.Identity.Name;
                    
                            MainAccount mainParties = new MainAccount();
                            mainParties.AccountName = "Parties";
                            mainParties.MainAccountNumber = "00";
                            mainParties.CreationDate = CurrentDate;
                            mainParties.CreatedBy = CurrentUser;
                            mainParties.CompanyID = model.CompanyID;
                            _context.Add(mainParties);

                            MainAccount mainAssets = new MainAccount();
                            mainAssets.AccountName = "Assets";
                            mainAssets.MainAccountNumber = "01";
                            mainAssets.CreationDate = CurrentDate;
                            mainAssets.CreatedBy = CurrentUser;
                            mainAssets.CompanyID = model.CompanyID;
                            _context.Add(mainAssets);

                            MainAccount mainLiabilities = new MainAccount();
                            mainLiabilities.AccountName = "Liabilities";
                            mainLiabilities.MainAccountNumber = "02";
                            mainLiabilities.CreationDate = CurrentDate;
                            mainLiabilities.CreatedBy = CurrentUser;
                            mainLiabilities.CompanyID = model.CompanyID;
                            _context.Add(mainLiabilities);

                            MainAccount mainIncome = new MainAccount();
                            mainIncome.AccountName = "Income";
                            mainIncome.MainAccountNumber = "03";
                            mainIncome.CreationDate = CurrentDate;
                            mainIncome.CreatedBy = CurrentUser;
                            mainIncome.CompanyID = model.CompanyID;
                            _context.Add(mainIncome);

                            MainAccount mainExpenses = new MainAccount();
                            mainExpenses.AccountName = "Expenses";
                            mainExpenses.MainAccountNumber = "04";
                            mainExpenses.CreationDate = CurrentDate;
                            mainExpenses.CreatedBy = CurrentUser;
                            mainExpenses.CompanyID = model.CompanyID;
                            _context.Add(mainExpenses);
                    
                            await _context.SaveChangesAsync();

                            int mainPartiesID = mainParties.MainAccountID;
                            int mainAssetsID = mainAssets.MainAccountID;
                            int mainLiabilitiesID = mainLiabilities.MainAccountID;
                            int mainIncomeID = mainIncome.MainAccountID;
                            int mainExpensesID = mainExpenses.MainAccountID;

                            SubAccount subParties = new SubAccount();
                            subParties.AccountName = "Parties";
                            subParties.SubAccountNumber = "0000";
                            subParties.CreationDate = CurrentDate;
                            subParties.CreatedBy = CurrentUser;
                            subParties.CompanyID = model.CompanyID;
                            subParties.MainAccountID = mainPartiesID;
                            _context.Add(subParties);

                            SubAccount subPurchase = new SubAccount();
                            subPurchase.AccountName = "Purchase";
                            subPurchase.SubAccountNumber = "0001";
                            subPurchase.CreationDate = CurrentDate;
                            subPurchase.CreatedBy = CurrentUser;
                            subPurchase.CompanyID = model.CompanyID;
                            subPurchase.MainAccountID = mainExpensesID;
                            _context.Add(subPurchase);

                            SubAccount subSaleReturn = new SubAccount();
                            subSaleReturn.AccountName = "Sale Return";
                            subSaleReturn.SubAccountNumber = "0002";
                            subSaleReturn.CreationDate = CurrentDate;
                            subSaleReturn.CreatedBy = CurrentUser;
                            subSaleReturn.CompanyID = model.CompanyID;
                            subSaleReturn.MainAccountID = mainExpensesID;
                            _context.Add(subSaleReturn);

                            SubAccount subCash = new SubAccount();
                            subCash.AccountName = "Cash";
                            subCash.SubAccountNumber = "0001";
                            subCash.CreationDate = CurrentDate;
                            subCash.CreatedBy = CurrentUser;
                            subCash.CompanyID = model.CompanyID;
                            subCash.MainAccountID = mainAssetsID;
                            _context.Add(subCash);

                            SubAccount subBank = new SubAccount();
                            subBank.AccountName = "Bank";
                            subBank.SubAccountNumber = "0002";
                            subBank.CreationDate = CurrentDate;
                            subBank.CreatedBy = CurrentUser;
                            subBank.CompanyID = model.CompanyID;
                            subBank.MainAccountID = mainAssetsID;
                            _context.Add(subBank);

                            SubAccount subSale = new SubAccount();
                            subSale.AccountName = "Sale";
                            subSale.SubAccountNumber = "0001";
                            subSale.CreationDate = CurrentDate;
                            subSale.CreatedBy = CurrentUser;
                            subSale.CompanyID = model.CompanyID;
                            subSale.MainAccountID = mainIncomeID;
                            _context.Add(subSale);

                            SubAccount subPurchaseReturn = new SubAccount();
                            subPurchaseReturn.AccountName = "Purchase Return";
                            subPurchaseReturn.SubAccountNumber = "0002";
                            subPurchaseReturn.CreationDate = CurrentDate;
                            subPurchaseReturn.CreatedBy = CurrentUser;
                            subPurchaseReturn.CompanyID = model.CompanyID;
                            subPurchaseReturn.MainAccountID = mainIncomeID;
                            _context.Add(subPurchaseReturn);

                            await _context.SaveChangesAsync();

                            int subPurchaseID = subPurchase.SubAccountID;
                            int subSaleReturnID = subSaleReturn.SubAccountID;
                            int subCashID = subCash.SubAccountID;
                            int subSaleID = subSale.SubAccountID;
                            int subPurchaseReturnID = subPurchaseReturn.SubAccountID;

                            TransactionAccount transPurchaseCash = new TransactionAccount();
                            transPurchaseCash.AccountName = "Cash Purchase";
                            transPurchaseCash.TransactionAccountNumber = "0001";
                            transPurchaseCash.OpeningBalance = 0;
                            transPurchaseCash.CreationDate = CurrentDate;
                            transPurchaseCash.CreatedBy = CurrentUser;
                            transPurchaseCash.CompanyID = model.CompanyID;
                            transPurchaseCash.SubAccountID = subPurchaseID;
                            _context.Add(transPurchaseCash);

                            TransactionAccount transPurchaseCredit = new TransactionAccount();
                            transPurchaseCredit.AccountName = "Credit Purchase";
                            transPurchaseCredit.TransactionAccountNumber = "0002";
                            transPurchaseCredit.OpeningBalance = 0;
                            transPurchaseCredit.CreationDate = CurrentDate;
                            transPurchaseCredit.CreatedBy = CurrentUser;
                            transPurchaseCredit.CompanyID = model.CompanyID;
                            transPurchaseCredit.SubAccountID = subPurchaseID;
                            _context.Add(transPurchaseCredit);

                            TransactionAccount transSaleReturn = new TransactionAccount();
                            transSaleReturn.AccountName = "Sale Return";
                            transSaleReturn.TransactionAccountNumber = "0001";
                            transSaleReturn.OpeningBalance = 0;
                            transSaleReturn.CreationDate = CurrentDate;
                            transSaleReturn.CreatedBy = CurrentUser;
                            transSaleReturn.CompanyID = model.CompanyID;
                            transSaleReturn.SubAccountID = subSaleReturnID;
                            _context.Add(transSaleReturn);

                            TransactionAccount transCashInHand = new TransactionAccount();
                            transCashInHand.AccountName = "Cash In Hand";
                            transCashInHand.TransactionAccountNumber = "0001";
                            transCashInHand.OpeningBalance = 0;
                            transCashInHand.CreationDate = CurrentDate;
                            transCashInHand.CreatedBy = CurrentUser;
                            transCashInHand.CompanyID = model.CompanyID;
                            transCashInHand.SubAccountID = subCashID;
                            _context.Add(transCashInHand);

                            TransactionAccount transCashSale = new TransactionAccount();
                            transCashSale.AccountName = "Cash Sale";
                            transCashSale.TransactionAccountNumber = "0001";
                            transCashSale.OpeningBalance = 0;
                            transCashSale.CreationDate = CurrentDate;
                            transCashSale.CreatedBy = CurrentUser;
                            transCashSale.CompanyID = model.CompanyID;
                            transCashSale.SubAccountID = subSaleID;
                            _context.Add(transCashSale);

                            TransactionAccount transCreditSale = new TransactionAccount();
                            transCreditSale.AccountName = "Credit Sale";
                            transCreditSale.TransactionAccountNumber = "0002";
                            transCreditSale.OpeningBalance = 0;
                            transCreditSale.CreationDate = CurrentDate;
                            transCreditSale.CreatedBy = CurrentUser;
                            transCreditSale.CompanyID = model.CompanyID;
                            transCreditSale.SubAccountID = subSaleID;
                            _context.Add(transCreditSale);
                    
                            TransactionAccount transPurchaseReturn = new TransactionAccount();
                            transPurchaseReturn.AccountName = "Purchase Return";
                            transPurchaseReturn.TransactionAccountNumber = "0001";
                            transPurchaseReturn.OpeningBalance = 0;
                            transPurchaseReturn.CreationDate = CurrentDate;
                            transPurchaseReturn.CreatedBy = CurrentUser;
                            transPurchaseReturn.CompanyID = model.CompanyID;
                            transPurchaseReturn.SubAccountID = subPurchaseReturnID;
                            _context.Add(transPurchaseReturn);

                            await _context.SaveChangesAsync();
                    // Insert statements on New user creation *** STATEMENT OF ACCOUNTS ***
                    
                    // Send an email with this link
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                        $"<h2>BiznsBook.com</h2>Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
                    _logger.LogInformation(3, "User created a new account with password.");

                    return RedirectToLocal(returnUrl);
                    //return RedirectToAction("SiteValidations", "Home");
                }
                AddErrors(result);
                 
            }
            return View(model);
        }
 
        [HttpGet]
        [Authorize(Roles = "BiznsBook")]
        [Authorize(Roles = "CompanyAdmin")]        
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            List<IdentityRole> RoleList;

            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
           
            RoleList = _context.Roles.Where(r=>r.Name != "SiteAdmin").Where(r => r.Name != "BiznsBook").Where(r=>r.Name != "CompanyAdmin").OrderByDescending(r => r.Name).ToList();
            RegisterVMCompanyUser model = new RegisterVMCompanyUser();
            model.RoleList = RoleList;
            if(_context.CompanyProfile.Where(r => r.CompanyID == CompID).Any() == true)
            {
                model.NoUserAllowed = _context.CompanyProfile.Where(r => r.CompanyID == CompID).FirstOrDefault().NoOfCompanyUsersAllowed;
            }            
            return View(model);          
        }

        [HttpPost]        
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BiznsBook")]
        [Authorize(Roles = "CompanyAdmin")]        
        public async Task<IActionResult> Register(RegisterVMCompanyUser model, string returnUrl = null)
        {
            string CompID_String = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompID_String);
            model.CompanyID = CompID;

            ViewData["ReturnUrl"] = returnUrl;
            int NoUserAllowed = 0;
            if (_context.CompanyProfile.Where(r => r.CompanyID == CompID).Any() == true)
            {
                NoUserAllowed = _context.CompanyProfile.Where(r => r.CompanyID == CompID).FirstOrDefault().NoOfCompanyUsersAllowed;
                model.NoUserAllowed = NoUserAllowed;
            }
            int UserCount = 0;
            
            UserCount = _context.Users.Where(r=>r.CompanyID==CompID).Count();
            
            NoUserAllowed = NoUserAllowed + 3;           
            if (NoUserAllowed > UserCount)
            {
                if (ModelState.IsValid)
                {
                    var user = new ApplicationUser { UserName = model.Email, Email = model.Email, CompanyID = model.CompanyID };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        for (int i = 0; i < model.SelectedRoles.Count(); i++)
                        {
                            string AA = model.SelectedRoles[i].ToString();
                            await _userManager.AddToRoleAsync(user, AA);                            
                        }                    
                        await _userManager.AddToRoleAsync(user, "BiznsBook");
                        await _userManager.AddClaimAsync(user, new Claim("CompanyID", CompID_String));

                        // Send an email with this link
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                        await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                            $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
                        _logger.LogInformation(3, "User created a new account with password.");
                        return RedirectToAction("Index", "Home", new { area = "CompanyAdmin" });
                    }                    
                }
            }
            List<IdentityRole> RoleList;
            RoleList = _context.Roles.Where(r => r.Name != "SiteAdmin").Where(r => r.Name != "BiznsBook").Where(r => r.Name != "CompanyAdmin").OrderByDescending(r => r.Name).ToList();
            model.RoleList = RoleList;
         
            return View(model);            
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterSiteAdmin(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterSiteAdmin(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                if (model.Email == "fahadhameed3h@hotmail.com")
                {
                    // Inserting SiteAdmin Role to table for First Time if not present
                    if (_context.Roles.Where(r => r.Name == "SiteAdmin").Any() == false)
                    {
                        IdentityRole Role = new IdentityRole();
                        string RoleName = "SiteAdmin";
                        Role.NormalizedName = RoleName.ToUpper();
                        Role.Name = RoleName;
                        _context.Add(Role);
                        await _context.SaveChangesAsync();
                    }

                    var user = new ApplicationUser { UserName = model.Email, Email = model.Email, CompanyID = 0, AccountActive = false };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        string CurrentRole = "SiteAdmin";
                        await this._userManager.AddToRoleAsync(user, CurrentRole);
                        await _userManager.AddClaimAsync(user, new Claim("CompanyID", "0"));

                        // Send an email with this link
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                        await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                            $"<b> BiznsBook Account Confirmation </b> <h2>Please confirm your account by clicking this link:</h2> <a href='{callbackUrl}'>link</a>");

                        return RedirectToLocal(returnUrl);
                    }
                    AddErrors(result);
                }
            }
            return View(model);
        }

        #endregion

        #region EditUserRoles

        [HttpGet]
        [Authorize(Roles = "BiznsBook")]
        [Authorize(Roles = "CompanyAdmin")]
        public IActionResult EditRoles(string id)
        {
            List<IdentityRole> RoleList;

            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            RoleList = _context.Roles.Where(r => r.Name != "SiteAdmin").Where(r => r.Name != "BiznsBook").Where(r => r.Name != "CompanyAdmin").OrderByDescending(r => r.Name).ToList();
            UserEdit model = new UserEdit();
            model.RoleList = RoleList;
            model.Email = _context.Users.Where(r => r.Id == id).First().Email;
            model.UserID = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "BiznsBook")]
        [Authorize(Roles = "CompanyAdmin")]
        public async Task<IActionResult> EditRoles(UserEdit model)
        {
            string CompID_String = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompID_String);
            
                if (ModelState.IsValid)
                {
                    _context.UserRoles.RemoveRange(_context.UserRoles.Where(x => x.UserId == model.UserID));
                    _context.SaveChanges();
                    var user = _context.Users.Where(r => r.CompanyID == CompID).Where(r => r.Id == model.UserID).First();

                    for (int i = 0; i < model.SelectedRoles.Count(); i++)
                    {
                        string AA = model.SelectedRoles[i].ToString();
                        await _userManager.AddToRoleAsync(user, AA);
                    }
                    await _userManager.AddToRoleAsync(user, "BiznsBook");
                    return RedirectToAction("Index", "Home", new { area = "CompanyAdmin" });
                }
            List<IdentityRole> RoleList;
            RoleList = _context.Roles.Where(r => r.Name != "SiteAdmin").Where(r => r.Name != "BiznsBook").Where(r => r.Name != "CompanyAdmin").OrderByDescending(r => r.Name).ToList();
            model.RoleList = RoleList;

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "SiteAdmin")]
        public IActionResult EditRolesCompanyAdmin(string id)
        {
            List<IdentityRole> RoleList;

            RoleList = _context.Roles.Where(r => (r.Name == "CompanyAdmin") || (r.Name == "BiznsBook")).ToList();
            UserEdit model = new UserEdit();
            model.RoleList = RoleList;
            model.Email = _context.Users.Where(r => r.Id == id).First().Email;
            model.UserID = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SiteAdmin")]
        public async Task<IActionResult> EditRolesCompanyAdmin(UserEdit model)
        { 
            if (ModelState.IsValid)
            {
                _context.UserRoles.RemoveRange(_context.UserRoles.Where(x => x.UserId == model.UserID));
                _context.SaveChanges();
                var user = _context.Users.Where(r => r.Id == model.UserID).First();

                for (int i = 0; i < model.SelectedRoles.Count(); i++)
                {
                    string AA = model.SelectedRoles[i].ToString();
                    await _userManager.AddToRoleAsync(user, AA);
                }
                return RedirectToAction("Index", "Home", new { area = "SiteAdmin" });
            }
            List<IdentityRole> RoleList;
            RoleList = _context.Roles.Where(r => r.Name == "CompanyAdmin").Where(r => r.Name == "BiznsBook").ToList();
            model.RoleList = RoleList;

            return View(model);
        }
        
        #endregion

        #region Login_LogOff_ConfirmPassword_Reset
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Require the user to have a confirmed email before they can log on.
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError(string.Empty, "You must have a confirmed email to log in.");
                        return View(model);
                    }
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation(1, "User logged in.");
                    return RedirectToAction("SiteValidations", "Home");
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
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

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                // Send an email with this link
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            AddErrors(result);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        #endregion

        #region Other_UnUsedFunctions

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            // Generate the token and send it
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
            if (string.IsNullOrWhiteSpace(code))
            {
                return View("Error");
            }

            var message = "Your security code is: " + code;
            if (model.SelectedProvider == "Email")
            {
                await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
            }
            else if (model.SelectedProvider == "Phone")
            {
                await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
            }

            return RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning(7, "User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid code.");
                return View(model);
            }
        }
 
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                _logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation(6, "User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        #endregion
         
        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}
