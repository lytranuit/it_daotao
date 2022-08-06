using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace it.Areas.Admin.Models
{
    [Table("tbl_employee_report")]
    public class EmployeeReportModel
    {
        [Key]
        public int id { get; set; }

        public int employee_report_id { get; set; }
        //[ForeignKey("employee_report_id")]
        //public virtual EmployeeModel employee_report { get; set; }
        public int employee_id { get; set; }

        [ForeignKey("employee_id")]
        public virtual EmployeeModel? employee { get; set; }
    }
}
