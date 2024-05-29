using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TkfClient.Models
{
    [Table("Share")]
    public class Share
    {
        [Key]
        public string Uid { get; set; }

        public string Isin { get; set; }

        public string Ticker { get; set; }

        public int Lot { get; set; }

        public string Currency { get; set; }

        public string Name { get; set; }

        public int ListSection { get; set; }

    }
}
