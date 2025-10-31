using System.Text.Json.Serialization;

namespace SnitzCore.BackOffice.ViewModels
{
  public class RowDataClass {
        [JsonPropertyName("id")]
        public int id { get; set; }

        [JsonPropertyName("rowData")]
        public List<string>? rowData { get; set; }

  }
}
