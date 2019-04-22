using AyaEntity.Base;
using AyaEntity.DataUtils;
using AyaEntity.SqlServices;
using AyaEntity.Statement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AyaEntity.Tests
{
  [TableName("blog_article")]
  class Article
  {

    [PrimaryKey]
    [ColumnName("id")]
    public int Id { get; set; }

    [ColumnName("article_name")]
    public string Name { get; set; }

    [ColumnName("article_title")]
    public string Title { get; set; }
  }


  class blog_article
  {
    public int id { get; set; }
    public string article_name { get; set; }
    public string article_title { get; set; }
  }



  [TestClass]
  public class BaseServiceTest
  {

    private SqlManager manage;

    public BaseServiceTest()
    {

      this.manage = new SqlManager($"Server={Config.server};Database={Config.dbName}; User={Config.username};Password={Config.pwd};charset=UTF8");

      //string m = "2";
      //// �Զ���sql�����������ģ��ƥ���ȡһ��ʵ��
      //ArticleSqlService service = new ArticleSqlService();
      //// �����������
      //// select top 1 * from tableName where name like '%' +@name +'%';
      //Article m2 = this.manage
      //                // ����ʹ���Զ����sql���������Զ���ҵ��
      //                .AddService("ArticleService",service)
      //                .UseService(option =>
      //                {
      //                  option.CurrentServiceKey = "ArticleService";
      //                  option.ServiceMethod= "LikeName";
      //                })
      //                .Get<Article>(new {name = "321" });
    }

    /// <summary>
    /// ���Ի�ȡentity
    /// </summary>
    [TestMethod]
    public void TestGet()
    {
      //����ʵ����
      Article artile = this.manage.Get<Article>(new Article { Name = "123" }
      );
      Assert.IsTrue(artile.Name.Equals("123"), "Article:��ѯ����name��Ϊ123");


      // ���Է�����ʵ����
      Article b_article = this.manage.Get<Article>(
        new  { article_name = "321" }
      );
      Assert.IsTrue(b_article.Name.Equals("321"), "blog_article:��ѯ����name��Ϊ321");


      // ���Զ�̬����
      artile = this.manage.Get<Article>(new { article_name = "321" });
      Assert.IsTrue(artile.Name.Equals("321"), "dynamic param: ��ѯ����name��Ϊ321");

    }



    /// <summary>
    /// ���Ի�ȡlist entity
    /// </summary>
    [TestMethod]
    public void TestGetList()
    {
      // ��������ȡ����
      IEnumerable<Article> list = this.manage.GetList<Article>();
      Assert.IsTrue(list.Count() > 0, "��ѯ�б�ʧ��");

      // ������
      list = this.manage.GetList<Article>(new Article { Name = "123" });
      Assert.IsTrue(list.Count() > 0 && list.All(m => m.Name.Equals("123")), "123:��ѯ�б�ʧ��");

      // ���Զ�������
      list = this.manage.GetList<Article>(new Article { Name = "%3%" }, "article_name like @Name");
      Assert.IsTrue(list.Count() > 0 && list.All(m => m.Name.Contains("3")), "%3%:��ѯ�б�ʧ��");



    }



    /// <summary>
    /// �����Զ���sql���
    /// </summary>
    [TestMethod]
    public void TestExcuteCustom()
    {
      // ִ���Զ���sql��ģ��ƥ�䣬��ȡһ��ʵ��
      Article artile = this.manage.ExcuteCustomGet<Article>(
        new MysqlSelectStatement()
            .Select(SqlAttribute.GetColumns(typeof(Article)))
            .Where(new { name = "%me" }, "article_name like @name")
            .From(SqlAttribute.GetTableName(typeof(Article)))
        );
      Assert.IsTrue(artile.Name.Equals("likeme"), "ģ����ѯƥ��likemeʧ��");

      // ִ���Զ���sql�� �����ѯ ��ȡ�ֵ�
      Dictionary<string,string> d = this.manage.ExcuteCustomGetList<KeyValuePair<string,string>>(
        new MysqlSelectStatement()
            .Select("count(*) as `Value`","article_name as `Key`")
            .Group("article_name")
            .From(SqlAttribute.GetTableName(typeof(Article)))
       ).ToDictionary(m=>m.Key,m=>m.Value);
      Assert.IsTrue(d.Count > 0 && d["kaakira"].Equals("2"), "�ֵ䲻ƥ��");
    }



    private string GetSqlString()
    {
      StatementService service = new BaseStatementService();
      ISqlStatementToSql sql = service.GetExcuteSQLString("Get", typeof(Article),
               new Article { Name = "123" }
             );

      return sql.ToSql();
    }








    //// ʹ��Ĭ�ϵ�sql������������򵥻�ȡһ��modelʵ��
    //// �������������
    //// select * from blog_article where name = @name
    //Article m = this.manage
    //                // ʹ��Ĭ��sql������������ΪĬ��,ps:�ɲ���)
    //                //.UseService(opt=>opt.CurrentServiceKey="default")
    //                .Get<Article>(new { name = "123", });



    //// �������ù��ˣ���ǰservice sql������ΪArticleService��ֱ�ӵ��ú�����Ч��һ��
    //this.manage.Get<Article>();


    //// �л���default service sql��������
    //this.manage.UseService(opt => opt.CurrentServiceKey = "default")
    //            .Get<Article>();


    //// ���л���articleservice 
    //this.manage.UseService(opt => opt.CurrentServiceKey = "ArticleService")
    //            .Get<Article>();

    //// ִ��update���������ۣ�����������������+1��
    //this.manage.UseService(opt => opt.ServiceMethod = "NewCommit")
    //            .Update<Article>(new { articleId = "123" });

    //// ִ��update����������ͷ����Ϣ
    //this.manage.UseService(opt => opt.ServiceMethod = "UpdateHead")
    //             .Update<Article>(new Article { Name = "��Ҫ������", Title = "��Ҫ�ı���", Id = "����������where��" });

  }


  /// <summary>
  /// Demo��service�Զ�����չ
  /// </summary>
  public class ArticleSqlService : StatementService
  {


    /// <summary>
    /// �Ż���ֻ����һ��
    /// </summary>
    private MysqlSelectStatement selectSql = new MysqlSelectStatement();
    private UpdateStatement updateSql = new UpdateStatement();
    private DeleteStatement deleteSql;
    private InsertStatement insertSql;


    protected override SqlStatement CreateSql(string funcName, object conditionParameters)
    {
      string flag = funcName + ":" + this.methodName;
      switch (flag)
      {
        case "Get:LikeName":
          return this.LikeName();
        case "Update:NewCommit":
          return this.NewCommit(conditionParameters);
        // ������������������ͷ����Ϣ���������֣����±��⣩
        case "Update:UpdateHead":
          return this.updateSql.Update(conditionParameters)
                                .Set("article_name=@Name", "article_title=@Title")
                                .Where(SqlAttribute.GetPrimaryColumn(this.entityType));
        default:
          throw new NotImplementedException("δʵ�ַ�����" + flag);
      }
    }

    /// <summary>
    /// �������������ۣ�����������+1
    /// </summary>
    /// <returns></returns>
    private SqlStatement NewCommit(object param)
    {
      return this.updateSql.Update(param).Set("commit_count+=1").Where("article_id=@articleId");
    }


    /// <summary>
    /// ģ�����ֲ�ѯ
    /// </summary>
    /// <returns></returns>
    private SqlStatement LikeName()
    {
      return this.selectSql
                  .Select("top 1 *")
                  .From(SqlAttribute.GetTableName(this.entityType));
    }


  }
}




