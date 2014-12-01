using DapperSimple.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.Common;
using Dapper.Contrib.Extensions;

namespace DapperSimple.Data.Repositories
{
    public class CategoryRepository : DatabaseProxy
    {

        public void AddCategory(Category category)
        {
            using (DbConnection connection = new SqlConnection(DataConfig.GetPrimaryConnectionString()))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();
                SqlMapperExtensions.Insert<Category>(connection, category);
            }
        }
        public void Insert(Category category)
        {
            string sql = "insert into category(Name,ParentId)values(@Name,@ParentId)";
            base.Insert<Category>(sql, category);
        }

        public void InsertMultiCategories(List<Category> categories)
        {
            string sql = "insert into category(Name,ParentId)values(@Name,@ParentId)";
            base.InsertMultiCategories<Category>(sql, categories);
        }

        public List<CategoryExt> QueryCategories()
        {
            string sql = "SELECT c.*, p.*  FROM Category c  INNER JOIN Product p ON c.Id = p.CategoryId";
            using (SqlConnection connection = new SqlConnection(DataConfig.GetPrimaryConnectionString()))
            {

                var lookup = new Dictionary<int, CategoryExt>();
connection.Query<CategoryExt, Product, CategoryExt>(sql, (c, p) => {
                         CategoryExt categoryExt;
                         if (!lookup.TryGetValue(c.Id, out categoryExt)) {
                             lookup.Add(c.Id, categoryExt = c);
                         }
                         if (categoryExt.products == null) 
                             categoryExt.products = new List<Product>();
                         categoryExt.products.Add(p);
                         return categoryExt;
                     }).AsQueryable();

var resultList = lookup.Values;

                return resultList.ToList();
               // return connection.Query<Product, Category, Product>(sql, (p, c) => { p.Catagory = c; return p; }, null, null, false, "Id", null, null).ToList();
            }
        }
    }
}
