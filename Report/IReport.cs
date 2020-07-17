using System;
using System.Collections.Generic;
using System.Text;

namespace Maps.Report
{
    public interface IReport
    {
        public void Save(string text);

        public void SaveAndDisplay(string filename,string text);
    }
}

