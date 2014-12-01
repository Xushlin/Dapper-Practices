using DapperSimple.Data.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using System.Data.Common;
using Dapper.Contrib.Extensions;

namespace DapperSimple.Data.Repositories
{
    public class CategoryRepository : BaseRepository
    {
        public void AddCategory(Category category)
        {
            using (DbConnection connection = new SqlConnection(DataConfig.GetPrimaryConnectionString()))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();
                //TODO: SqlMapperExtensions 这里还没有测试通过
                SqlMapperExtensions.Insert<Category>(connection, category);
            }
        }
        /// <summary>
        /// 插入一条数据
        /// </summary>
        /// <param name="category"></param>
        public void Insert(Category category)
        {
            const string sql = "insert into category(Name,ParentId)values(@Name,@ParentId)";
            Insert<Category>(sql, category);
        }
        /// <summary>
        /// 插入多条数据
        /// </summary>
        /// <param name="categories"></param>
        public void InsertMultiCategories(List<Category> categories)
        {
            const string sql = "insert into category(Name,ParentId)values(@Name,@ParentId)";
            InsertMultiCategories<Category>(sql, categories);
        }
        /// <summary>
        /// 查询一对多的关系
        /// </summary>
        /// <returns></returns>
        public List<CategoryExt> QueryCategories()
        {
            const string sql = "SELECT c.*, p.*  FROM Category c  INNER JOIN Product p ON c.Id = p.CategoryId";
            using (var connection = new SqlConnection(DataConfig.GetPrimaryConnectionString()))
            {
                connection.Open();
                var lookup = new Dictionary<int, CategoryExt>();
                connection.Query<CategoryExt, Product, CategoryExt>(sql, (c, p) =>
                {
                    CategoryExt categoryExt;
                    if (!lookup.TryGetValue(c.Id, out categoryExt))
                    {
                        lookup.Add(c.Id, categoryExt = c);
                    }
                    if (categoryExt.Products == null)
                        categoryExt.Products = new List<Product>();
                    categoryExt.Products.Add(p);
                    return categoryExt;
                }).AsQueryable();

                var resultList = lookup.Values;

                return resultList.ToList();
            }
        }
    }
}
