using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Diagnostics;
using System.Text;
using MySql.Data.MySqlClient;
using StackExchange.Profiling;

namespace SFL.Database.Sql
{
    public enum CommandType
    {
        SQL,
        StoredProcedure
    }

    public abstract class AbstractDbAction<T>
    {
        //private const string RESERVED_KEYS = "@@SPID";

        private const string CONN_STRING_KEY = "sfl_default_conn_str";
        private CommandType _commandType = CommandType.SQL;
        private String _connectionString = "SQL_Conn";
        public AbstractDbAction()
        {
            var connName = ConfigurationManager.AppSettings[CONN_STRING_KEY];
            if (!String.IsNullOrEmpty(connName))
                _connectionString = connName;
        }

        public AbstractDbAction(string connectStringName)
        {
            _connectionString = connectStringName;
        }

        #region Must Implement

        protected abstract string SetQueryString();

        #endregion

        public CommandType CommandType
        {
            get { return _commandType; }
        }

        public String ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        public T Execute(params object[] parameters)
        {
            var profiler = MiniProfiler.Current; // it's ok if this is null
            var stacktrace = new StackTrace();
            using (profiler.Step(stacktrace.GetFrame(2).GetMethod().Name
                + "/"
                + stacktrace.GetFrame(1).GetMethod().Name))
            {
                return Main(parameters);
            }
        }

        public T Main(params object[] param)
        {
            // Init Connection String
            Init(ref _commandType);

            ISqlCommandProvider commandProvider;
            using (var database = CreateDatabase())
            {
                try
                {
                    // Create command
                    String sql = SetQueryString();
                    DbCommand command;
                    if (CommandType == CommandType.SQL)
                        command = database.GetSqlTextCommand(sql, param);
                    else
                    {
                        String paraNames = SetStoredProcedureParamNames();
                        command = database.GetStoredProcCommand(sql, paraNames, param);
                    }

                    // Execute query
                    Object returnValue;
                    DataSet ret = new DataSet();

                    command.CommandTimeout = 600;
                    BeforeExecute(database, command, param);
                    var adapter = database.CreateDbDataAdapter(command);
                    adapter.Fill(ret);

                    // Get return value in return type
                    if (ret.Tables.Count == 0)
                        return default(T);

                    returnValue = DbTypeUtil.GetCastedResult<T>(ret);
                    T retT = (T)returnValue;
                    AfterExecute(retT, database, command, param);

                    return retT;
                }
                finally
                {
                    database.Connection.Close();
                }
            }
        }

        protected DbDataAdapter CreateDataAdapter(DbConnection conn, DbCommand cmd)
        {
            if (conn is SqlConnection)
                return new SqlDataAdapter(cmd as SqlCommand);

            if (conn is MySqlConnection)
                return new MySqlDataAdapter(cmd as MySqlCommand);

            return new OleDbDataAdapter(cmd as OleDbCommand);
        }


        #region Extensable Process
        virtual protected void Init(ref CommandType commandType)
        {
        }

        virtual protected string SetStoredProcedureParamNames()
        {
            return "";
        }


        virtual protected Database CreateDatabase()
        {
            string providerName = ConfigurationManager.ConnectionStrings[ConnectionString].ProviderName;
            string connString = ConfigurationManager.ConnectionStrings[ConnectionString].ToString();

            return DbFactory.Create(connString, providerName);
        }

        virtual protected void BeforeExecute(Database db, DbCommand command, object[] inParam)
        {
        }

        virtual protected void AfterExecute(T result, Database db, DbCommand command, object[] inParam)
        {
        }
        #endregion

    }
}
