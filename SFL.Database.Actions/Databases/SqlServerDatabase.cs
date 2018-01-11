using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SFL.Database.Sql.Databases
{
    class SqlServerDatabase : Database
    {
        public SqlServerDatabase(String connString)
            : base(new SqlConnection(connString))
        {
        }

        override public DbDataAdapter CreateDbDataAdapter(DbCommand cmd)
        {
            return new SqlDataAdapter(cmd as SqlCommand);
        }

        override public void SpecifyCommandText(DbCommand command)
        {
            ResetCommandParamKey(command, "@");
        }
    }
}
