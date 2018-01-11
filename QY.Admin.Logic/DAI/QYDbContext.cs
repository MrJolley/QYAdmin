using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Configuration;

namespace QY.Admin.Logic.Models
{
    public class QYDbContext : DbContext, IDisposable
    {
        public QYDbContext()
            : base(nameOrConnectionString: "DefaultConnection")
        {
            string autoMigrationSetting = ConfigurationManager.AppSettings["AutoDbMigration"];
            bool autoMigration = false;
            bool.TryParse(autoMigrationSetting, out autoMigration);
            if (!autoMigration)
                Database.SetInitializer<QYDbContext>(null);
            else
                Database.SetInitializer<QYDbContext>(new MigrateDatabaseToLatestVersion<QYDbContext, MyAppConfiguation>());
        }

        public class MyAppConfiguation : System.Data.Entity.Migrations.DbMigrationsConfiguration<QYDbContext>
        {
            public MyAppConfiguation()
            {
                this.AutomaticMigrationsEnabled = true;
                this.AutomaticMigrationDataLossAllowed = true;
            }
        }

        public virtual DbSet<User> Users { get; set; }
    }
}
