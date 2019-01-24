using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Invento.Areas.Product.Models;
using Invento.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace Invento.Areas.Product.Controllers
{
    [Authorize(Roles = "BiznsBook")]
    [Area("Product")]
    [Route("Product/[controller]")]
    public class ProductGroupsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductGroupsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Product Groups")]
        public IActionResult Index()
        {
            //return View(await _context.ProductGroup.ToListAsync());
            return View();
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Add Product Groups")]
        public IActionResult Create()
        {
            return PartialView();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Add Product Groups")]
        public async Task<IActionResult> Create([Bind("ProductGroupID,CompanyID,CreatedBy,ProductGroupName")] ProductGroup productGroup)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            productGroup.CompanyID = CompID;
            productGroup.CreatedBy = User.Identity.Name;

            if (ModelState.IsValid)
            {
                _context.Add(productGroup);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(productGroup);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Edit Product Groups")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productGroup = await _context.ProductGroup.SingleOrDefaultAsync(m => m.ProductGroupID == id);
            if (productGroup == null)
            {
                return NotFound();
            }
            return PartialView(productGroup);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Edit Product Groups")]
        public async Task<IActionResult> Edit([Bind("ProductGroupID,CompanyID,CreatedBy,ProductGroupName")] ProductGroup productGroup)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            productGroup.CompanyID = CompID;
            productGroup.CreatedBy = User.Identity.Name;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productGroup);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductGroupExists(productGroup.ProductGroupID))
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
            return View(productGroup);
        }
        
        private bool ProductGroupExists(int id)
        {
            return _context.ProductGroup.Any(e => e.ProductGroupID == id);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Product Groups")]
        public JsonResult LoadProductGroups()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var data = _context.ProductGroup.Where(r => r.CompanyID == CompID).ToList();
            return Json(new { data = data });
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

//    var productGroup = await _context.ProductGroup.SingleOrDefaultAsync(m => m.ProductGroupID == id);
//    if (productGroup == null)
//    {
//        return NotFound();
//    }

//    return View(productGroup);
//}

//[Route("[action]")]
//public async Task<IActionResult> Delete(int? id)
//{
//    if (id == null)
//    {
//        return NotFound();
//    }

//    var productGroup = await _context.ProductGroup.SingleOrDefaultAsync(m => m.ProductGroupID == id);
//    if (productGroup == null)
//    {
//        return NotFound();
//    }

//    return View(productGroup);
//}

//[Route("[action]")]
//[HttpPost, ActionName("Delete")]
//[ValidateAntiForgeryToken]
//public async Task<IActionResult> DeleteConfirmed(int id)
//{
//    var productGroup = await _context.ProductGroup.SingleOrDefaultAsync(m => m.ProductGroupID == id);
//    _context.ProductGroup.Remove(productGroup);
//    await _context.SaveChangesAsync();
//    return RedirectToAction("Index");
//}
