using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SnitzCore.BackOffice.ViewModels
{
  public class RowDataClass {
        [JsonPropertyName("id")]
        public int id { get; set; }

        [JsonPropertyName("rowData")]
        public List<string> rowData { get; set; }

  }
}
