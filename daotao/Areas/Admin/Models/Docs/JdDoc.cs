
namespace it.Areas.Admin.Models.Docs
{
    public class JdDoc
    {
        public int employee_id { get; set; }
        public int version { get; set; }

        public EmployeeModel employee { get; set; }
        public List<EmployeeModel> report_to { get; set; }

        public List<EmployeeModel> replace_to { get; set; }
    }
}
