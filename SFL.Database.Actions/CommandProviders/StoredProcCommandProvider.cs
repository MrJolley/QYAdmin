using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SFL.Database.Sql.CommandProviders
{
    class StoredProcCommandProvider : ISqlCommandProvider
    {
        public Database Database
        {
            get;
            protected set;
        }

        public String ProcName
        {
            get;
            protected set;
        }

        public String Parameters
        {
            get;
            protected set;
        }

        public Object[] Inputs
        {
            get;
            protected set;
        }

        public StoredProcCommandProvider(Database database, String procName, String parameters, Object[] inputs)
        {
            ProcName = procName;
            Database = database;
            Inputs = inputs;
            Parameters = parameters;
        }

        private DbCommand GetSqlStringCommand(DbConnection conn, String query)
        {
            // Set Command
            var command = conn.CreateCommand(); ;
            command.CommandText = query.Trim();
            command.CommandType = System.Data.CommandType.StoredProcedure;

            return command;
        }

        private String[] GetProcParams(String query)
        {
            query = query.Replace("\r", "").Replace("\n", " ");

            char[] splitor = { ' ' };
            List<string> paramname = new List<string>();

            string[] ret = query.Split(splitor);
            for (int i = 0; i < ret.Length; i++)
            {
                String s = ret[i].Trim();
                if (!string.IsNullOrEmpty(s) &&
                    !paramname.Contains(s))
                    paramname.Add(s);
            }

            return paramname.ToArray();
        }

        private void SetProcCommandInputParamValue(DbCommand command, object[] param, String[] inParamNames)
        {
            // Set parameters
            for (int i = 0; i < inParamNames.Length; i++)
            {
                var para = command.CreateParameter();
                para.ParameterName = inParamNames[i];
                para.Value = param[i];

                if (para.Value is DataTable)
                {
                    var sqlPara = para as SqlParameter;
                    if (sqlPara == null)
                        throw new Exception("Only SQL Server supports a parameter of DataTable Type.");
                    sqlPara.SqlDbType = SqlDbType.Structured;
                }
                else
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
            String[] paras = GetProcParams(Parameters);

            var command = GetSqlStringCommand(Database.Connection, ProcName);
            SetProcCommandInputParamValue(command, Inputs, paras);

            return command;
        }
    }
}
