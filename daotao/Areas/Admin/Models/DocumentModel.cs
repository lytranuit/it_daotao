using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace it.Areas.Admin.Models
{
    [Table("tbl_document")]
    public class DocumentModel
    {
        [Key]
        public int id { get; set; }

        public int document_type { get; set; }
        public string? data { get; set; }
        public string? file_url { get; set; }
        public string? title { get; set; }
        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public DateTime? deleted_at { get; set; }
    }
}
