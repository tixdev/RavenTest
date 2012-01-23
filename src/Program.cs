using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Linq;
using Raven.Database.Server;

namespace RavenDB
{
    class Program
    {
        static void Main(string[] args)
        {
            var store = SetupDocumentStore();

            using(var session = store.OpenSession())
            {
                for (int i = 0; i < 100; i++)
                {
                    var product = new Product
                                      {
                                          Cost = 3.99m,
                                          Name = "Milk",
                                          Quantity = i
                                      };
                    session.Store(product);
                }

                session.SaveChanges();

                RavenQueryStatistics statistics;
                var products =
                    session.Query<Product>().Customize(p => p.WaitForNonStaleResultsAsOfLastWrite()).Statistics(
                        out statistics).Take(0);

                Console.WriteLine("Product count: {0}", products.Count());
            }
            
            Console.Read();
        }

        private static IDocumentStore SetupDocumentStore()
        {
            var document = new EmbeddableDocumentStore
                               {
                                   DataDirectory = "Data",
                                   UseEmbeddedHttpServer = true
                               };

            NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(8080);

            var store = document.Initialize();
            return store;
        }
    }

    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Cost { get; set; }
        public int Quantity { get; set; }
    }

    public class Order
    {
        public string Id { get; set; }
        public string Customer { get; set; }
        public IList<OrderLine> OrderLines { get; set; }

        public Order()
        {
            OrderLines = new List<OrderLine>();
        }
    }

    public class OrderLine
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
