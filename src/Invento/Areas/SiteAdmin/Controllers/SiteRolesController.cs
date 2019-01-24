using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Invento.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Invento.Controllers
{
    //[Authorize(Roles = "SiteAdmin")]
    [Area("SiteAdmin")]
    [Route("SiteAdmin/[controller]")]
    public class SiteRolesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SiteRolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("[action]")]
        public ActionResult Index()
        {
            var Roles = _context.Roles.ToList();
            return View(Roles);
        }

        [Route("[action]")]
        public ActionResult Create()
        {
            var Role = new IdentityRole();
            return View(Role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[action]")]
        public async Task<IActionResult> Create(IdentityRole Role)
        {
            Role.NormalizedName = Role.Name.ToUpper();
            if (ModelState.IsValid)
            {
                _context.Add(Role);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}