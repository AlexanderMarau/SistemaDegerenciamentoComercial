﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padaria.Repository.Entities
{
    [Table(name: "PayBox")]
    public class PayBox
    {
        [Key]
        [Range(minimum:0,maximum:int.MaxValue, ErrorMessage = "{0} exceeded the registration limit")]
        public int PayBoxID { get; set; }
        [Range(minimum: 0.00, maximum: double.MaxValue, ErrorMessage = "{0} contains invalid values")]
        public double Value { get; set; }
        [DisplayName(displayName:"User")]
        public int UserID { get; set; }
        public virtual Users User { get; set; }
    }
}
