using AyaEntity.Base;
using AyaEntity.Command;
using AyaEntity.DataUtils;
using AyaEntity.Services;
using AyaEntity.Statement;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AyaEntity.Tests
{
  [TableName("blog_article")]
  public class Article
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

  [TableName("blog_article")]
  class Article_NotSelect
  {

    [PrimaryKey]
    [IdentityKey]
    [ColumnName("id")]
    public int Id { get; set; }

    [NotSelect]
    [ColumnName("article_name")]
    public string Name { get; set; }

    [ColumnName("article_title")]
    public string Title { get; set; }
    [ColumnName("state")]
    public byte State { get; set; }
  }
  /// <summary>
  /// �������ע�ͣ������Լ��Ĳ������ݿ�
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
    private ArticleDBService articleService;

    public BaseServiceTest()
    {

      // ʹ�������ַ��� ��ʼ��manage  
      manage = new SqlManager($"Server={Config.server};Database={Config.dbName}; User={Config.username};Password={Config.pwd};charset=UTF8");
      // ʹ��Ĭ��db service
      DBService dbService = this.manage.UseService<DBService>();
      Article article = dbService.GetEntity<Article>(new Article { Id = 3 });

      // ����Զ���sql service ��ʹ��
      this.manage.AddService<ArticleDBService>();
      this.articleService = this.manage.UseService<ArticleDBService>();
      Article max = articleService.GetMaxIdArticle();

    }


    [TestMethod]
    public void GetLastInsertId()
    {
      // ����ʵ������
      int id = articleService.Insert(new Article { Name = "test insert", Title = "���Բ�������" }, true);
      Assert.IsTrue(id > 1, "���뵥�����ݴ���v:" + id);
    }

    /// <summary>
    /// ����not select ����
    /// </summary>
    [TestMethod]
    public void TestNotSelect()
    {
      IEnumerable<Article_NotSelect> list = articleService.GetEntityList<Article_NotSelect>();
      Assert.IsTrue(list.All(m => m.Name == null));
    }



    /// <summary>
    /// ���Ի�ȡentity
    /// </summary>
    [TestMethod]
    public void TestGet()
    {
      //����ʵ����������������������Զ�����
      Article article = articleService.GetEntity<Article>(new Article { Name = "3 insert list 3" }
      );
      Assert.IsTrue(article.Name.Equals("3 insert list 3"), "1");


      // ���Է�����ʵ���������
      article = articleService.GetEntity<Article>(new { article_name = "3 insert list 3" });
      Assert.IsTrue(article.Name.Equals("3 insert list 3"), "2");

    }


    /// <summary>
    /// ���Ի�ȡlist entity
    /// </summary>
    [TestMethod]
    public void TestGetList()
    {
      // ��������ȡ����
      IEnumerable<Article> list = articleService.GetEntityList<Article>();
      Assert.IsTrue(list.Count() > 0, "��ѯ�б�ʧ��");

      // ������
      list = articleService.GetEntityList<Article>();
      Assert.IsTrue(list.Count() > 0, "�б��ѯ�쳣��������");
      // ���Զ�������
      list = articleService.GetEntityList<Article>(new { id = "%9%" }, "id like @id");
      Assert.IsTrue(list.Count() > 0 && list.All(m => m.Id.ToString().Contains('9')), "%3%:��ѯ�б�ʧ��");

    }



    /// <summary>
    /// ���Բ�������
    /// </summary>
    [TestMethod]
    public void TestInsert()
    {
      // ����ʵ������
      int row = articleService.Insert(new Article { Name = "test insert", Title = "���Բ�������" });
      Assert.IsTrue(row == 1, "���뵥�����ݴ���Ӱ������:" + row);

      // �������ʵ������
      List<Article> list = new List<Article>();
      for (int i = 0; i < 10; i++)
      {
        list.Add(new Article { Name = i + " insert list " + i, Title = "���Բ������� " + i });
      }

      row = articleService.InsertList(list);
      Assert.IsTrue(row == list.Count, "����������ݴ���Ӱ������:" + row);
    }

    //[TestMethod]
    //public void TestDeleteAll()
    //{
    //  int row = articleService.Delete<Article>(new Article { Id = 0 }, "1=1");
    //  Assert.IsTrue(row > 0);
    //}


    [TestMethod]
    public void TestDelete()
    {

      //BaseStatementService s = new BaseStatementService();
      //string str= s.GetExcuteSQLString("Delete", typeof(Article), new {id = 0 }).ToSql();
      //Assert.IsFalse(true, str);

      // �������� �ѹ�ɾ��
      articleService.Insert(new Article { Name = "temp delete ", Title = "���Բ�������" });
      int row = articleService.Delete<Article>(new Article { Name = "temp delete" });
      Assert.IsTrue(row == 1, "ɾ������ʧ�ܣ�Ӱ������:" + row);


      try
      {
        // ���Կ�where�������׳��쳣������Я��where������Ϊ�����ݰ�ȫ
        articleService.Delete<Article>(null);
      }
      catch (Exception ex)
      {
        Assert.IsTrue(ex.Message.Contains("1=1"), "����ȫ��ɾ��");

        // ����ɾ�����У��ֶ����ϲ�������һ��ȷ��ɾ�����еĹ���
        IEnumerable<Article> list = articleService.GetEntityList<Article>();
        row = articleService.Delete<Article>("1=1");
        Assert.IsTrue(list.Count() == row, "δ��ɾ����������,ԭ����������:" + list.Count() + ",Ӱ������:" + row);
        // ɾ�����ٰ����ݼӻ�ȥ
        row = articleService.InsertList(list);
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
      int maxid = articleService.GetMaxId();
      // �����Զ������
      int row = articleService.Delete<Article>(new Article { Id = maxid - 2 }, "id > @Id");
      Assert.IsTrue(row > 1 && row < 3, "�Զ������ɾ���쳣��Ӱ������:" + row);
    }



    [TestMethod]
    public void TestGetMaxId()
    {
      int maxid = articleService.GetMaxId();
      Assert.IsTrue(maxid > 0, "��ȡmaxid�쳣��maxid:" + maxid);
    }


    /// <summary>
    /// �����Զ���sql���
    /// </summary>
    [TestMethod]
    public void TestExcuteCustom()
    {

      // ʹ���Զ���sql����ѯ���id

      int maxid = articleService.GetMaxId();
      Article a = articleService.GetEntity<Article>(new Article { Id = maxid });
      // ִ���Զ���sql��ģ��ƥ�䣬��ȡһ��ʵ��
      Article artile = articleService.LikeArticleName(a.Name);

      Assert.IsTrue(artile.Name.Equals(a.Name), "ģ����ѯƥ��likemeʧ��");

    }

    /// <summary>
    /// �����Զ���SQL����ȡ�ֵ����
    /// </summary>
    [TestMethod]
    public void TestExcuteCustomToDict()
    {
      // ִ���Զ���sql�� �����ѯ ��ȡ�ֵ�
      Dictionary<string, int> d = articleService.GetArticleDictionaryCounts();
      Assert.IsTrue(d.Count > 0);
    }

    /// <summary>
    /// ���Ը���
    /// </summary>
    [TestMethod]
    public void TestUpdate()
    {
      Article max = articleService.GetMaxIdArticle();
      // Ĭ�ϰ�������id��������
      string name = "update max name";
      int row = articleService.Update<Article>(new Article { Name = name, Id = max.Id });
      Article a = articleService.GetEntity<Article>(new { id = max.Id });
      Assert.AreEqual(a.Name, name);
    }


    /// <summary>
    /// ���Ը����Զ������
    /// </summary>
    [TestMethod]
    public void TestUpdateCustom()
    {
      Article max = articleService.GetMaxIdArticle();
      // Ĭ�ϰ�������id�������� 
      string title = "update title";
      int row = articleService.Update<Article>(new { Name = max.Name, Title = title, Id = max.Id }, "article_name=@Name AND id=@Id", "article_title=@Title");

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
        Article max = articleService.GetMaxIdArticle();
        int row = articleService.Update<Article>(new Article { Name = "123", Id = 0 });
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

}




