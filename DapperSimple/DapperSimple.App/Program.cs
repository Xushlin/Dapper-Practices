
using DapperSimple.Data;
using DapperSimple.Data.Migrations;
using DapperSimple.Data.Models;
using DapperSimple.Data.Repositories;
using System;
using System.Collections.Generic;

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
            //Console.WriteLine("SqlMapperExtensions test complete...");
            //Console.ReadKey();
        }
        static void QueryCateGoryWitchProducts()
        {
            var categoryRepository = new CategoryRepository();
            var categories = categoryRepository.QueryCategories();
            //TODO:这里可以打印出来看看
            //foreach (var category in categories)
            //{
            //    Console.WriteLine("product,Id:{0}, Name:{1},Price:{2}, Category name: {3},Category Id:{4},Category parentId:{5}", product.Id, product.Name, product.Price, product.Catagory.Name, product.Catagory.Id, product.Catagory.ParentId);
            //}
            Console.WriteLine("Query CateGory Witch Products completed..");
            Console.ReadKey();
        }
        static void QueryProductsWithCategory()
        {
            var productRepository = new ProductRepository();
            var products = productRepository.QueryProduct();
            foreach (var product in products)
            {
                Console.WriteLine("product,Id:{0}, Name:{1},Price:{2}, Category name: {3},Category Id:{4},Category parentId:{5}", product.Id, product.Name, product.Price, product.Catagory.Name, product.Catagory.Id, product.Catagory.ParentId);
            }
            Console.WriteLine("Query Products With Category completed...");
            Console.ReadKey();


        }
        static void CreateProduct()
        {
            var productRepository = new ProductRepository();
            for (int i = 0; i < 100; i++)
            {
                var category1 = new Category
                {
                    Id = i + 1,
                };

                var product = new Product
                {
                    Name = Faker.NameFaker.Name(),
                    Price = Faker.NumberFaker.Number(30, 5000),
                    Catagory = category1
                };
                productRepository.Insert(product);
            }

            Console.WriteLine("add multi product completed...");
            Console.ReadKey();

        }
        static void CreateCategory()
        {
            var categoryRepository = new CategoryRepository();
            var categories = new List<Category>();
            for (var i = 0; i < 100; i++)
            {
                var category1 = new Category
                {
                    Name = Faker.StringFaker.Alpha(60),
                    ParentId = 0
                };
                categories.Add(category1);
                //categoryRepository.Insert(category1);
            }
            categoryRepository.InsertMultiCategories(categories);
            Console.WriteLine("insert multi categories completed...");
            Console.ReadKey();
        }
        public static void SetUpMigration()
        {
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
