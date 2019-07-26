using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InsuranceClientPortal.ViewModel
{
    public class CustomerViewModel
    {
        public String Name { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Application Date")]
        public DateTime AppDate { get; set; }

        public String InsuranceType { get; set; } // Partition Key

        [DataType(DataType.EmailAddress)]
        public String Email { get; set; } // Partition Key

        [DataType(DataType.Currency)]
        public double Amount { get; set; }

        [DataType(DataType.Currency)]
        public double Premium { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public IFormFile Image { get; set; }
    }
}
