using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using SFL.Database.Sql.CommandProviders;

namespace SFL.Database.Sql
{
    public abstract class Database : IDisposable
    {
        public DbConnection Connection
        {
            get;
            protected set;
        }

        public Database(DbConnection connection)
        {
            Connection = connection;
        }

        public abstract DbDataAdapter CreateDbDataAdapter(DbCommand cmd);

        public abstract void SpecifyCommandText(DbCommand command);

        protected void ResetCommandParamKey(DbCommand command, String newKey)
        {
            String cmdText = command.CommandText;
            foreach (DbParameter para in command.Parameters)
            {
                String name = para.ParameterName;
                cmdText = cmdText.Replace("?" + name, "@" + name);

            }
            command.CommandText = cmdText;
        }


        public DbCommand GetSqlTextCommand(String sql, object[] inputs)
        {
            var commandProvider = new SqlCommandProvider(this, sql, inputs);
            return commandProvider.GetDbCommand();
        }


        public DbCommand GetStoredProcCommand(String sql, String paraNames, object[] inputs)
        {
            var commandProvider = new StoredProcCommandProvider(this, sql, paraNames, inputs);
            return commandProvider.GetDbCommand();
        }

        public void Dispose()
        {
            Connection.Close();
            Connection.Dispose();
        }
    }
}
