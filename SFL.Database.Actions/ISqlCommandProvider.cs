using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace SFL.Database.Sql
{
    interface ISqlCommandProvider
    {
        DbCommand GetDbCommand();
    }
}
