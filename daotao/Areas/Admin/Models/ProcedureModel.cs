using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace it.Areas.Admin.Models
{
    [Table("tbl_procedure")]
    public class ProcedureModel
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        public string code { get; set; }
        public int? department_id { get; set; }

        public virtual List<ProcedureVersionModel>? versions { get; set; }
        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public DateTime? deleted_at { get; set; }
    }
}
