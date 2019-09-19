using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCenter.Repository
{
    public static class DbDataReaderExtensions
    {
        public static string GetStringNullSafe(this DbDataReader reader, int colIndex, string defaultValue = "")
        {
            return reader.IsDBNull(colIndex)
                ? defaultValue
                : reader.GetString(colIndex);
        }
    }
}
