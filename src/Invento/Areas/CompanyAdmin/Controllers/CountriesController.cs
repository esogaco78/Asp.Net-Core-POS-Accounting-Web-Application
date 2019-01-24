using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Invento.Data;
using Microsoft.AspNetCore.Authorization;
using Invento.Areas.CompanyAdmin.Models.Company;

namespace Invento.Areas.CompanyAdmin.Controllers
{   
    [Area("CompanyAdmin")]
    [Route("Company/[controller]")]
    [Authorize(Roles = "BiznsBook")]
    public class CountriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CountriesController(ApplicationDbContext context)
        {
            _context = context;    
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Countries")]
        public async Task<IActionResult> Index()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            return View(await _context.Country.Where(r=>r.CompanyID == CompID).ToListAsync());
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Add Countries")]
        public IActionResult Create()
        {
            return PartialView();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Add Countries")]
        public async Task<IActionResult> Create([Bind("CountryID,ISO,Iso3,Name,NiceName,NumCode,PhoneCode")] Country country)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            country.CompanyID = CompID;
            country.CreatedBy = User.Identity.Name;

            if (ModelState.IsValid)
            {
                _context.Add(country);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return PartialView(country);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Edit Countries")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var country = await _context.Country.SingleOrDefaultAsync(m => m.CountryID == id);
            if (country == null)
            {
                return NotFound();
            }
            return PartialView(country);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Edit Countries")]
        public async Task<IActionResult> Edit(int id, [Bind("CountryID,ISO,Iso3,Name,NiceName,NumCode,PhoneCode")] Country country)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            country.CompanyID = CompID;
            country.CreatedBy = User.Identity.Name;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(country);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CountryExists(country.CountryID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return PartialView(country);
        }
        
        private bool CountryExists(int id)
        {
            return _context.Country.Any(e => e.CountryID == id);
        }
        
        [Route("[action]")]
        public JsonResult LoadCountries()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            return Json(new { data = _context.Country.Where(r=>r.CompanyID == CompID).ToList() });
        }
    }
}


//[Route("[action]")]
//public async Task<IActionResult> Details(int? id)
//{
//    if (id == null)
//    {
//        return NotFound();
//    }

//    var country = await _context.Country.SingleOrDefaultAsync(m => m.CountryID == id);
//    if (country == null)
//    {
//        return NotFound();
//    }

//    return PartialView(country);
//}

//[Route("delete")]
//public async Task<IActionResult> Delete(int? id)
//{
//    if (id == null)
//    {
//        return NotFound();
//    }

//    var country = await _context.Country.SingleOrDefaultAsync(m => m.CountryID == id);
//    if (country == null)
//    {
//        return NotFound();
//    }

//    return View(country);
//}


//[ActionName("Delete")]
//[HttpPost, ValidateAntiForgeryToken]
//[Route("delete")]
//public async Task<IActionResult> DeleteConfirmed(Country ctry)
//{
//    var country = await _context.Country.SingleOrDefaultAsync(m => m.CountryID == ctry.CountryID);
//    _context.Country.Remove(country);
//    await _context.SaveChangesAsync();
//    return RedirectToAction("Index");
//}
