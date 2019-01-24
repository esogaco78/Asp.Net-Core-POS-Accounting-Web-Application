using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Invento.Areas.CompanyAdmin.Models.Company;
using Invento.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Invento.Areas.Finance.Models;

namespace Invento.Areas.CompanyAdmin.Controllers
{
    [Authorize(Roles = "BiznsBook")]
    [Area("CompanyAdmin")]
    [Route("Company/[controller]")]
    public class PartiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PartiesController(ApplicationDbContext context)
        {
            _context = context;    
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Parties")]
        public IActionResult Index()
        {      
            return View();
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,View Parties Details")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parties = await _context.Parties.SingleOrDefaultAsync(m => m.PartiesID == id);
            if (parties == null)
            {
                return NotFound();
            }

            return PartialView(parties);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Add Parties")]
        public IActionResult Create()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            ViewData["CountryID"] = new SelectList(_context.Country.Where(r => r.CompanyID == CompID), "CountryID", "Name");
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Add Parties")]
        public async Task<IActionResult> Create([Bind("PartiesID,AdditionalInfo1,AdditionalInfo2,Area,BusinessRelation,City,CompanyID,ContactPerson,CountryID,CreatedBy,Email,ExtraBool,TransactionAccountID,ExtraString,Fax,Observations,OtherDetails,PartyName,PartyShortName,Phone1,Phone2,Remarks,Road,State")] Parties parties)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            parties.CompanyID = CompID;
            parties.CreatedBy = User.Identity.Name;

            if (ModelState.IsValid)
            {
                // Chart of Account TRANSACTION ACCOUNT ENTRY
                TransactionAccount TraAcc = new TransactionAccount();
                
                TraAcc.AccountName = parties.PartyName;
                TraAcc.CompanyID = CompID;
                TraAcc.CreatedBy = User.Identity.Name;
                TraAcc.CreationDate = DateTime.Now;
                TraAcc.OpeningBalance = 0;

                int subAccID = _context.SubAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountNumber == "0000").FirstOrDefault().SubAccountID;

                if(_context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == subAccID).Any() == false)
                {
                    TraAcc.TransactionAccountNumber = "0001";
                }
                else
                {
                    int CountTransAcc = _context.TransactionAccount.Where(r => r.CompanyID == CompID).Where(r => r.SubAccountID == subAccID).Count();
                    int ADD_CountTransAcc = CountTransAcc + 1;
                    if(ADD_CountTransAcc < 10)
                    {
                        TraAcc.TransactionAccountNumber = "000" + ADD_CountTransAcc;
                    }
                    else if (ADD_CountTransAcc < 100)
                    {
                        TraAcc.TransactionAccountNumber = "00" + ADD_CountTransAcc;
                    }
                    else if (ADD_CountTransAcc < 1000)
                    {
                        TraAcc.TransactionAccountNumber = "0" + ADD_CountTransAcc;
                    }
                    else
                    {
                        TraAcc.TransactionAccountNumber = ADD_CountTransAcc.ToString();
                    }
                }

                TraAcc.SubAccountID = subAccID;
                
                _context.Add(TraAcc);
                await _context.SaveChangesAsync();
                int New_TransID = TraAcc.TransactionAccountID;
                // Here TransactionAccountID is Party Transaction AccountID

                parties.TransactionAccountID = New_TransID;

                _context.Add(parties);

                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            ViewData["CountryID"] = new SelectList(_context.Country.Where(r => r.CompanyID == CompID), "CountryID", "Name", parties.CountryID);
            return View(parties);
        }

        [Route("[action]")]
        [Authorize(Roles = "CompanyAdmin,Edit Parties")]
        public async Task<IActionResult> Edit(int? id)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            if (id == null)
            {
                return NotFound();
            }

            var parties = await _context.Parties.SingleOrDefaultAsync(m => m.PartiesID == id);
            if (parties == null)
            {
                return NotFound();
            }
            ViewData["CountryID"] = new SelectList(_context.Country.Where(r => r.CompanyID == CompID), "CountryID", "Name", parties.CountryID);
            return View(parties);
        }

        [Route("[action]")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CompanyAdmin,Edit Parties")]
        public async Task<IActionResult> Edit([Bind("PartiesID,AdditionalInfo1,AdditionalInfo2,Area,BusinessRelation,City ,ContactPerson,CountryID ,Email,ExtraBool,TransactionAccountID,ExtraString,Fax,Observations,OtherDetails,PartyName,PartyShortName,Phone1,Phone2,Remarks,Road,State")] Parties parties)
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            parties.CompanyID = CompID;
            parties.CreatedBy = User.Identity.Name;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Database.ExecuteSqlCommand("UPDATE [Invento].[dbo].[TransactionAccount] SET AccountName = @p0 where CompanyID = @p1 AND TransactionAccountID = @p2", parties.PartyName , CompID , parties.TransactionAccountID);
                    _context.Update(parties);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PartiesExists(parties.PartiesID))
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
            ViewData["CountryID"] = new SelectList(_context.Country.Where(r => r.CompanyID == CompID), "CountryID", "Name", parties.CountryID);
            return View(parties);
        }

        private bool PartiesExists(int id)
        {
            return _context.Parties.Any(e => e.PartiesID == id);
        }

        [Route("[action]")]
        public JsonResult LoadParties()
        {
            string CompId = User.Claims.Where(r => r.Type == "CompanyID").FirstOrDefault().Value;
            int CompID = Convert.ToInt32(CompId);

            var data = _context.Parties.Where(r => r.CompanyID == CompID).ToList();
            return Json(new { data = data });
        }

    }
}
