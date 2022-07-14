using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace it.Areas.Admin.Models
{
    [Table("tbl_record_train")]
    public class RecordTrainModel
    {
        [Key]
        public int id { get; set; }

        public int procedure_version_id { get; set; }
        public int employee_id { get; set; }
        public int record_id { get; set; }

        [ForeignKey("record_id")]
        public virtual RecordModel? record { get; set; }

        [ForeignKey("procedure_version_id")]
        public virtual ProcedureVersionModel? ProcedureVersion { get; set; }
        public bool? is_pass { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? date_sign { get; set; }
        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public DateTime? deleted_at { get; set; }
    }
}
