using AyaEntity.Base;
using AyaEntity.DataUtils;
using AyaEntity.SqlServices;
using AyaEntity.Statement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AyaEntity.Tests
{
  [TableName("blog_article")]
  class Article
  {

    [PrimaryKey]
    [IdentityKey]
    [ColumnName("id")]
    public int Id { get; set; }

    [ColumnName("article_name")]
    public string Name { get; set; }

    [ColumnName("article_title")]
    public string Title { get; set; }
    [ColumnName("state")]
    public byte State { get; set; }
  }


  class blog_article
  {
    public int id { get; set; }
    public string article_name { get; set; }
    public string article_title { get; set; }
    public byte state { get; set; }
  }

  /// <summary>
  /// �������ע�ͣ������Լ������ݿ�
  /// </summary>
  //public class Config
  //{
  //  public const string server="��������ַ";
  //  public const string username="�û���";
  //  public const string pwd ="����";
  //  public const string dbName ="���ݿ���";
  //}

  [TestClass]
  public class BaseServiceTest
  {

    private SqlManager manage;

    public BaseServiceTest()
    {

      // ��ʼ��manage
      this.manage = new SqlManager($"Server={Config.server};Database={Config.dbName}; User={Config.username};Password={Config.pwd};charset=UTF8");

      // ����Զ���sql service
      ArticleSqlService service = new ArticleSqlService();
      this.manage.AddService("ArticleService", service);


    }

    /// <summary>
    /// ���Ի�ȡentity
    /// </summary>
    [TestMethod]
    public void TestGet()
    {
      //����ʵ����������������������Զ�����
      Article article = this.manage.GetEntity<Article>(new Article { Name = "3 insert list 3" }
      );
      Assert.IsTrue(article.Name.Equals("3 insert list 3"), "1");


      // ���Է�����ʵ���������
      article = this.manage.GetEntity<Article>(new { article_name = "3 insert list 3" });
      Assert.IsTrue(article.Name.Equals("3 insert list 3"), "2");

    }



    /// <summary>
    /// ���Ի�ȡlist entity
    /// </summary>
    [TestMethod]
    public void TestGetList()
    {
      // ��������ȡ����
      IEnumerable<Article> list = this.manage.GetEntityList<Article>();
      Assert.IsTrue(list.Count() > 0, "��ѯ�б�ʧ��");

      // ������
      list = this.manage.GetEntityList<Article>();
      Assert.IsTrue(list.Count() > 0, "�б��ѯ�쳣��������");
      // ���Զ�������
      list = this.manage.GetEntityList<Article>(new { id = "%9%" }, "id like @id");
      Assert.IsTrue(list.Count() > 0 && list.All(m => m.Id.ToString().Contains('9')), "%3%:��ѯ�б�ʧ��");

    }



    /// <summary>
    /// ���Բ�������
    /// </summary>
    [TestMethod]
    public void TestInsert()
    {
      // ����ʵ������
      int row = this.manage.Insert(new Article { Name = "test insert", Title = "���Բ�������" });
      Assert.IsTrue(row == 1, "���뵥�����ݴ���Ӱ������:" + row);

      // �������ʵ������
      List<Article> list = new List<Article>();
      for (int i = 0; i < 10; i++)
      {
        list.Add(new Article { Name = i + " insert list " + i, Title = "���Բ������� " + i });
      }

      row = this.manage.InsertList(list);
      Assert.IsTrue(row == list.Count, "����������ݴ���Ӱ������:" + row);
    }

    //[TestMethod]
    //public void TestDeleteAll()
    //{
    //  int row = this.manage.Delete<Article>(new Article { Id = 0 }, "1=1");
    //  Assert.IsTrue(row > 0);
    //}


    [TestMethod]
    public void TestDelete()
    {

      //BaseStatementService s = new BaseStatementService();
      //string str= s.GetExcuteSQLString("Delete", typeof(Article), new {id = 0 }).ToSql();
      //Assert.IsFalse(true, str);

      // �������� �ѹ�ɾ��
      this.manage.Insert(new Article { Name = "temp delete ", Title = "���Բ�������" });
      int row = this.manage.Delete<Article>(new Article { Name = "temp delete" });
      Assert.IsTrue(row == 1, "ɾ������ʧ�ܣ�Ӱ������:" + row);


      try
      {
        // ���Կ�where�������׳��쳣������Я��where������Ϊ�����ݰ�ȫ
        this.manage.Delete<Article>(new Article { Id = 0 });
      }
      catch (Exception ex)
      {
        Assert.IsTrue(ex.Message.Contains("1=1"), "����ȫ��ɾ��");

        // ����ɾ�����У��ֶ����ϲ�������һ��ȷ��ɾ�����еĹ���
        IEnumerable<Article> list= this.manage.GetEntityList<Article>();
        row = this.manage.Delete<Article>(new Article { Id = 0 }, "1=1");
        Assert.IsTrue(list.Count() == row, "δ��ɾ����������,ԭ����������:" + list.Count() + ",Ӱ������:" + row);
        // ɾ�����ٰ����ݼӻ�ȥ
        row = this.manage.InsertList(list);
        Assert.IsTrue(row == list.Count(), "����������ݴ���Ӱ������:" + row);
      }
    }


    /// <summary>
    /// ����ɾ��2��maxid
    /// </summary>
    [TestMethod]
    public void TestDeleteMaxId()
    {
      // ʹ���Զ���sql����ѯ���id
      int maxid = this.manage.UseService("ArticleService","GetMaxId").Get<int,Article>();
      // �����Զ������
      int row = this.manage.UseServiceDefault().Delete<Article>(new Article { Id = maxid - 2 }, "id > @Id");
      Assert.IsTrue(row > 1 && row < 3, "�Զ������ɾ���쳣��Ӱ������:" + row);
    }



    [TestMethod]
    public void TestGetMaxId()
    {
      // ʹ���Զ���sql����ѯ���id
      this.manage.UseService(option =>
      {
        option.CurrentServiceKey = "ArticleService";
        option.ServiceMethod = "GetMaxId";
      });

      int maxid = this.manage.Get<int,Article>();
      Assert.IsTrue(maxid > 0, "��ȡmaxid�쳣��maxid:" + maxid);
      this.manage.UseServiceDefault();
    }




    /// <summary>
    /// �����Զ���sql���
    /// </summary>
    [TestMethod]
    public void TestExcuteCustom()
    {

      // ʹ���Զ���sql����ѯ���id
      int maxid = this.manage.UseService("ArticleService","GetMaxId").Get<int,Article>();
      Article a = this.manage.UseServiceDefault().GetEntity<Article>(new Article { Id = maxid });
      // ִ���Զ���sql��ģ��ƥ�䣬��ȡһ��ʵ��
      Article artile = this.manage.ExcuteCustomGet<Article>(
        new MysqlSelectStatement()
            .Select(SqlAttribute.GetColumns(typeof(Article)))
            .Where(new { name = a.Name.Substring(0,2)+"%" }, "article_name like @name")
            .From(SqlAttribute.GetTableName(typeof(Article)))
        );
      Assert.IsTrue(artile.Name.Equals(a.Name), "ģ����ѯƥ��likemeʧ��");

    }

    /// <summary>
    /// �����Զ���SQL����ȡ�ֵ����
    /// </summary>
    [TestMethod]
    public void TestExcuteCustomToDict()
    {

      // ִ���Զ���sql�� �����ѯ ��ȡ�ֵ�
      Dictionary<string,string> d = this.manage.ExcuteCustomGetList<KeyValuePair<string,string>>(
        new MysqlSelectStatement()
            .Select("count(*) as `Value`","article_name as `Key`")
            .Group("article_name")
            .From(SqlAttribute.GetTableName(typeof(Article)))
       ).ToDictionary(m=>m.Key,m=>m.Value);
      Assert.IsTrue(d.Count > 0);
    }

    /// <summary>
    /// ���Ը���
    /// </summary>
    [TestMethod]
    public void TestUpdate()
    {
      Article max = this.manage.UseService("ArticleService","GetMaxIdEntity").GetEntity<Article>();
      // Ĭ�ϰ�������id��������
      string name = "update max name";
      int row = this.manage.UseServiceDefault().Update<Article>(new Article { Name = name ,Id = max.Id });
      Article a= this.manage.GetEntity<Article>(new { id = max.Id });
      Assert.AreEqual(a.Name, name);

    }


    /// <summary>
    /// ���Ը����Զ������
    /// </summary>
    [TestMethod]
    public void TestUpdateCustom()
    {
      Article max = this.manage.UseService("ArticleService","GetMaxIdEntity").GetEntity<Article>();
      // Ĭ�ϰ�������id��������
      string title = "update title";
      int row = this.manage.UseServiceDefault().Update<Article>(new { Name = max.Name ,Title=title,Id=max.Id },"article_name=@Name AND id=@Id","article_title=@Title");

      Assert.AreEqual(row, 1);

    }


    /// <summary>
    /// ���Ը���,����Ϊ0������Ĭ��ֵ�ų�����
    /// </summary>
    [TestMethod]
    public void TestUpdateDefalutId()
    {
      try
      {

        // Ĭ�ϰ�������id��������
        Article max = this.manage.UseService("ArticleService", "GetMaxIdEntity").GetEntity<Article>();
        int row = this.manage.UseServiceDefault().Update<Article>(new Article { Name = "123" ,Id =0 });

      }
      catch (Exception ex)
      {
        if (!ex.Message.Contains("1=1"))
        {
          Assert.Fail();
        }
      }

    }

  }



  /// <summary>
  /// Demo������ service�Զ�����չ
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
        // ģ�����ֲ�ѯ
        case "Get:LikeName":
          return this.selectSql
                  .Limit(1)
                  .Where("article_name like @Name")
                  .From(SqlAttribute.GetTableName(this.entityType));
        // ��ȡ��ǰ��������id
        case "Get:GetMaxId":
          return this.selectSql
                  .Select("Max(id)")
                  .Limit(1)
                  .Where(conditionParameters)
                  .From(SqlAttribute.GetTableName(this.entityType));
        // ��ȡ��ǰ��������id ��entity
        case "GetEntity:GetMaxIdEntity":
          string tn = SqlAttribute.GetTableName(this.entityType);
          return this.selectSql
                  .Select(SqlAttribute.GetColumns(this.entityType))
                  .From(tn)
                  .Where("Id=("
                  + new MysqlSelectStatement()
                        .Select("Max(id) as Id")
                        .Limit(1)
                        .Where(conditionParameters)
                        .From(tn).ToSql()
                  + ")");
        // state ������һ
        case "Update:StateAdd":
          return this.updateSql
                  .Set("state+=1")
                  .Where(conditionParameters)
                  .From(SqlAttribute.GetPrimaryColumn(this.entityType));
        // ������������������ͷ����Ϣ���������֣����±��⣩
        //case "Update:UpdateHead":
        //  return this.updateSql.Set("article_name=@Name", "article_title=@Title")
        //                        .Where(SqlAttribute.GetPrimaryColumn(this.entityType));
        default:
          throw new NotImplementedException("δʵ�ַ�����" + flag);
      }
    }




  }
}




