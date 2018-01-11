using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace SFL.Database.Sql.Databases
{
    class MySqlDatabase : Database
    {
        public MySqlDatabase(String connString)
            : base(new MySqlConnection(connString))
        {
        }

        override public DbDataAdapter CreateDbDataAdapter(DbCommand cmd)
        {
            return new MySqlDataAdapter(cmd as MySqlCommand);
        }

        override public void SpecifyCommandText(DbCommand command)
        {
        }
    }
}
