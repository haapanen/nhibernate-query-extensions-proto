using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using log4net;
using NHibernate.CustomQueries.Data;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Transform;

namespace NHibernate.CustomQueries
{
    class Program
    {
        private static ISessionFactory _sessionFactory;

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            _sessionFactory = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2012.ConnectionString(c => c.FromConnectionStringWithKey("ConnectionString")))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<ItemMap>())
                .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(false, true))
                .BuildSessionFactory();

            try
            {
                using (var session = _sessionFactory.OpenSession())
                using (var tx = session.BeginTransaction())
                {
                    //session.CreateTemporaryIdTable("#ItemIds", new int[]{1});
                    //session.CreateSQLQuery("CREATE TABLE #ItemIds (Id INT NOT NULL);").ExecuteUpdate();
                    //foreach (var item in Enumerable.Range(1, 30000))
                    //{
                    //    var query = session.CreateSQLQuery("INSERT INTO #ItemIds (Id) VALUES (:Id);").SetParameter("Id", item).ExecuteUpdate();
                    //}

                    //var count = session.CreateSQLQuery("SELECT COUNT(*) FROM #ItemIds;");
                    var result = session.CreateTemporaryIdTable("ItemIds", Enumerable.Range(1, 30000).Select(v => v.ToString()).ToArray())
                        .QueryOver<Item>().WhereIn(i => i.Id, "ItemIds").TransformUsing(Transformers.DistinctRootEntity).List();

                    // InitializeData(session, tx);
                    //var result = session.QueryOver<Item>().WhereIn(i => i.Id, "#ItemIds", "Id");
                    //var stuff = result.TransformUsing(Transformers.RootEntity).List();
                    //tx.Commit();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            
        }

        private static void InitializeData(ISession session, ITransaction tx)
        {
            foreach (var current in session.Query<Item>())
            {
                session.Delete(current);
            }

            for (var itemIndex = 1; itemIndex < 20; ++itemIndex)
            {
                var item = new Item
                {
                    Name = $"Item {itemIndex}",
                    Operations = new List<ItemOperation>
                    {
                        new ItemOperation
                        {
                            OperationPeriod = 3,
                            OperationPeriodType = "Month",
                            Entries = Enumerable.Range(1, 10).Select(i => new ItemOperationEntry
                            {
                                Comment = $"Entry for index {i}",
                                CreatedAt = DateTime.UtcNow.AddDays(new Random().Next(-300, 0))
                            }).ToList()
                        },
                        new ItemOperation
                        {
                            OperationPeriod = 1,
                            OperationPeriodType = "Year",
                            Entries = Enumerable.Range(1, 10).Select(i => new ItemOperationEntry
                            {
                                Comment = $"Entry for index {i}",
                                CreatedAt = DateTime.UtcNow.AddDays(new Random().Next(-900, 0))
                            }).ToList()
                        }
                    }
                };

                session.Save(item);
            }

            session.Flush();
            tx.Commit();
        }
    }
}
