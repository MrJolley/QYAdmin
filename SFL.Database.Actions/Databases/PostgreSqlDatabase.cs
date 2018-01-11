using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Npgsql;

namespace SFL.Database.Sql.Databases
{
    class PostgreSqlDatabase : Database
    {
        public PostgreSqlDatabase(String connString)
            : base(new NpgsqlConnection(connString))
        {
        }

        override public DbDataAdapter CreateDbDataAdapter(DbCommand cmd)
        {
            return new NpgsqlDataAdapter(cmd as NpgsqlCommand);
        }

        override public void SpecifyCommandText(DbCommand command)
        {
            ResetCommandParamKey(command, ":");
        }
    }
}
