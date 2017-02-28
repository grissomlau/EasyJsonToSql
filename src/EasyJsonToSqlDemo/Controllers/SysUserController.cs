using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using EasyJsonToSql;
using MySql.Data.MySqlClient;
using System.Data;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Xml.Serialization;
using System.Text;

namespace EasyJsonToSql.Demo.Controllers
{
    public class SysUserController : Controller
    {
        // GET: SysUser
        public ActionResult Index()
        {
            return View();
        }
    }
    public class UserController : ApiController
    {
        // 同时兼容 xml 和 json
        string sqlXml = @"
<SqlConfig>
    <Select>
        user.*
    </Select>
    <From>
        BasUser user
    </From>
    <Where>
    </Where>
    <IDs>Id</IDs>
    <Table>BasUser</Table>
    <Insert>
        <Fields>
            <Field Name=""Name"" IsIgnore=""false""></Field>
        </Fields>
    </Insert>
</SqlConfig>
            ";

        string sqlJson = @"
{
    ""Select"":""user.*"",
    ""From"":""BasUser user"",
    ""ID"":""Id"",
    ""Table"":""BasUser"",
    ""Insert"":{
        ""Fields"":[
            {""Name"":""Name"",""IsIgnore"":""false""}
            ]
        }
    }
";

        string cnnStr = "server=192.168.10.119;database=easyjrm_demo;user=root;pwd=root";
        public dynamic Get()
        {
            var dt = new DataTable();
            
            // 用 xml 的配置
            //var sqlconfig = LoadFromXml<SqlConfig>(sqlXml);

            // 用 json 的配置
            var sqlconfig = JsonConvert.DeserializeObject<SqlConfig>(sqlJson);
            var sqlSb = new StringBuilder();
            sqlSb.AppendFormat("Select {0} From {1}", sqlconfig.Select, sqlconfig.From);
            using (var da = new MySqlDataAdapter(sqlSb.ToString(), cnnStr))
            {
                da.Fill(dt);
            }
            return dt;
        }

        public dynamic Post()
        {
            var json = "";
            using (StreamReader sr = new StreamReader(HttpContext.Current.Request.InputStream))
            {
                json = sr.ReadToEnd();
            }
            var jobj = JObject.Parse(json);
            // 用 xml 的配置
            // var sqlconfig = LoadFromXml<SqlConfig>(sqlXml);

            // 用 json 的配置
            var sqlconfig = JsonConvert.DeserializeObject<SqlConfig>(sqlJson);
            var builder = new Proxy().ToDbBuilders(sqlconfig, jobj);

            var insertSqlSb = new StringBuilder(); 
            //获取第一个sqlconfig
            var data = builder[0].Data;
            insertSqlSb.AppendFormat("insert into {0}(", data.TableName);
            var valueSqlSb = new StringBuilder();
            var paras = new List<MySqlParameter>();
            foreach (var dbField in data.Fields)
            {
                // 不是自增的字段才添加
                if (!dbField.IsId)
                {
                    insertSqlSb.AppendFormat("{0}", dbField.DbName);
                    valueSqlSb.AppendFormat("@{0}", dbField.DbName);
                    paras.Add(new MySqlParameter("@" + dbField.DbName, dbField.Value));
                }
            }
            insertSqlSb.AppendFormat(") values({0})", valueSqlSb);

            var affectCount = 0;
            using (var cnn = new MySqlConnection(cnnStr))
            {
                using (var cmd = new MySqlCommand(insertSqlSb.ToString(), cnn))
                {
                    cnn.Open();
                    cmd.Parameters.AddRange(paras.ToArray());
                    affectCount = cmd.ExecuteNonQuery();
                }
            }
            return affectCount;
        }

        static T LoadFromXml<T>(string xml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(xml);
            MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length);
            var sqllSetting = xmlSerializer.Deserialize(ms);
            return (T)sqllSetting;
        }

    }


}
