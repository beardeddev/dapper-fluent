using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Fluent.Tests.Entities
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public byte[] Picture { get; set; }
    }
}
