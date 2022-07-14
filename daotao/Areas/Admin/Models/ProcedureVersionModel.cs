using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace it.Areas.Admin.Models
{
    [Table("tbl_procedure_version")]
    public class ProcedureVersionModel
    {
        [Key]
        public int id { get; set; }

        public string? name { get; set; }
        public string? code { get; set; }
        public int version { get; set; }


        public int procedure_id { get; set; }

        [ForeignKey("procedure_id")]
        public virtual ProcedureModel? procedure { get; set; }

        public bool need_train { get; set; }
        public DateTime? date_review { get; set; }
        public DateTime? date_effect { get; set; }
        public DateTime? date_approve { get; set; }
        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public DateTime? deleted_at { get; set; }
    }
}
