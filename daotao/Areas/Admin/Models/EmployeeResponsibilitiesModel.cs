using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace it.Areas.Admin.Models
{
    [Table("tbl_employee_responsibilities")]
    public class EmployeeResponsibilitiesModel
    {
        [Key]
        public int id { get; set; }

        public string? content { get; set; }
        public string? content_en { get; set; }

        public int employee_id { get; set; }
        [JsonIgnore]
        [ForeignKey("employee_id")]
        public EmployeeModel employee { get; set; }
    }
}
