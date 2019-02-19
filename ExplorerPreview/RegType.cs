using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerPreview
{
    [Serializable]
    public class RegTypeModel {

        public string Key;

        public RegTypeModel()
        {

        }
    }

    public class RegType : INotifyPropertyChanged
    {
        private string _file;
        private string _regKey;
        private string _percievedType;

        public string File { get => _file; set { _file = value; OnPropertyChanged("File"); } }
        public string RegKey { get => _regKey; set { _regKey = value; OnPropertyChanged("RegKey"); } }
        public string PercievedType { get => _percievedType; set { _percievedType = value; OnPropertyChanged("PercievedType"); } }

        //public string File { get; set; }
        //public string RegKey { get; set; }
        //public string PercievedType { get; set; }

        public void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public RegType()
        {

        }

        public RegType(string file, string key, string percievedType)
        {
            File = file;
            RegKey = key;
            PercievedType = percievedType;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
