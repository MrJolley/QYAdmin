using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SFL.Database.Sql
{
    class DbTypeUtil
    {
        public static DbType GetDBType(object value)
        {
            if (value is int)
                return DbType.Int32;

            if (value is double)
                return DbType.Double;

            if (value is decimal)
                return DbType.Decimal;

            if (value is byte[])
                return DbType.Binary;

            if (value is Guid)
                return DbType.Guid;

            if (value is string)
                return DbType.String;

            if (value is bool)
                return DbType.Boolean;

            if (value is DateTime)
                return DbType.DateTime;

            if (value == null)
                return DbType.String;

            return DbType.Object;
        }

        public static T GetCastedResult<T>(Object value)
        {
            object ret = null;
            Type retType = typeof(T);
            if (!Convert.IsDBNull(value))
            {
                if (retType == typeof(int))
                    ret = Convert.ToInt32(value);
                else if (retType == typeof(double))
                    ret = Convert.ToDouble(value);
                else if (retType == typeof(decimal))
                    ret = Convert.ToDecimal(value);
                else if (retType == typeof(bool))
                    ret = Convert.ToBoolean(value);
                else if (retType == typeof(String))
                    ret = value.ToString();
                else if (retType == typeof(DateTime))
                    ret = Convert.ToDateTime(value.ToString());
                else
                    ret = value;
            }

            return (T)ret;
        }

        public static T GetCastedResult<T>(DataSet ds)
        {
            object ret = null;
            Type retType = typeof(T);
            if (retType == ds.GetType())
                ret = ds;
            else if (retType == typeof(DataTable))
                ret = ds.Tables[0];
            else if (ds.Tables[0].Rows.Count > 0)
            {
                object value = ds.Tables[0].Rows[0][0];
                ret = GetCastedResult<T>(value);
            }

            return (T)ret;
        }
    }
}
