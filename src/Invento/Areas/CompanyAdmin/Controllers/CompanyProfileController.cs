using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Invento.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Invento.Areas.CompanyAdmin.Models.Company;

namespace Invento.Controllers
{
    [Authorize(Roles = "SiteAdmin,BiznsBook")]
    [Authorize(Roles = "CompanyAdmin,SiteAdmin,BiznsBook")]
    [Area("CompanyAdmin")]
    [Route("Company/[controller]")]
    public class CompanyProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;
        
        public CompanyProfileController(ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        [Route("[action]")]
        public IActionResult Index()
        {
            HttpContext.Session.Clear();
            string UserID = User.Claims.First().Value;
            string CompID = User.Claims.Where(r => r.Type == "CompanyID").First().Value;
            int ID = Convert.ToInt32(CompID);
            byte[] Logo = _context.CompanyProfile.SingleOrDefault(m => m.CompanyID == ID).FileData;
            if(Logo != null)
            { 
                HttpContext.Session.Set("CompanyLogo", Logo);
            }
            HttpContext.Session.SetInt32("CompanyID", ID);

            return View();
        }

        #region CompanyProfile

        [Route("[action]")]
        public async Task<IActionResult> Profile()
        {           
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;                        
            int CompID = Convert.ToInt32(CompId);           

            if (_context.CompanyProfile.Where(r => r.CompanyID == CompID).Any() == false)
            {
                ViewData["CountryID"] = new SelectList(_context.Country.Where(r=>r.CompanyID == CompID), "CountryID", "Name");
                return View("CreateProfile");
            }
            else
            {
                int CurrentCompProfileId = _context.CompanyProfile.Where(r => r.CompanyID == CompID).FirstOrDefault().CompanyProfileID;               
                if (CurrentCompProfileId == 0)
                {
                    return NotFound();
                }
                var companyProfile = await _context.CompanyProfile.SingleOrDefaultAsync(m => m.CompanyProfileID == CurrentCompProfileId);
                if (companyProfile == null)
                {
                    return NotFound();
                }
                ViewData["CountryID"] = new SelectList(_context.Country.Where(r => r.CompanyID == CompID), "CountryID", "Name", companyProfile.CountryID);
                return View("EditProfile",companyProfile);                
            }
        }    
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[action]")]
        public async Task<IActionResult> CreateProfile(CompanyProfile companyProfile , IFormFile File)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            companyProfile.CompanyID = CompID;
            companyProfile.CompanyProfileID = 0;
            if (ModelState.IsValid)
            {
                byte[] data;
                if (File != null)
                {
                    using (var stream = File.OpenReadStream())
                    {
                        data = new byte[stream.Length];
                        stream.Read(data, 0, (int)stream.Length);
                    }
                    companyProfile.FileData = data;
                    companyProfile.FileName = File.FileName;
                }
                companyProfile.CompanyProfileID = 0;
                _context.CompanyProfile.Add(companyProfile);
                await _context.SaveChangesAsync();
                if(User.IsInRole("SiteAdmin"))
                {
                    return RedirectToAction("Index", "Home", new { area = "SiteAdmin" });
                }
                else
                {
                    return RedirectToAction("Index", "Home", new { area = "CompanyAdmin" });
                }                
            }
            ViewData["CountryID"] = new SelectList(_context.Country.Where(r => r.CompanyID == CompID), "CountryID", "Name", companyProfile.CountryID);
            return View(companyProfile);
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[action]")]
        public async Task<IActionResult> EditProfile([Bind("CompanyProfileID,Address,AlternativeContact,City,CompanyName,CompanyWebsite,ContactNumber,CountryID,Fax,OfficeContact,FileData,FileName")] CompanyProfile companyProfile, IFormFile File)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            companyProfile.CompanyID = CompID;

            if (ModelState.IsValid)
            {
                try
                {
                    byte[] data;                               
                    if (File != null)
                    {
                        using (var stream = File.OpenReadStream())
                        {
                            data = new byte[stream.Length];
                            stream.Read(data, 0, (int)stream.Length);
                        }
                        companyProfile.FileData = data;
                        companyProfile.FileName = File.FileName;
                    }
                    else if (File == null)
                    {                        
                        companyProfile.FileData = companyProfile.FileData;
                        companyProfile.FileName = companyProfile.FileName;
                    }
                    _context.Update(companyProfile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyProfileExists(companyProfile.CompanyProfileID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Home", new { area = "CompanyAdmin" });
            }
            ViewData["CountryID"] = new SelectList(_context.Country.Where(r => r.CompanyID == CompID), "CountryID", "Name", companyProfile.CountryID);
            return View(companyProfile);
        }
        
        private bool CompanyProfileExists(int id)
        {
            return _context.CompanyProfile.Any(e => e.CompanyProfileID == id);
        }

        #endregion
            
    }
}