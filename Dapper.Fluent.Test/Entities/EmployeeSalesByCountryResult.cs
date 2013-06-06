using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Fluent.Tests.Entities
{
    public partial class EmployeeSalesByCountryResult
    {
        public string Country { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public DateTime ShippedDate { get; set; }
        public int OrderId { get; set; }
        public float SaleAmount { get; set; }
    }
}
