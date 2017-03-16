// The MIT License (MIT)
// Copyright (c) 2013 Khalid Abuhakmeh
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, *  
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Akka.Cluster.Management.Cli.Utils
{
    public class ConsoleTableSettings
    {
        public char FieldDelimiter { get; set; }
        public char RowDelimiter { get; set; }
        public int Padding { get; set; }

        public ConsoleTableSettings(char fieldDelimiter = '\0', char rowDelimiter = '\0', int padding = 2)
        {
            FieldDelimiter = fieldDelimiter;
            RowDelimiter = rowDelimiter;
            Padding = padding;
        }
    }

    public class ConsoleTable
    {
        private readonly StringBuilder table;
        private readonly string[] columnHeadings;
        private readonly ConsoleTableSettings settings;
        private readonly List<List<string>> rows;

        public ConsoleTable(IEnumerable<string> columnHeadings)
            : this(columnHeadings, new ConsoleTableSettings())
        { }

        public ConsoleTable(IEnumerable<string> columnHeadings, ConsoleTableSettings settings)
        {
            this.columnHeadings = columnHeadings.ToArray();
            this.settings = settings;
            this.table = new StringBuilder();
            this.rows = new List<List<string>>();
        }

        public void AddRow(IEnumerable<string> row)
        {
            var enumerable = row as string[] ?? row.ToArray();
            if (enumerable.Length < columnHeadings.Length)
            {
                throw new ArgumentException("The row has less columns than the headings row.");
            }

            rows.Add(new List<string>(enumerable));
        }

        public void WriteToConsole()
        {
            var columnWidths = GetColumnWidths();

            BuildRow(columnWidths, new string[] { });
            BuildRow(columnWidths, columnHeadings);
            BuildRow(columnWidths, new string[] { });

            foreach (var row in rows)
            {
                BuildRow(columnWidths, row.ToArray());
            }

            Console.Write(table.ToString());
            Console.WriteLine("");
        }

        private void BuildRow(IReadOnlyList<int> columnWidths, IReadOnlyList<string> rowValues)
        {
            var row = string.Empty;
            if (rowValues.Count > 0)
            {
                for (var i = 0; i < columnWidths.Count; i++)
                {
                    row += settings.FieldDelimiter
                        + rowValues[i].PadRight(columnWidths[i]) 
                        + string.Empty.PadRight(settings.Padding, ' ');

                    if (i == rowValues.Count - 1)
                    {
                        row += settings.FieldDelimiter;
                    }
                }
            }
            else
            {
                row = columnWidths.Aggregate(row, (current, t) => current 
                    + char.ToString(settings.RowDelimiter).PadRight(t + settings.Padding * 2, settings.RowDelimiter)
                    + char.ToString(settings.RowDelimiter));
            }
            table.AppendLine(row);
        }

        private int[] GetColumnWidths()
        {
            var columnWidths = new int[columnHeadings.Length];

            for (var i = 0; i < columnHeadings.Length; i++)
            {
                columnWidths[i] = columnHeadings[i].Length;
            }

            for (var i = 0; i < rows.Count; i++)
            {
                for (var j = 0; j < rows[i].Count; j++)
                {
                    if (columnWidths[j] < rows.ElementAt(i).ElementAt(j).Length)
                    {
                        columnWidths[j] = rows[i].ElementAt(j).Length;
                    }
                }
            }
            return columnWidths;
        }
    }
}
