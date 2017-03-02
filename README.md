# EasyJsonToSql
把 json 结构数据解析成标准的 sql， 实现标准化和自动化的增删改查



###Proxy 
这个类是 EasyJsonToSql 入口，用来获取 SelectBuilder 和 DbBuilder 对象。
####方法

1. 获取 SelectBuilder

```csharp
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

    var sqlconfig = JsonConvert.DeserializeObject<SqlConfig>(sqlJson);
    var builder = new Proxy().ToSelectBuilder(sqlconfig, nameValues);

```
2. 获取 DbBuilder

```csharp

   // 插入数据
    var postJson = @"{""master"":{""inserted"":[{""data"":{""Name"":""abc1""}}]}}";

 	var jobj = JObject.Parse(postJson);
	var sqlconfig = JsonConvert.DeserializeObject<SqlConfig>(sqlJson);
	var builder = new Proxy().ToDbBuilders(sqlconfig, jobj);

```

###SelectBuilder
该类负责处理查询分析，把 json 转换成查询的 sql。
####关键属性
1. Data[SelectBuilderData]： 生成的 sql 对象。

####常用方法
1. AddWhere: 添加 where 条件 sql, 

```csharp 
 builder.AddWhere("And table1.Id = 1");
 builder.AddWhere("And table1.Id = @Id").AddParam（"Id",1);
```

2. AddParam: 添加参数, ```builder.AddParam("Id",1)``` 

####用法演示

```csharp

var data = builder.Data;
var sql = string.format("Select {0} From {1} Where {2}", data.Select, data.From, data.Where);

```

###DbBuilder
该类负责处理增删改分析，把 json 转换成增删改的 sql。
#### 关键属性
1. Data[BuilderData]: 生成的 sql 对象。

####常用方法
1. AddChild: 添加子表对象。
2. AddWhere: 添加 where 条件 sql;  
```csharp
 builder.AddWhere("And table1.Id = 1");
 builder.AddWhere("And table1.Id = @Id").AddParam("Id", 1);
```
 <!-- continue counter --> 
3. AddParam: 添加参数， ```builder.AddParam("Id",1)```; 

###SqlConfig
该类保存 select、from、where、insert、update、delete， 以及子表、依赖关系、自增字段、主键等 sql 相关对象，标准化 sql 配置以便脱离具体业务。  
用来初始化 SelectBuilder 和 DBBuilder， 实现标准化的增删改查操作。   
 上面就是用 json 配置的来反射出 SqlConfig, ```  var sqlconfig = JsonConvert.DeserializeObject<SqlConfig>(sqlJson);```
####关键字段
```csharp
public class SqlConfig
{
    public SqlConfig()
    {
        this.Where = new Where();
        this.Children = new List<SqlConfig>();
        this.OrderBy = new OrderBy();
        this.GroupBy = new GroupBy();
        this.Dependency = new Dependency();
        this.Insert = new Insert();
        this.Update = new Update();
        this.Delete = new Delete();
        this.SingleQuery = new SingleQuery();
        this.Export = new Export();
        this.Import = new Import();
        this.BillCodeRule = new BillCodeRule();
    }

    private string _settingName;
    /// <summary>
    /// 配置名称，默认和表名一致，一般不会用到，方法以后扩展,如一个配置文件出现相同的表时，用来区分不同的配置
    /// </summary>
    public string SettingName
    {
        get
        {
            if (string.IsNullOrEmpty(_settingName))
            {
                _settingName = Table;
            }
            return _settingName;
        }
        set
        {
            _settingName = value;
        }
    }

    #region 查询配置
    /// <summary>
    /// 查询的字段
    /// </summary>
    public string Select { get; set; }
    /// <summary>
    /// 查询的表名以及关联的表名，如 left join, right join
    /// </summary>
    public string From { get; set; }
    /// <summary>
    /// 查询的条件
    /// 前端返回的查询条件，只有出现在这些配置好的字段，才会生成为了 sql 的 where 条件，
    /// 没出现的字段会被忽略
    /// </summary>
    public Where Where { get; set; }
    /// <summary>
    /// 分页时必须会乃至的排序规则
    /// </summary>
    public OrderBy OrderBy { get; set; }

    public GroupBy GroupBy { get; set; }
    /// <summary>
    /// 页码
    /// </summary>
    public int PageNumber { get; set; }
    /// <summary>
    /// 页大小
    /// </summary>
    public int PageSize { get; set; }


    #endregion 查询配置

    /// <summary>
    /// 指定该配置所属于的表
    /// </summary>
    public string Table { get; set; }

    #region 增删改配置
    /// <summary>
    /// 对应前端返回的 json 格式数据的键名
    /// e.g.: {master:{inserted:[{data:{}}]}} 中的 master 就是这里要对应的 JsonName
    /// 注意默认主表的 jsonName 是 master， 所以主表一般可省略不写, 但子表必须得指定
    /// </summary>
    public string JsonName { get; set; }
    /// <summary>
    /// 自增的字段,指定了自增的字段，在 insert 时会自动忽略该字段
    /// </summary>
    public string ID { get; set; }
    /// <summary>
    /// 主键, 在保存成功后会返回主键的值; 
    /// </summary>
    public string PKs { get; set; }
    /// <summary>
    /// 唯一值的字段，对应数据库 unique, 在 insert,update 前会判断是否已存在
    /// </summary>
    public string Uniques { get; set; }
    /// <summary>
    /// 唯一值的字段的值是否允许为空
    /// </summary>
    public string UniqueAllowEmptys { get; set; }
    /// <summary>
    /// 所属的父级配置, 在 xml 中不用指定，程序会自动分析
    /// </summary>
    public SqlConfig Parent { get; set; }
    /// <summary>
    /// 包含的子级配置, 即子表的配置，需要在 xml 中配置
    /// </summary>
    public List<SqlConfig> Children { get; set; }
    /// <summary>
    /// 依赖父表的字段
    /// </summary>
    public Dependency Dependency { get; set; }
    /// <summary>
    /// insert 的配置
    /// </summary>
    public Insert Insert { get; set; }
    /// <summary>
    /// update 的配置
    /// </summary>
    public Update Update { get; set; }
    /// <summary>
    /// delete 的配置
    /// </summary>
    public Delete Delete { get; set; }
    #endregion
    /// <summary>
    /// 单条记录查询的配置，一般用在配置列表双击弹出那条记录的获取的 sql 
    /// </summary>
    public SingleQuery SingleQuery { get; set; }
    /// <summary>
    /// 导出配置
    /// </summary>
    public Export Export { get; set; }
    /// <summary>
    /// 导入配置
    /// </summary>
    public Import Import { get; set; }
    /// <summary>
    /// 是否物理删除?
    /// </summary>
    public bool DeleteAnyway { get; set; }
    /// <summary>
    /// 表单编码的生成配置
    /// </summary>
    public BillCodeRule BillCodeRule { get; set; }


}
```
####用法
1. xml 配置
```csharp
string sqlXml = @"
<SqlConfig>
    <Select>
        user.*
    </Select>
    <From>
        BasUser user
    </From>
    <Where>
    	<Fields>
    		<Field Name=""Name"" Cp=""like""></Field>
    	</Fields>
    </Where>
    <OrderBy>
    	<Default>Id</Default>
    </OrderBy>
    <IDs>Id</IDs>
    <PKs>Id</PKs>
    <Table>BasUser</Table>
    <Insert>
        <Fields>
            <Field Name=""Name"" IsIgnore=""false""></Field>
        </Fields>
    </Insert>
</SqlConfig>
            ";
```

 <!-- continue counter -->
2. json 配置 
```csharp
 string sqlJson = @"
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
    ""PKs"":""Id"",
    ""Table"":""BasUser"",
    ""Insert"":{
        ""Fields"":[
            {""Name"":""Name"",""IsIgnore"":""false""}
            ]
        }
    }
";
var sqlconfig = JsonConvert.DeserializeObject<SqlConfig>(sqlJson);
```



