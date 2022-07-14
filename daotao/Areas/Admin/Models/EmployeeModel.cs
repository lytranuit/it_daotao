using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace it.Areas.Admin.Models
{
    [Table("tbl_employee")]
    public class EmployeeModel
    {
        [Key]
        public int id { get; set; }

        public string? email { get; set; }
        public string FullName { get; set; }
        public string? msnv { get; set; }
        public string? position { get; set; }
        public string? work_group { get; set; }
        public string? education { get; set; }
        public int? version { get; set; }
        public int department_id { get; set; }
        [ForeignKey("department_id")]
        public virtual DepartmentModel? department { get; set; }


        public virtual List<EmployeeWorkgroupModel> workgroups { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? date_work { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? birthday { get; set; }




        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public DateTime? deleted_at { get; set; }


        [NotMapped]
        public virtual List<RecordTrainModel>? trains { get; set; }
    }
}
