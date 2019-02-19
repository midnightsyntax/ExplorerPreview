using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerPreview.ViewModels
{
    public class MainViewModel
    {
        public Data Data { get; set; }
        public MVSettings Settings { get; set; }

        public MainViewModel()
        {
            Settings = new MVSettings();
            Data = new Data();
            var rtA = new RegType(".json", @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\.jsonld", "text");
            var rtB = new RegType(".txt", @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\.txt", "text");
            var rtC = new RegType(".bat", @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\.bat", "none");

            Data.RegTypes.Add(rtA);
            Data.RegTypes.Add(rtB);
            Data.RegTypes.Add(rtC);
        }
    }

    public class MVSettings
    {
        private double _listBoxHeight = 280;
        public double ListBoxHeight { get => _listBoxHeight; set => _listBoxHeight = value; }

        public MVSettings()
        {
            ListBoxHeight = 280;
        }
    }
}
