using EasyJsonToSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyJsonToSql
{
    public class DbBuilder : IDbBuilder
    {
        public BuilderData Data { get; set; }

        public DbBuilder(SqlConfig setting)
        {
            this.Data = new BuilderData(setting);
            if (setting != null)
                this.Data.TableName = setting.Table;
        }

        public IDbBuilder AddChild(IDbBuilder child)
        {
            child.Data.Parent = this;
            this.Data.Children.Add(child);
            return this;
        }

        public IDbBuilder SetFieldValue(string name, object value)
        {
            var field = this.Data.AllFields.FirstOrDefault(x => x.Name == name);
            field.Value = value;
            return this;
        }

        public object GetFieldValue(string name)
        {
            return this.Data.AllFields.FirstOrDefault(x => x.Name == name);
        }

        public DbField GetField(string name)
        {
            return this.Data.AllFields.FirstOrDefault(x => x.Name == name);
        }

        public IDbBuilder AddWhere(string sql, params object[] paramArr)
        {
            this.Data.Where += " " + string.Format(sql, paramArr);
            return this;
        }

        public IDbBuilder AddParam(string paramName, object value)
        {
            var para = this.Data.Params.FirstOrDefault(x => x.ParamName.Trim() == paramName.Trim());
            if (para != null)
            {
                para.Value = value;
                //throw new Exception("Params already exist " + paramName + ", please assign another name for this added param!");
            }
            else {
                this.Data.Params.Add(new DbField { Name = paramName, Value = value, ParamName = paramName });
            }
            return this;
        }


        public IDbBuilder AddField(DbField field)
        {
            this.Data.AllFields.Add(field);
            this.Data.Fields.Add(field);
            return this;
        }

        public IDbBuilder AddAllField(DbField field)
        {
            this.Data.AllFields.Add(field);
            return this;
        }
    }
}
