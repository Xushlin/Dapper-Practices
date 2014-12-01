
using DapperSimple.Data;
using DapperSimple.Data.Migrations;
using DapperSimple.Data.Models;
using DapperSimple.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSimple.App
{
    class Program
    {
        static void Main(string[] args)
        {
            SetUpMigration();

            CreateCategory();
            CreateProduct();
            QueryCateGoryWitchProducts();
            QueryProductsWithCategory();

            //CategoryRepository categoryRepository = new CategoryRepository();
            //categoryRepository.AddCategory(new Category
            //{
            //    Name = "test...",
            //    ParentId = 2,
            //});
            //Console.WriteLine("Query complete...");
            //Console.ReadKey();

                          
        }

        static void QueryCateGoryWitchProducts()
        {
            CategoryRepository categoryRepository = new CategoryRepository();
            var categories = categoryRepository.QueryCategories();
            //foreach (var category in categories)
            //{
            //    Console.WriteLine("product,Id:{0}, Name:{1},Price:{2}, Category name: {3},Category Id:{4},Category parentId:{5}", product.Id, product.Name, product.Price, product.Catagory.Name, product.Catagory.Id, product.Catagory.ParentId);
            //}
            Console.WriteLine("Query complete...");
            Console.ReadKey();

        }
        static void QueryProductsWithCategory()
        {
            ProductRepository productRepository = new ProductRepository();
            var products = productRepository.QueryProduct();
            foreach (var product in products)
            {
                Console.WriteLine("product,Id:{0}, Name:{1},Price:{2}, Category name: {3},Category Id:{4},Category parentId:{5}", product.Id, product.Name, product.Price, product.Catagory.Name, product.Catagory.Id, product.Catagory.ParentId);
            }
            Console.WriteLine("Query complete...");
            Console.ReadKey();

            
        }

        static void CreateProduct()
        {
            ProductRepository productRepository = new ProductRepository();
            List<Category> categories = new List<Category>();
            for (int i = 0; i < 100; i++)
            {
                Category category1 = new Category
                {
                    Id = i + 1,
                };

                Product product = new Product();
                product.Name = Faker.NameFaker.Name();
                product.Price = Faker.NumberFaker.Number(30, 5000);
                product.Catagory = category1;
                productRepository.Insert(product);
                // categories.Add(category1);
                //categoryRepository.Insert(category1);
            }
            //categoryRepository.InsertMultiCategories(categories);
            Console.WriteLine("insert complete...");
            Console.ReadKey();
     
        }
        static void CreateCategory()
        {
            CategoryRepository categoryRepository = new CategoryRepository();
            List<Category> categories = new List<Category>();
            for (int i = 0; i < 10000; i++)
            {
                Category category1 = new Category
                {
                    Name = Faker.StringFaker.Alpha(60),
                    ParentId = 200
                };
                categories.Add(category1);
                //categoryRepository.Insert(category1);
            }
            categoryRepository.InsertMultiCategories(categories);
            Console.WriteLine("insert complete...");
            Console.ReadKey();
        }

        public static void SetUpMigration(){
            ApplicationConfig.RegisterApplication(ApplicationInfo.Instance, VersionTable.Versions,typeof(Program).GetType().BaseType.Assembly);
            var primaryConnectionStringKey = DataConfig.GetPrimaryConnectionStringKey();
            DataConfig.EnsureDatabaseIsAvailable(connectionStringKey: primaryConnectionStringKey,
                                                 applicationName: typeof(Program).Assembly.GetName().Name,
                                                 createLocalDbIfMissing: true,
                                                 migrationsAssembly:
                                                     MigrationsAssembly.FromClass
                                                     <_2014112901_Init>());

        }
    }
}
