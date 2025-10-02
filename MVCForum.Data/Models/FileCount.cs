using SnitzCore.Data.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitzCore.Data.Models
{
    [SnitzTable("FILECOUNT", "FORUM")]

    public class FileCount
    {
        [Column("FC_ID")]
        [Key]
        public int Id { get; set; }
        public string Posted { get; set; }
        public int LinkHits { get; set; }
        public int LinkOrder { get; set; }
        public int Archived { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }

    }
}
