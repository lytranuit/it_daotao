using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace it.Areas.Admin.Models
{
    [Table("tbl_record")]
    public class RecordModel
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        public int department_id { get; set; }


        [ForeignKey("department_id")]
        public virtual DepartmentModel? department { get; set; }

        public virtual List<RecordTrainModel>? trains { get; set; }
        public string? purpose { get; set; }
        public string? address { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? date { get; set; }
        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public DateTime? deleted_at { get; set; }
    }
}
