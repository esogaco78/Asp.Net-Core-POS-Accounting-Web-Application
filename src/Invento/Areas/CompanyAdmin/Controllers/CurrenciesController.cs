using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Invento.Data;
using Microsoft.AspNetCore.Authorization;
using Invento.Areas.CompanyAdmin.Models.Company;
using System;

namespace Invento.Areas.SiteAdmin.Controllers
{    
    [Area("CompanyAdmin")]
    [Route("Company/[controller]")]
    [Authorize(Roles = "BiznsBook")]
    public class CurrenciesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CurrenciesController(ApplicationDbContext context)
        {
            _context = context;    
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Currencies")]
        public async Task<IActionResult> Index()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            return View(await _context.Currency.Where(r => r.CompanyID == CompID).ToListAsync());
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Add Currencies")]
        public IActionResult Create()
        {
            return PartialView();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Add Currencies")]
        public async Task<IActionResult> Create([Bind("CurrencyID,CurrencyName,ISO")] Currency currency)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            currency.CompanyID = CompID;
            currency.CreatedBy = User.Identity.Name;

            if (ModelState.IsValid)
            {
                _context.Add(currency);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return PartialView(currency);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Edit Currencies")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currency = await _context.Currency.SingleOrDefaultAsync(m => m.CurrencyID == id);
            if (currency == null)
            {
                return NotFound();
            }
            return PartialView(currency);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Edit Currencies")]
        public async Task<IActionResult> Edit(int id, [Bind("CurrencyID,CurrencyName,ISO")] Currency currency)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            currency.CompanyID = CompID;
            currency.CreatedBy = User.Identity.Name;           

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(currency);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CurrencyExists(currency.CurrencyID))
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
            return PartialView(currency);
        }

        private bool CurrencyExists(int id)
        {
            return _context.Currency.Any(e => e.CurrencyID == id);
        }

        [Route("[action]")]
        public JsonResult LoadCurriencies()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            return Json(new { data = _context.Currency.Where(r=>r.CompanyID == CompID).ToList() });
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

//    var currency = await _context.Currency.SingleOrDefaultAsync(m => m.CurrencyID == id);
//    if (currency == null)
//    {
//        return NotFound();
//    }

//    return View(currency);
//}



//[Route("[action]")]
//public async Task<IActionResult> Delete(int? id)
//{
//    if (id == null)
//    {
//        return NotFound();
//    }

//    var currency = await _context.Currency.SingleOrDefaultAsync(m => m.CurrencyID == id);
//    if (currency == null)
//    {
//        return NotFound();
//    }

//    return View(currency);
//}

//[Route("[action]")]
//[HttpPost, ActionName("Delete")]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> DeleteConfirmed(int id)
//{
//    var currency = await _context.Currency.SingleOrDefaultAsync(m => m.CurrencyID == id);
//    _context.Currency.Remove(currency);
//    await _context.SaveChangesAsync();
//    return RedirectToAction("Index");
//}