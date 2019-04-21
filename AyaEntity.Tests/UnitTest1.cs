using AyaEntity.Base;
using AyaEntity.DataUtils;
using AyaEntity.SqlServices;
using AyaEntity.Statement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AyaEntity.Tests
{
  [TestClass]
  public class UnitTest1
  {
    const string connectionString ="";
    private SqlManager manage;


    public UnitTest1()
    {
      this.manage = new SqlManager(connectionString);

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

    [TestMethod]
    public void TestMethod1()
    {

      // ʹ��Ĭ�ϵ�sql������������򵥻�ȡһ��modelʵ��
      // �������������
      // select * from blog_article where name = @name
      Article m = this.manage
                      // ʹ��Ĭ��sql������������ΪĬ��,ps:�ɲ���)
                      //.UseService(opt=>opt.CurrentServiceKey="default")
                      .Get<Article>(new { name = "123", });



      // �������ù��ˣ���ǰservice sql������ΪArticleService��ֱ�ӵ��ú�����Ч��һ��
      this.manage.Get<Article>();


      // �л���default service sql��������
      this.manage.UseService(opt => opt.CurrentServiceKey = "default")
                  .Get<Article>();


      // ���л���articleservice 
      this.manage.UseService(opt => opt.CurrentServiceKey = "ArticleService")
                  .Get<Article>();

      // ִ��update���������ۣ�����������������+1��
      this.manage.UseService(opt => opt.ServiceMethod = "NewCommit")
                  .Update<Article>(new { articleId = "123" });

      // ִ��update����������ͷ����Ϣ
      this.manage.UseService(opt => opt.ServiceMethod = "UpdateHead")
                   .Update<Article>(new Article { Name = "��Ҫ������", Title = "��Ҫ�ı���", Id = "����������where��" });

      Assert.IsFalse(result, "1 should not be prime");
    }


  }




}
