using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyJsonToSql;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConEasyJsonToSqlDemo
{
    class Program
    {
        const string sqlJson = @"
{
    ""Select"":""user.*"",
    ""From"":""BasUser user"",
    ""Where"":{
        ""Fields"":[
            {""Name"":""Name"",""Cp"":""like""}
        ]
    },
    ""OrderBy"":{""Default"":""Id""},
    ""ID"":""Id"",
    ""Table"":""BasUser"",
    ""Insert"":{
        ""Fields"":[
            {""Name"":""Name"",""IsIgnore"":""false""}
            ]
        }
    }
";
        const string cnnStr = "server=192.168.10.127;database=easyjrm_demo;user=root;pwd=root";
        static void Main(string[] args)
        {
            // 插入数据
            var postJson = @"{""master"":{""inserted"":[{""data"":{""Name"":""abc1""}}]}}";
            var affectRows = Post(postJson);
            Console.WriteLine("影响行数: " + affectRows);
            Console.WriteLine("press any key to select where name like abc records ...");
            Console.ReadKey();

            // 查询数据
            var nameValues = new NameValueCollection();
            nameValues.Add("name", "abc");
            var dt = Get(nameValues);
            Console.WriteLine();
            Console.WriteLine("查询数据结果: ");
            Console.WriteLine();
            Console.Write(JsonConvert.SerializeObject(dt));
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("press any key to exit ...");
            Console.ReadKey();
        }

        public static DataTable Get(NameValueCollection nameValues)
        {
            var dt = new DataTable();

            // 用 xml 的配置
            //var sqlconfig = LoadFromXml<SqlConfig>(sqlXml);

            // 用 json 的配置
            var sqlconfig = JsonConvert.DeserializeObject<SqlConfig>(sqlJson);
            var sqlSb = new StringBuilder();
            var builder = new Proxy().ToSelectBuilder(sqlconfig, nameValues);

            var builderData = builder.Data;
            sqlSb.AppendFormat("Select {0} From {1} Where {2}", builderData.Select, builderData.From, builderData.Where);

            using (var da = new MySqlDataAdapter(sqlSb.ToString(), cnnStr))
            {
                da.Fill(dt);
            }
            return dt;
        }

        public static int Post(string postJson)
        {
            var jobj = JObject.Parse(postJson);
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
    }
}
