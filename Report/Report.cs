using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Maps.Report
{
    public class ReportComparer : IReport
    {
        string path = "MapsComparer01.txt";

        void IReport.Save(string text)
        {
            File.WriteAllText(path, text.ToString());
        }

        void IReport.SaveAndDisplay(string filename,string text)
        {
            Console.WriteLine(text);

            var csv = new StringBuilder(text);
            csv.AppendLine("");
            File.AppendAllText(filename, csv.ToString());
        }
    }
}
