using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TkfClient.Models
{
    [Table("Candle")]
    internal class CandleSync
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string Uid { get; set; }

        public string Isin { get; set; }

        public DateTime Time { get; set; }

        public decimal Open { get; set; }

        public decimal Close { get; set; }

        public long Volume { get; set; }

        public decimal Low { get; set; }

        public decimal High { get; set; }
    }
}
