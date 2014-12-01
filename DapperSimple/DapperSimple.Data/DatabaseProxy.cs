using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using DapperSimple.Data.Migrations;
namespace DapperSimple.Data
{
    public abstract class DatabaseProxy
    {
        public void Insert<T>(string sql, T t)
        {
            using (SqlConnection connection = new SqlConnection(DataConfig.GetPrimaryConnectionString()))
            {
                connection.Execute(sql, t);
            }
        }

        public void InsertMultiCategories<T>(string sql, List<T> t)
        {
            using (SqlConnection connection = new SqlConnection(DataConfig.GetPrimaryConnectionString()))
            {
                connection.Execute(sql, t);
            }
        }       
    }
}
