using System.Collections.Generic;
using Dapper;
using System.Data.SqlClient;

namespace DapperSimple.Data
{
    public abstract class BaseRepository
    {
        public void Insert<T>(string sql, T t)
        {
            using (var connection = new SqlConnection(DataConfig.GetPrimaryConnectionString()))
            {
                connection.Open();
                connection.Execute(sql, t);
            }
        }
        public void InsertMultiCategories<T>(string sql, List<T> t)
        {
            using (var connection = new SqlConnection(DataConfig.GetPrimaryConnectionString()))
            {
                connection.Open();
                connection.Execute(sql, t);
            }
        }       
    }
}
