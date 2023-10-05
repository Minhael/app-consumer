using System.Data;
using System.Diagnostics;

namespace Common.Lang.Extensions
{
    [DebuggerStepThrough]
    public static class DataRowExtensions
    {
        /// <summary>
        /// Get value of the column and convert DBNull to null if value is null.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="columnName"></param>
        /// <returns>Nullable dotnet object</returns>
        public static object? GetNullable(this DataRow self, string columnName)
        {
            return self[columnName].Let(it => it == DBNull.Value ? null : it);
        }
    }
}