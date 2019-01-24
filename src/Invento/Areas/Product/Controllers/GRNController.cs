using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Invento.Areas.Product.Models;
using Invento.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Invento.Areas.Product.Controllers
{
    [Authorize(Roles = "BiznsBook")]
    [Area("Product")]
    [Route("Product/[controller]")]
    public class GRNController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GRNController(ApplicationDbContext context)
        {
            _context = context;    
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Good Receive Notes")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Good Receive Notes Details")]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            GRNVM model = new GRNVM();

            model.ContactNumber = _context.GRN.Where(m => m.GRNID == id).SingleOrDefault().ContactNumber;
            model.ContactPerson = _context.GRN.Where(m => m.GRNID == id).SingleOrDefault().ContactPerson;
            model.CreatedBy = _context.GRN.Where(m => m.GRNID == id).SingleOrDefault().CreatedBy;
            model.GRNDate = _context.GRN.Where(m => m.GRNID == id).SingleOrDefault().GRNDate;
            model.PartyName = _context.GRN.Where(m => m.GRNID == id).Include(r=>r.Parties).FirstOrDefault().Parties.PartyName;
            model.Remarks = _context.GRN.Where(m => m.GRNID == id).SingleOrDefault().Remarks;
            model.TotalQuantity = _context.GRN.Where(m => m.GRNID == id).SingleOrDefault().TotalQuantity;

            model.GRNItem = _context.GRNItem.Where(r => r.GRNID == id).Include(r=>r.Item).ToList();

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Add Good Receive Notes")]
        public IActionResult Create()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r=>r.CompanyID == CompID), "PartiesID", "PartyName");
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Add Good Receive Notes")]
        public async Task<IActionResult> Create(GRN gRN , IEnumerable<Item> ItemList)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            // Main GRN
            gRN.CreatedBy = User.Identity.Name;
            gRN.CompanyID = CompID;

            _context.GRN.Add(gRN);
            await _context.SaveChangesAsync();
            // Main GRN

            int GRN_ID = gRN.GRNID;
            
            var ItemList_Result = ItemList.ToList();
            ItemList_Result.RemoveAll(x => x.CompanyID == 777777);
            ItemList_Result.ForEach(x => x.CompanyID = CompID);
            foreach (var item in ItemList_Result)
            {
                GRNItem grnlItem = new GRNItem();

                grnlItem.ItemID = item.ItemID;
                grnlItem.GRNID = GRN_ID;                
                grnlItem.Quantity = item.Quantity;
                grnlItem.CompanyID = CompID;

                _context.GRNItem.Add(grnlItem);
                await _context.SaveChangesAsync();                
            }

            ViewData["PartiesID"] = new SelectList(_context.Parties.Where(r => r.CompanyID == CompID), "PartiesID", "PartyName", gRN.PartiesID);
            return RedirectToAction("Index");
        }

        //[Route("[action]")]
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
        //    int CompID = Convert.ToInt32(CompId);

        //    if (id == null)
        //    {
        //        return NotFound();
        //    }
        //    GRN gRN = new GRN();
        //    gRN = await _context.GRN.Where(r=>r.CompanyID == CompID).Where(m => m.GRNID == id).Include(r => r.GRNItem).SingleOrDefaultAsync();
        //    if (gRN == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["PartiesID"] = new SelectList(_context.Parties, "PartiesID", "PartyName", gRN.PartiesID);
        //    return View(gRN);
        //}

        //[Route("[action]")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("GRNID,CompanyID,ContactNumber,ContactPerson,CreatedBy,GRNDate,GRN_I_Extra,PartiesID,Remarks,TotalQuantity")] GRN gRN)
        //{
        //    if (id != gRN.GRNID)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(gRN);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!GRNExists(gRN.GRNID))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction("Index");
        //    }
        //    ViewData["PartiesID"] = new SelectList(_context.Parties, "PartiesID", "PartyName", gRN.PartiesID);
        //    return View(gRN);
        //}

        //[Route("[action]")]
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var gRN = await _context.GRN.SingleOrDefaultAsync(m => m.GRNID == id);
        //    if (gRN == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(gRN);
        //}

        //[Route("[action]")]
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var gRN = await _context.GRN.SingleOrDefaultAsync(m => m.GRNID == id);
        //    _context.GRN.Remove(gRN);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        private bool GRNExists(int id)
        {
            return _context.GRN.Any(e => e.GRNID == id);
        }

        [Route("[action]")]
        public JsonResult LoadGRN()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var data = _context.GRN.Where(r => r.CompanyID == CompID).Select(x => new { x.GRNID , grnDate = x.GRNDate.ToString("d") ,x.Parties.PartyName ,x.TotalQuantity ,x.ContactPerson ,x.ContactNumber ,x.CreatedBy}).ToList();
            return Json(new { data = data });
        }

    }
}
