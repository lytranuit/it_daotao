using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace it.Areas.Admin.Models
{
    [Table("tbl_employee_replace")]
    public class EmployeeReplaceModel
    {
        [Key]
        public int id { get; set; }

        public int employee_replace_id { get; set; }
        //[ForeignKey("employee_replace_id")]
        //public virtual EmployeeModel employee_replace { get; set; }

        public int employee_id { get; set; }

        [ForeignKey("employee_id")]
        public virtual EmployeeModel? employee { get; set; }
    }
}
