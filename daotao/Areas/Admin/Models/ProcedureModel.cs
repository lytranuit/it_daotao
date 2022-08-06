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
        public string? name_en { get; set; }
        public string code { get; set; }
        public string? type { get; set; }
        [NotMapped]
        public List<string>? e_type
        {
            get
            {
                return string.IsNullOrEmpty(type) ? new List<string>() : type.Split(",").ToList();
            }
            set
            {
                type = string.Join(",", value);
            }
        }
        public string? frequency { get; set; }
        [NotMapped]
        public List<string>? e_frequency
        {
            get
            {
                return string.IsNullOrEmpty(frequency) ? new List<string>() : frequency.Split(",").ToList();
            }
            set
            {
                frequency = string.Join(",", value);
            }
        }
        public int? department_id { get; set; }

        [ForeignKey("department_id")]

        public virtual DepartmentModel? department { get; set; }
        public virtual List<ProcedureVersionModel>? versions { get; set; }
        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public DateTime? deleted_at { get; set; }
    }
}
