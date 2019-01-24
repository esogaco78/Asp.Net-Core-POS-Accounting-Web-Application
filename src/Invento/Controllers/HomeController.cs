using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Invento.Data;
using Microsoft.AspNetCore.Http;
using Invento.Models;
using Microsoft.AspNetCore.Identity;
using Invento.Models.ManageViewModels;
using Invento.Services;
using Microsoft.AspNetCore.Localization;

namespace Invento.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public HomeController(ApplicationDbContext context,
            SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,IEmailSender emailSender)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
        }


        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }


        [Authorize]
        public IActionResult SiteValidations()
        {
            if(User.IsInRole("BiznsBook"))
            {
                if (User.IsInRole("CompanyAdmin"))
                {
                    string UserID = User.Claims.First().Value;
                    bool AccActive = _context.Users.Where(r => r.Id == UserID).FirstOrDefault().AccountActive;
                    if (AccActive == false)
                    {
                        string CompID = User.Claims.Where(r => r.Type == "CompanyID").First().Value;
                        int ID = Convert.ToInt32(CompID);

                        var CompData = _context.CompanyProfile.Where(r => r.CompanyID == ID);
                        if (CompData.Any() == true)
                        {
                            byte[] Logo = CompData.FirstOrDefault().FileData;
                            if (Logo != null)
                            {
                                HttpContext.Session.Set("CompanyLogo", Logo);
                            }
                        }
                        HttpContext.Session.SetInt32("CompanyID", ID);
                        return RedirectToAction("Index", "Home", new { area = "CompanyAdmin" });
                    }
                    else
                    {
                        _signInManager.SignOutAsync();
                        return RedirectToAction("AccountLocked", "Home");
                    }
                }             
                else
                {
                    string UserID = User.Claims.First().Value;
                    bool AccActive = _context.Users.Where(r => r.Id == UserID).FirstOrDefault().AccountActive;
                    if (AccActive == false)
                    {
                        string CompID = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;

                        int ID = Convert.ToInt32(CompID);
                        var CompData = _context.CompanyProfile.Where(r => r.CompanyID == ID);
                        if (CompData.Any() == true)
                        {
                            byte[] Logo = _context.CompanyProfile.SingleOrDefault(m => m.CompanyID == ID).FileData;
                            if (Logo != null)
                            {
                                HttpContext.Session.Set("CompanyLogo", Logo);
                            }
                        }
                        HttpContext.Session.SetInt32("CompanyID", ID);
                        return RedirectToAction("Index", "Home", new { area = "CompanyUser" });
                    }
                    else
                    {
                        _signInManager.SignOutAsync();
                        return RedirectToAction("AccountLocked", "Home");
                    }
                }
            }
            //else if (User.IsInRole("SiteAdmin"))
            else
            {
                return RedirectToAction("Index", "Home", new { area = "SiteAdmin" });
            }
        }
        public IActionResult Index()
        {
            if(User.Identity.IsAuthenticated == false)
            {
                return View();
            }
            else
            {
                return RedirectToAction("SiteValidations", "Home");
            }
        }

        public IActionResult AccountLocked()
        {
            return View();
        }


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [HttpGet]        
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactVM model)
        {
            if (ModelState.IsValid)
            {
                string email = "fahadhameed3h@hotmail.com";
                string subject = "BiznsBook Message: " + model.Email + " , " + model.Company + " , " + model.PhoneNumber;
                // Send an email with this Message                                
                await _emailSender.SendEmailAsync(email , subject,
                    $"<h4> Name Of Sender:</h4><h3> " +model.FullName+ " </h3><h4>Message: <h3><p>" + model.Message + " </p>");
                return RedirectToAction("ThankYou", "Home");
            }
            return View();
        }


        public IActionResult MyNotes()
        {
            return PartialView("~/Views/Shared/_MyNotes.cshtml");
        }

        [HttpGet]
        public IActionResult CustomerSupport()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerSupport(ContactVM model)
        {
            if(model.CustomerNo == "123456789")
            { 
                if (ModelState.IsValid)
                {
                    string email = "fahadhameed3h@hotmail.com";
                    string subject = "BiznsBook Customer Support Message: " + model.Email + " , " + model.Company + " , " + model.PhoneNumber;
                    // Send an email with this Message                                
                    await _emailSender.SendEmailAsync(email, subject,
                        $"<h4> Name Of Sender:</h4><h3> " + model.FullName + " </h3><h4>Message: <h3><p>" + model.Message + " </p>");
                    return RedirectToAction("ThankYou", "Home");
                }
            }                        
            return View();
        }

        public IActionResult Features()
        {
            return View();
        }
        public IActionResult Pricing()
        {
            return View();
        }
        public IActionResult ThankYou()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
