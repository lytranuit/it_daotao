using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace it.Areas.Admin.Models
{
    [Table("tbl_department")]
    public class DepartmentModel
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        public string? name_en { get; set; }
        public string? symbol { get; set; }
        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public DateTime? deleted_at { get; set; }
    }
}
