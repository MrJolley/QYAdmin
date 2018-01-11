using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using QY.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QY.Admin.Web.Models
{
    public class IdentityConfig
    {
        public class ApplicationRoleManager : RoleManager<ApplicationRole>
        {
            public ApplicationRoleManager(IRoleStore<ApplicationRole, string> roleStore)
                : base(roleStore)
            { }

            public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
            {
                return new ApplicationRoleManager(new RoleStore<ApplicationRole>(context.Get<ApplicationDbContext>()));
            }
        }

        public class ApplicationDbInitializer : System.Data.Entity.CreateDatabaseIfNotExists<ApplicationDbContext>
        {
            protected override void Seed(ApplicationDbContext context)
            {
                var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var roleManager = HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();

                //创建角色
                var roles = new List<ApplicationRole>
                      {
                          new ApplicationRole { Name="SuperAdmin",Description="系统超级管理员--负责用户角色创建和管理，用户权限分配"},
                          new ApplicationRole { Name="Admin",Description="系统管理员--受限制访问部分功能"},
                      };

                foreach (var role in roles)
                {
                    var _role = roleManager.FindByName(role.Name);
                    if (_role == null)
                    {
                        var roleResult = roleManager.Create(role);
                    }
                }

                //创建用户
                var users = new List<ApplicationUser>
                    {
                         new ApplicationUser { Email="jing.chen@sail-fs.com", UserName="admin", Id=Guid.NewGuid().ToString() },
                    };
                 
                foreach (var user in users)
                {
                    var _user = userManager.FindByName(user.UserName);
                    if (_user == null)
                    {
                        var userResult = userManager.Create(user, "admin.QY.com123");
                        var userLockResult = userManager.SetLockoutEnabled(user.Id, false);
                    }
                }

                //给用户添加角色
                foreach(var user in users)
                {
                    userManager.AddToRoles(userManager.FindByName(user.UserName).Id,roles.Select(c =>c.Name).ToArray());
                }

                base.Seed(context);
            }
        }

    }
}