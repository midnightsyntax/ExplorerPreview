using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ExplorerPreview
{
    public class Data : INotifyPropertyChanged
    {
        public RangeObservableCollection<RegType> RegTypes { get; set; }
        public RangeObservableCollection<RegType> FileteredTypes { get; set; }

        private ICollectionView _FileListView { get; set; }
        private ObservableCollection<RegType> _FilesList;
        public ObservableCollection<RegType> Files
        {
            get
            {
                return _FilesList;
            } set
            {
                _FilesList = value;
            }
        }

        
        public CollectionViewSource view { get; set; }

        private RegType _lastSelectedListItem;
        public RegType LastSelectedListItem {
            get
            {
                return _lastSelectedListItem;
            }
            set
            {
                _lastSelectedListItem = value;
                OnPropertyChanged("LastSelectedListItem");
            }
        }


        //public string FilterSearch
        //{
        //    get {
        //        if (_filterSearch == null) {
        //            _filterSearch = "";
        //        }
        //        return _filterSearch;
        //        }
        //    set {
        //        _filterSearch = value;
        //        OnPropertyChanged("FilterSearch");
        //    }
        //}
        private string _filterSearch;
        public string FilterSearch { get => _filterSearch; set => _filterSearch = value; }

        private bool _applyButtonStatus;
        public bool ApplyButtonStatus
        {
            get
            {
                return _applyButtonStatus;
            }
            set
            {
                _applyButtonStatus = value;
                OnPropertyChanged("ApplyButtonStatus");
            }
        }
        private int _screenY;
        private int _screenX;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public void Filter()
        {
            _FileListView.Filter = item => ((RegType)item).File.Contains(FilterSearch) == true;
        }

        public int ScreenY { get => _screenY; set => _screenY = value; }
        public int ScreenX { get => _screenX; set => _screenX = value; }

        public void InitFilters()
        {
            _FileListView = CollectionViewSource.GetDefaultView(_FilesList);
        }

        public void Refresh()
        {
            _FileListView.Refresh();
        }

        public Data()
        {
            _FilesList = new ObservableCollection<RegType>();

            RegTypes = new RangeObservableCollection<RegType>();
            FileteredTypes = new RangeObservableCollection<RegType>();

            ScreenX = 2560;
            ScreenY = 1440;

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var rtA = new RegType(".json", @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\.jsonld", "text");
                var rtB = new RegType(".txt", @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\.txt", "text");
                var rtC = new RegType(".bat", @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\.bat", "none");

                RegTypes.Add(rtA);
                RegTypes.Add(rtB);
                RegTypes.Add(rtC);

            }
            
        }
    }
}
