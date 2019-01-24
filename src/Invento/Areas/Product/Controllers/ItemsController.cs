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
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ItemsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Products")]
        public IActionResult Index()
        {
            //string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            //int CompID = Convert.ToInt32(CompId);
            //ViewData["CompanyName"] = _context.CompanyProfile.Where(r => r.CompanyID == CompID).FirstOrDefault().CompanyName;
            return View();
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Products Details")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Item.SingleOrDefaultAsync(m => m.ItemID == id);
            if (item == null)
            {
                return NotFound();
            }

            return PartialView(item);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Add Products")]
        public IActionResult Create()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            ViewData["ProductGroupID"] = new SelectList(_context.ProductGroup.Where(r=>r.CompanyID == CompID), "ProductGroupID", "ProductGroupName");
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Add Products")]
        public async Task<IActionResult> Create([Bind("ItemID,CheckCase_1,CheckCase_2,Date,CrossRef,ItemExtra_Dec_1,ItemExtra_Dec_2,ItemExtra_Dec_3,ItemExtra_Dec_4,ItemExtra_Dec_5,ItemExtra_Int_1,ItemExtra_Int_2,ItemExtra_Int_3,ItemExtra_Int_4,ItemExtra_Int_5,ItemExtra_String_1,ItemExtra_String_2,ItemExtra_String_3,ItemExtra_String_4,ItemExtra_String_5,ItemMainCompany,ItemName,ItemType,ItemType2,LCPrice,NotMappedBool,NotMappedInt,NotMappedString_1,NotMappedString_2,OEMNo,PhotoData,ProductDescription,ProductGroupID,Remark,Quantity,SalePrice,Size,Value")] Item item, IFormFile File)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            item.CompanyID = CompID;
            item.CreatedBy = User.Identity.Name;
            item.Date = DateTime.Today;

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
                    item.PhotoData = data;                    
                }


                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ProductGroupID"] = new SelectList(_context.ProductGroup.Where(r => r.CompanyID == CompID), "ProductGroupID", "ProductGroupName", item.ProductGroupID);
            return View(item);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Edit Products")]
        public async Task<IActionResult> Edit(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Item.SingleOrDefaultAsync(m => m.ItemID == id);
            if (item == null)
            {
                return NotFound();
            }
            ViewData["ProductGroupID"] = new SelectList(_context.ProductGroup.Where(r => r.CompanyID == CompID), "ProductGroupID", "ProductGroupName", item.ProductGroupID);
            return View(item);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Edit Products")]
        public async Task<IActionResult> Edit([Bind("ItemID,CheckCase_1,CheckCase_2,Date,CrossRef,ItemExtra_Dec_1,ItemExtra_Dec_2,ItemExtra_Dec_3,ItemExtra_Dec_4,ItemExtra_Dec_5,ItemExtra_Int_1,ItemExtra_Int_2,ItemExtra_Int_3,ItemExtra_Int_4,ItemExtra_Int_5,ItemExtra_String_1,ItemExtra_String_2,ItemExtra_String_3,ItemExtra_String_4,ItemExtra_String_5,ItemMainCompany,ItemName,ItemType,ItemType2,LCPrice,NotMappedBool,NotMappedInt,NotMappedString_1,NotMappedString_2,OEMNo,PhotoData,ProductDescription,ProductGroupID,Remark,Quantity,SalePrice,Size,Value")] Item item, IFormFile File)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            item.CompanyID = CompID;
            item.CreatedBy = User.Identity.Name;
            item.Date = DateTime.Today;
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
                    item.PhotoData = data;                    
                }
                else if (File == null)
                {
                    item.PhotoData = item.PhotoData;                   
                }
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.ItemID))
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
            ViewData["ProductGroupID"] = new SelectList(_context.ProductGroup.Where(r => r.CompanyID == CompID), "ProductGroupID", "ProductGroupName", item.ProductGroupID);
            return View(item);
        }

        private bool ItemExists(int id)
        {
            return _context.Item.Any(e => e.ItemID == id);
        }

        [Route("[action]")]
        public JsonResult LoadItems()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            //var data = _context.Item.Where(r => r.CompanyID == CompID).ToList();
            var data = _context.Item.Where(r => r.CompanyID == CompID).Select(x => new { x.ItemID, x.CreatedBy, x.CrossRef, x.ItemMainCompany, x.ItemName, x.ItemType, x.LCPrice, x.OEMNo, x.ProductDescription, x.ProductGroup, x.Quantity, x.Remark, x.SalePrice, x.Size, x.ItemType2,x.ItemExtra_String_1, x.ItemExtra_String_2, x.ItemExtra_String_3 }).ToList();
            return Json(new { data = data });
        }

        [Route("[action]")]
        public JsonResult LoadItemsStockCard()
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
             var data = _context.Item.Where(r => r.CompanyID == CompID).ToList();
             return Json(new { data = data });
        }

        [Route("[action]")]
        [Authorize]
        public IActionResult ItemPhoto(int id)
        {            
            ItemDetailVM VM = new ItemDetailVM();

            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var data = _context.Item.Where(r => r.CompanyID == CompID).Where(m => m.ItemID == id).FirstOrDefault();

            byte[] photo = data.PhotoData;
            if(photo != null)
            { 
                string imageBase64 = Convert.ToBase64String(photo);
                string imageSrc = string.Format("data:image/gif;base64,{0}", imageBase64);
                VM.ItemPhoto = imageSrc;
            }            
            VM.ItemName = data.ItemName;
            VM.ItemDetailVMID = data.ItemID;
            VM.Quantity = data.Quantity;

            return PartialView(VM);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Stock Card")]
        public IActionResult StockCard()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);
            ViewData["CompanyName"] = _context.CompanyProfile.Where(r => r.CompanyID == CompID).FirstOrDefault().CompanyName;
            return View();
        }

        [Route("[action]")]
        [Authorize]
        public JsonResult CheckOEM(string OEM)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var a = _context.Item.Where(r => r.CompanyID == CompID).Where(p => p.OEMNo == OEM).Any();
            return Json(a);
        }

        [Route("[action]")]
        [Authorize]
        public JsonResult CheckCrossRef(string CrossRef)
        {
            int? CompID = HttpContext.Session.GetInt32("CompanyID");
            var a = _context.Item.Where(r => r.CompanyID == CompID).Where(p => p.CrossRef == CrossRef).Any();
            return Json(a);
        }
    }
}
