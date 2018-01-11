using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QY.Admin.Logic;
using QY.Admin.Logic.Models;

namespace QY.Admin.Web.Controllers
{
    public class UsersController : Controller
    {
        [Authorize(Roles = "Admin")]
        public ActionResult Users()
        {
            var list = new UserService().GetAllUsers();
            return View(list);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult UpdateUser(int id, string from = "Users")
        {
            ViewBag.From = from;
            var user = new UserService().GetUser(id);
            return PartialView(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult UpdateUser(User model, string from = "Users", string filter = null)
        {
            var service = new UserService();
            var user = service.GetUser(model.Id);
            user.EmailAddress = model.EmailAddress;
            user.EnglishName = model.EnglishName;
            user.ChineseName = model.ChineseName;
            user.IsManager = model.IsManager;
            user.department = model.department;
            user.IsExcluded = model.IsExcluded;
            service.UpdateUser(user, User.Identity.Name);
            return RedirectToAction(from, new { filter = filter });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CreateUser()
        {
            var model = new User();
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult CreateUser(User model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedBy = User.Identity.Name;
                model.CreatedTime = DateTime.Now;
                model.IsExcluded = false;
                new UserService().CreateUser(model);
            }
            return RedirectToAction("Users");
        }
    }
}