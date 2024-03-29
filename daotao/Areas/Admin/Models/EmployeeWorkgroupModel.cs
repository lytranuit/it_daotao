﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace it.Areas.Admin.Models
{
    [Table("tbl_employee_workgroup")]
    public class EmployeeWorkgroupModel
    {
        [Key]
        public int id { get; set; }

        public int workgroup_id { get; set; }
        [ForeignKey("workgroup_id")]
        public WorkgroupModel workgroup { get; set; }

        public int employee_id { get; set; }
        [ForeignKey("employee_id")]
        public EmployeeModel employee { get; set; }
    }
}
