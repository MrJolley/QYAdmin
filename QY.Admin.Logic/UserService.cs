using QY.Admin.Logic.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QY.Admin.Logic
{
    public class UserService
    {
        public List<User> GetAllUsers()
        {
            using (QYDbContext db = new QYDbContext())
            {
                return db.Users.Where(r => r.IsExcluded == false).OrderBy(r => r.EnglishName).ToList();
            }
        }

        public User GetUser(int id)
        {
            using (QYDbContext db = new QYDbContext())
            {
                return db.Users.Find(id);
            }
        }

        public int UpdateUser(User user, string loginName = null)
        {
            using (QYDbContext db = new QYDbContext())
            {
                var userDb = db.Users.Find(user.Id);
                AutoMapper.Mapper.Map(user, userDb);
                if (!string.IsNullOrWhiteSpace(loginName))
                {
                    userDb.UpdatedBy = loginName;
                    userDb.UpdatedTime = DateTime.Now;
                }
                db.Users.Attach(userDb);
                db.Entry(userDb).State = EntityState.Modified;
                return db.SaveChanges();
            }
        }

        public int CreateUser(User user)
        {
            using (QYDbContext db = new QYDbContext())
            {
                db.Users.Add(user);
                return db.SaveChanges();
            }
        }
    }
}
