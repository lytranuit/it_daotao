using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace it.Areas.Admin.Models
{
    [Table("tbl_document")]
    public class DocumentModel
    {
        [Key]
        public int id { get; set; }

        public string? title { get; set; }
        public int type_id { get; set; }
        [Column("data")]
        internal string? _data { get; set; }

        [NotMapped]
        public JObject E_data
        {
            get
            {
                return JsonConvert.DeserializeObject<JObject>(string.IsNullOrEmpty(_data) ? "{}" : _data);
            }
            set
            {
                _data = value.ToString();
            }
        }
        public string? file_url { get; set; }
        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public DateTime? deleted_at { get; set; }
    }
}
