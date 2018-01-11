using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace SFL.Database.Sql.Databases
{
    class DefaultDatabase : Database
    {
        public DefaultDatabase(String connString)
            : base(new OleDbConnection(connString))
        {
        }

        override public DbDataAdapter CreateDbDataAdapter(DbCommand cmd)
        {
            return new OleDbDataAdapter(cmd as OleDbCommand);
        }

        override public void SpecifyCommandText(DbCommand command)
        {
            ResetCommandParamKey(command, ".");
        }
    }
}
