using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyJsonToSql
{
    public interface IDbBuilder
    {
        BuilderData Data { get; set; }
        IDbBuilder AddChild(IDbBuilder child);
        IDbBuilder SetFieldValue(string name, object value);
        object GetFieldValue(string name);
        DbField GetField(string name);
        IDbBuilder AddField(DbField field);
        IDbBuilder AddAllField(DbField field);
        IDbBuilder AddWhere(string sql, params object[] paramArr);
        IDbBuilder AddParam(string paramName, object value);
    }
}
