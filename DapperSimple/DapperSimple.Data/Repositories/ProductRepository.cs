using DapperSimple.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DapperSimple.Data.Repositories
{
    public class ProductRepository : DatabaseProxy
    {
        public void Insert(Product product)
        {
            string sql = "insert into product(Name,Price,CategoryId)values(@Name,@Price,@CategoryId)";
            base.Insert<dynamic>(sql, new { Price = product.Price, Name = product.Name, CategoryId = product.Catagory.Id });
        }

        public void InsertMultiCategories(List<Product> products)
        {
            string sql = "insert into product(Name,Price,CategoryId)values(@Name,@Price,@CategoryId)";
            base.InsertMultiCategories<Product>(sql, products);
        }

        public List<Product> QueryProduct()
        {
            string sql = "select p.*,c.* from product p  Left join Category c on c.Id=p.CateGoryId";
            using (SqlConnection connection = new SqlConnection(DataConfig.GetPrimaryConnectionString()))
            {
                return connection.Query<Product, Category, Product>(sql, (p, c) => { p.Catagory = c; return p; }, null, null, false, "Id", null, null).ToList();
            }
        }

    }
}
