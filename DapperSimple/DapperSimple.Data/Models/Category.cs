using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSimple.Data.Models
{
    [Table("Category")]
    public class Category
    {
        [KeyAttribute]
        public int? Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
    }

    public class CategoryExt
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public List<Product> products { get; set; }
    }
}
