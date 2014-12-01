using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSimple.Data.Models
{
    public  class Product
    {
        public Product()
        {
            Catagory = new Category();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public Category Catagory { get; set; }
    }
}
