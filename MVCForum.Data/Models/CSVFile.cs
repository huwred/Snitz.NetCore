using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SnitzCore.Data.Models
{
    public class CSVFiles
    {
        public DataTable Table;

        public CSVFiles(string filepath, DataColumn[] columns)
        {
            Table = new DataTable();
            Table.Columns.AddRange(columns);
            string csvData = System.IO.File.ReadAllText(filepath);

            var pattern = @"(\,|\r?\n|\r|^)(?:""([^""]*(?:""""[^""] *)*)""|([^""\r\n]*))";
            MatchCollection matches = Regex.Matches(csvData, pattern);
            int i = 1;
            foreach (Match match in matches)
            {
                var matched_delimiter = match.Groups[1].Value;

                if (matched_delimiter != ",")
                {
                    i = 1;
                    // Since this is a new row of data, add an empty row to the array.
                    Table.Rows.Add();
                    Table.Rows[Table.Rows.Count - 1][i] = match.Groups[2].Value;
                    i++;
                }
                else
                {
                    Table.Rows[Table.Rows.Count - 1][i] = match.Groups[2].Value.Replace("\"\"", "\"");
                    i++;
                }

            }

        }

    }
}
