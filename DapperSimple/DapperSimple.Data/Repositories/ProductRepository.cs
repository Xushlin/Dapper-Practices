using DapperSimple.Data.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DapperSimple.Data.Repositories
{
    public class ProductRepository : BaseRepository
    {
        /// <summary>
        /// 插入一条Product
        /// </summary>
        /// <param name="product"></param>
        public void Insert(Product product)
        {
            const string sql = "insert into product(Name,Price,CategoryId)values(@Name,@Price,@CategoryId)";
            base.Insert<dynamic>(sql, new { Price = product.Price, Name = product.Name, CategoryId = product.Catagory.Id });
        }
        /// <summary>
        /// 插入多条Products
        /// </summary>
        /// <param name="products"></param>
        public void InsertMultiCategories(List<Product> products)
        {
            const string sql = "insert into product(Name,Price,CategoryId)values(@Name,@Price,@CategoryId)";
            base.InsertMultiCategories<Product>(sql, products);
        }

        /// <summary>
        /// 关联查询多张表多对一关系
        /// </summary>
        /// <returns></returns>
        public List<Product> QueryProduct()
        {
            const string sql = "select p.*,c.* from product p  Left join Category c on c.Id=p.CateGoryId";
            using (var connection = new SqlConnection(DataConfig.GetPrimaryConnectionString()))
            {
                connection.Open();
                return connection.Query<Product, Category, Product>(sql, (p, c) => { p.Catagory = c; return p; }, null, null, false, "Id", null, null).ToList();
            }
        }
    }
}
