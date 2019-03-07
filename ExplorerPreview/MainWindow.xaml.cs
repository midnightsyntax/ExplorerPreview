using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExplorerPreview
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Data data;

        double dpiX = 0;
        double dpiY = 0;
        double ListHeight;

        bool TypeAnimTriggerd = false;

        private const string RegPrefix = "_PREVIEW_DEFAULT_";

        public MainWindow()
        {
            data = new Data();

            using (RegistryKey tempKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes"))
            {
                foreach (var key in tempKey.GetSubKeyNames())
                {
                    if (key != "")
                    {
                        using (RegistryKey keyValue = tempKey.OpenSubKey(key))
                        {
                            var finalKey = keyValue.GetValue("PerceivedType");
                            if (key[0] == '.')
                            {
                                var rk = new RegType(key, string.Join("\\", tempKey.Name, key), finalKey == null ? "" : finalKey.ToString());
                                data.Files.Add(rk);
                            }
                        }
                    }
                }
            }
            InitializeComponent();

            data.InitFilters();
            DataContext = data;
            Loaded += MainWindow_Loaded;
            ListTypes.Height = 300;
            SaveNewTypeButton.Visibility = Visibility.Collapsed;
            NewTypeTextBox.Visibility = Visibility.Collapsed;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //SizeToContent = SizeToContent.Width;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            PresentationSource source = PresentationSource.FromVisual(this);

            if (source != null)
            {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
            }

            SearchBox.Focus();

            ListHeight = ListTypes.ActualHeight;


            base.OnSourceInitialized(e);
        }

        private string SelectedRadio;
        private string LastPercievedType = null;

        private void Load()
        {
            //RegTypeModel regTypeModel = new RegTypeModel();
            //System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(ModelMap));
            //using (var writer = new System.IO.StreamWriter(@"e:\test.xml"))
            //{
            //    serializer.Serialize(writer, regTypeModel);
            //}
        }

        private void ListTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListTypes.SelectedIndex == -1) return;
            if (infoFileTypeValue.Visibility == Visibility.Hidden) infoFileTypeValue.Visibility = Visibility.Visible;
            if (infoPerceivedTypeValue.Visibility == Visibility.Hidden) infoPerceivedTypeValue.Visibility = Visibility.Visible;

            data.LastSelectedListItem = (RegType)ListTypes.Items.GetItemAt(ListTypes.SelectedIndex);
            RegType selItem = data.Files[ListTypes.SelectedIndex];
            if (ListTypes.SelectedIndex >= 0)
            {
                LastPercievedType = data.LastSelectedListItem.PercievedType;
                if (data.LastSelectedListItem.PercievedType != "")
                {
                    string fileType = data.LastSelectedListItem.PercievedType.ToLower();

                    btnsItemsControl.Items.Cast<RadioButton>().ToList().ForEach(b => {
                        if (b.Content.ToString().ToLower() == fileType)
                        {
                            b.IsChecked = true;
                        }
                        else
                        {
                            b.IsChecked = false;
                        }
                    });

                    ApplyButton.IsEnabled = false;
                    SetButtonText(fileType);
                }
                else
                {
                    PercievedTypeNoneRadioButton.IsChecked = true;
                    SetButtonText("none");
                }
            }
            else
            {
                LastPercievedType = "None";
                SetButtonText("none");
            }

        }

        private void FilterList()
        {
            data.Filter();
            //data.view.View.Filter = item => ((RegType)item).File.Contains(SearchBox.Text);
            //data.view.View.Refresh();

            //data.view.Filter += (s, e) => { e.Accepted = ((RegType)e.Item).File.Contains(SearchBox.Text); };

            //data.Files.FilterFrom(data.RegTypes, (t) => t.File.Contains(SearchBox.Text));
            //btnsItemsControl.Items.Cast<RadioButton>().ToList().ForEach(b => { b.IsChecked = false; });
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            data.FilterSearch = SearchBox.Text;
            infoFileTypeValue.Visibility = Visibility.Hidden;
            infoPerceivedTypeValue.Visibility = Visibility.Hidden;

            if (ListTypes == null) return;
            FilterList();
        }
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {

            var style = ApplyButton.Style;
            var _key = data.LastSelectedListItem.File;
            using (RegistryKey tempKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes"))
            {
                using (RegistryKey key = tempKey.OpenSubKey(_key, true))
                {
                    string newPType = SelectedRadio.ToLower();
                    Debug.WriteLine(string.Format("set {0} to {1}", _key, newPType));
                    var prefixKey = key.CreateSubKey(RegPrefix, true);
                    if (Array.Exists(key.GetSubKeyNames(), skey => skey == RegPrefix))
                    {
                        var conTyp = key.GetValue("Content Type");
                        var perTyp = key.GetValue("PerceivedType");
                        Debug.WriteLine($"Write default backup key '{_key}': Content Type: {conTyp}, PerceivedType: {perTyp}");
                        if (conTyp != null) prefixKey.SetValue("Content Type", conTyp);
                        if (perTyp != null) prefixKey.SetValue("PerceivedType", perTyp);
                    }

                    key.SetValue("PerceivedType", newPType);
                    LastPercievedType = newPType;
                    ApplyButton.IsEnabled = false;
                    data.LastSelectedListItem.PercievedType = newPType;
                    if (newPType == "none")
                    {
                        MessageBox.Show(string.Format("{0} percieved type removed. Explorer preview pane will not preview files of this type.", _key, newPType), "Perceived Type Change", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }
                    else
                    {
                        MessageBox.Show(string.Format("{0} will now be previewed as {1} in the Explorer preview pane", _key, newPType), "Perceived Type Change", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }
                }
            }
        }
        private void PercievedTypeGroup_Click(object sender, RoutedEventArgs e)
        {
            var btn = e.Source as RadioButton;
            string selText = btn.Content.ToString().ToLower();

            ApplyButton.IsEnabled = LastPercievedType == selText ? false : true;
            SelectedRadio = selText;
            SetButtonText(selText);
        }
        private void SetButtonText(string str)
        {
            ApplyButton.Content = str == "" ? "Set preview type" : string.Format("Set preview type '{0}'", str);
        }

        private void NewTypeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            FadeInButtonNewType(false);
        }
        private void NewTypeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            FadeOutButtonNewType(false);
        }

        private void FadeInButtonNewType(bool full)
        {
            double animFrom = full ? 0.8 : NewTypeButton.Opacity;
            DoubleAnimation anim = new DoubleAnimation(animFrom, 1, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd)
            {
                EasingFunction = new SineEase()
                {
                    EasingMode = EasingMode.EaseIn
                }
            };
            NewTypeButton.BeginAnimation(OpacityProperty, anim);
        }
        private void FadeOutButtonNewType(bool full)
        {
            double animTo = full ? 0 : 0.4;
            DoubleAnimation anim = new DoubleAnimation(NewTypeButton.Opacity, animTo, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd)
            {
                EasingFunction = new SineEase()
                {
                    EasingMode = EasingMode.EaseIn
                }
            };
            NewTypeButton.BeginAnimation(OpacityProperty, anim);
        }

        private void NewTypeButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            NewTypeButton.Visibility = Visibility.Collapsed;
            NewTypeTextBox.Visibility = Visibility.Visible;

            SaveNewTypeButton.Opacity = 1;
            SaveNewTypeButton.Visibility = Visibility.Visible;

            NewTypeTextBox.Focus();
            NewTypeTextBox.Select(NewTypeTextBox.Text.Length, 1);


            if (!TypeAnimTriggerd)
            {
                ListTypes.BeginAnimation(HeightProperty, new DoubleAnimation(ListHeight, ListHeight - 50, TimeSpan.FromMilliseconds(400), FillBehavior.HoldEnd)
                {
                    EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut }
                });
            }
            TypeAnimTriggerd = true;
        }
        private void NewTypeTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                NewTypeTextBox.Text = "";
                NewTypeButton.Opacity = 1;
                HideTextboxNewType();
            }
        }

        private void HideTextboxNewType()
        {
            NewTypeTextBox.Text = "";

            ListTypes.BeginAnimation(HeightProperty, new DoubleAnimation(ListTypes.ActualHeight, ListHeight, TimeSpan.FromMilliseconds(400), FillBehavior.Stop)
            {
                EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut }
            });

            Storyboard sb = new Storyboard();
            TypeAnimTriggerd = false;

            var fade = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                FillBehavior = FillBehavior.Stop
            };

            Storyboard.SetTarget(fade, SaveNewTypeButton);
            Storyboard.SetTargetProperty(fade, new PropertyPath(OpacityProperty));

            sb.Completed += (s, e) =>
            {
                SaveNewTypeButton.Visibility = Visibility.Collapsed;
                NewTypeButton.Visibility = Visibility.Visible;
                NewTypeTextBox.Visibility = Visibility.Collapsed;
            };

            sb.Children.Add(fade);
            sb.Begin();
        }

        private void SaveNewTypeButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (NewTypeTextBox.Text == "")
            {
                MessageBox.Show("File type cant be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                SaveNew();
            }
        }
        private void SaveNew()
        {
            var _v = NewTypeTextBox.Text.Trim();
            var newType = (_v[0] == '.' ? _v : "." + _v);
            using (RegistryKey tempKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes", true))
            {
                RegistryKey key = tempKey.OpenSubKey(newType, false);
                if (key == null)
                {
                    bool success = false;
                    try
                    {
                        tempKey.CreateSubKey(newType).SetValue("PerceivedType", "none");
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Something went wrong! Exception: {0}", ex.InnerException.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        if (success)
                        {
                            MessageBox.Show(string.Format("{0} is now a registered file type! Click OK to set Perceived Type", newType), "New File Type Registered", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                            HideTextboxNewType();
                            var _newRegType = new RegType(newType, newType, "none");
                            data.Files.Add(_newRegType);
                            SearchBox.Text = newType;
                            data.LastSelectedListItem = _newRegType;
                            PercievedTypeNoneRadioButton.IsChecked = true;
                            data.Refresh();
                            ListTypes.SelectedIndex = ListTypes.Items.IndexOf(_newRegType);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(string.Format("{0} is already a registered file type", newType), "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void ContentGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CheckMouseHover();
        }
        private void ContentGrid_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CheckMouseHover();
        }

        private void CheckMouseHover()
        {
            if (TypeAnimTriggerd)
            {
                if (!(gridBottomLeft.IsMouseOver || ApplyButton.IsMouseOver))
                {
                    HideTextboxNewType();
                    TypeAnimTriggerd = false;
                }
            }
        }
    }
}
