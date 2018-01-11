using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFL.Database.Sql.Databases;

namespace SFL.Database.Sql
{
    class DbFactory
    {
        public static readonly String SQL_SERVER = "System.Data.SqlClient";
        public static readonly String MY_SQL = "MySql.Data.MySqlClient";
        public static readonly String POSTGRE_SQL = "Npgsql";

        public static Database Create(String connString, String providerName = "")
        {
            if (String.IsNullOrEmpty(providerName) ||
                providerName == SQL_SERVER)
                return new SqlServerDatabase(connString);

            if (providerName == POSTGRE_SQL)
                return new PostgreSqlDatabase(connString);

            if (providerName == MY_SQL)
                return new MySqlDatabase(connString);

            return new DefaultDatabase(connString);
        }
    }
}
