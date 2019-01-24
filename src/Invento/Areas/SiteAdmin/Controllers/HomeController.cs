using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Invento.Models;
using Invento.Data;
using Microsoft.EntityFrameworkCore;
using Invento.Areas.CompanyAdmin.Models.Company;

namespace Invento.Areas.SiteAdmin.Controllers
{
    [Authorize(Roles = "SiteAdmin")]
    [Area("SiteAdmin")]
    [Route("SiteAdmin/[controller]")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }


        [Route("[action]")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("[action]")]
        [Authorize(Roles = "SiteAdmin")]
        public IActionResult MyUsers()
        {            
            string CurrentRole;
            var userList = new List<SiteUserInfo>();
            foreach (var user in _context.Users.Include(r => r.Roles).OrderByDescending(r=>r.CompanyID).ToList())
            {
                var userRolesId = user.Roles.Select(m => m.RoleId).ToList();
                var model = new SiteUserInfo()
                {
                    UserID = user.Id,
                    Email = user.Email,
                    CompanyID = user.CompanyID,
                    AccountActive = user.AccountActive,
                    Roles = _context.Roles.Where(r => userRolesId.Contains(r.Id)).ToList()
                };
                List<string> RoleListTemp = new List<string>();
                string RoleNamesTemp;
                for (int i = 0; i < model.Roles.Count; i++)
                {
                    CurrentRole = model.Roles[i].Name;
                    RoleListTemp.Add(CurrentRole);
                }
                RoleNamesTemp = string.Join(" , ", RoleListTemp.ToArray());
                model.RoleName = RoleNamesTemp;
                userList.Add(model);
            }
            userList.RemoveAll(r => r.RoleName == "SiteAdmin");            
            return View(userList);
        }

        [Route("[action]")]
        [Authorize(Roles = "SiteAdmin")]
        public async Task<IActionResult> AccountActive([Bind("UserID")] string id)
        {
            ApplicationUser UserModel = new ApplicationUser();
            UserModel = _context.Users.Where(r => r.Id == id).FirstOrDefault();

            UserModel.AccountActive = false;

            _context.Users.Update(UserModel);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyUsers");
        }

        [Route("[action]")]
        [Authorize(Roles = "SiteAdmin")]
        public async Task<IActionResult> AccountDeActive([Bind("UserID")] string id)
        {
            ApplicationUser UserModel = new ApplicationUser();
            UserModel = _context.Users.Where(r => r.Id == id).FirstOrDefault();

            UserModel.AccountActive = true;

            _context.Users.Update(UserModel);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyUsers");
        }

    }
}