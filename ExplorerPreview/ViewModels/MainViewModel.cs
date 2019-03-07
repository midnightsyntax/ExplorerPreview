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

        public MainViewModel()
        {
            Data = new Data();
            Data.RegTypes.Add(new RegType(".json", @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\.jsonld", "text"));
            Data.RegTypes.Add(new RegType(".txt", @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\.txt", "text"));
            Data.RegTypes.Add(new RegType(".bat", @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\.bat", "none"));
        }
    }
}
