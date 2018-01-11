using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SFL.Database.Sql.CommandProviders
{
    class SqlCommandProvider : ISqlCommandProvider
    {
        public Database Database
        {
            get;
            protected set;
        }

        public String Sql
        {
            get;
            protected set;
        }

        public Object[] Inputs
        {
            get;
            protected set;
        }


        public SqlCommandProvider(Database database, String sql, Object[] inputs)
        {
            Sql = sql;
            Database = database;
            Inputs = inputs;
        }

        private DbCommand GetSqlStringCommand(DbConnection conn, String query)
        {
            // Set Command
            var command = conn.CreateCommand(); ;
            command.CommandText = query;

            return command;
        }

        private string[] GetSqlParamNames(string query)
        {
            string pattern = @"\?(\w*)\s";
            List<string> paramname = new List<string>();

            MatchCollection collection = Regex.Matches(query, pattern);

            foreach (Match m in collection)
            {
                string name = m.Groups[1].Value;
                if (!paramname.Contains(name))
                    paramname.Add(name);
                //string name = m.Value.Replace("\r", "").Trim();
                //if (!RESERVED_KEYS.Equals(name))
                //    if (!paramname.Contains(name))
                //        paramname.Add(name);
            }

            return paramname.ToArray();

        }

        private void SetSqlCommandInputParamValue(DbCommand command, object[] param, string query)
        {
            // Set parameters
            String[] inParamNames = GetSqlParamNames(query);
            for (int i = 0; i < inParamNames.Length; i++)
            {
                //var para = new MySqlParameter(inParamNames[i], param[i]);
                var para = command.CreateParameter();
                para.ParameterName = inParamNames[i];
                para.Value = param[i];
                para.DbType = DbTypeUtil.GetDBType(para.Value);

                if (para.DbType == DbType.String &&
                    para.Value != null &&
                    string.IsNullOrEmpty(para.Value.ToString()))
                    para.Value = null;

                if (para.Value == null)
                    para.Value = Convert.DBNull;

                command.Parameters.Add(para);
            }
        }

        public DbCommand GetDbCommand()
        {
            var command = GetSqlStringCommand(Database.Connection, Sql);
            SetSqlCommandInputParamValue(command, Inputs, Sql);
            Database.SpecifyCommandText(command);

            return command;
        }
    }
}
