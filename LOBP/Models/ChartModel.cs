using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LOBP.Models
{
    public class MyApplicationData
    {
        public int TotalCount { get; set; }
        public int ExpiringCount { get; set; }
        public int ProcessingCount { get; set; }
        public int YetToSubmitCount { get; set; }
        public int PermitCount { get; set; }
        public int AppointmentCount { get; set; }
    }




}