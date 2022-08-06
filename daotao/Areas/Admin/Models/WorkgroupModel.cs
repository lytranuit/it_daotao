using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace it.Areas.Admin.Models
{
    [Table("tbl_workgroup")]
    public class WorkgroupModel
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        public string? name_en { get; set; }
        public int department_id { get; set; }


        [ForeignKey("department_id")]
        public virtual DepartmentModel? department { get; set; }

        public virtual List<WorkgroupProcedureModel> procedures { get; set; }
        //public virtual List<>? attachments { get; set; }
        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public DateTime? deleted_at { get; set; }
    }
}
