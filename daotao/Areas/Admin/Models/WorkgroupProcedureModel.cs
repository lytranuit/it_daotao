using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace it.Areas.Admin.Models
{
    [Table("tbl_workgroup_procedure")]
    public class WorkgroupProcedureModel
    {
        [Key]
        public int id { get; set; }

        public int workgroup_id { get; set; }
        //[JsonIgnore]
        [ForeignKey("workgroup_id")]
        public virtual WorkgroupModel? workgroup { get; set; }

        public int procedure_id { get; set; }

        //[JsonIgnore]
        [ForeignKey("procedure_id")]
        public virtual ProcedureModel? procedure { get; set; }
    }
}
