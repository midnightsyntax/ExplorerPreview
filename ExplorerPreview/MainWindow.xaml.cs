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
            listTypes.Height = 300;
            btnSaveNewType.Visibility = Visibility.Collapsed;
            tbNewType.Visibility = Visibility.Collapsed;

            //data.view = new CollectionViewSource()
            //{
            //    Source = data.RegTypes
            //};
            //data.view.View.Filter = item => ((RegType)item).File.Contains(txtBox.Text);
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

            txtBox.Focus();

            ListHeight = listTypes.ActualHeight;


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

        private void listTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listTypes.SelectedIndex == -1) return;
            //var albi = (listTypes.ItemContainerGenerator.ContainerFromItem(e.AddedItems[0]) as ListBoxItem);
            //var sss = VisualStateManager.GoToState(albi, "MouseEnter", true);
            if (infoFileTypeValue.Visibility == Visibility.Hidden) infoFileTypeValue.Visibility = Visibility.Visible;
            if (infoPerceivedTypeValue.Visibility == Visibility.Hidden) infoPerceivedTypeValue.Visibility = Visibility.Visible;
            
            data.LastSelectedListItem = (RegType)listTypes.Items.GetItemAt(listTypes.SelectedIndex);
            RegType selItem = data.Files[listTypes.SelectedIndex];
            if (listTypes.SelectedIndex >= 0)
            {
                LastPercievedType = data.LastSelectedListItem.PercievedType;
                if (data.LastSelectedListItem.PercievedType != "")
                {
                    string fileType = data.LastSelectedListItem.PercievedType.ToLower();

                    bool isCustom = true;

                    btnsItemsControl.Items.Cast<RadioButton>().ToList().ForEach(b => {
                        if (b.Content.ToString().ToLower() == fileType)
                        {
                            b.IsChecked = true;
                            isCustom = false;
                        } else
                        {
                            b.IsChecked = false;
                        }
                    });

                    btnApply.IsEnabled = false;
                    if (isCustom) tbCustom.Text = fileType;
                    SetButtonText(fileType);
                }
                else
                {
                    radioNone.IsChecked = true;
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
            //data.view.View.Filter = item => ((RegType)item).File.Contains(txtBox.Text);
            //data.view.View.Refresh();

            //data.view.Filter += (s, e) => { e.Accepted = ((RegType)e.Item).File.Contains(txtBox.Text); };

            //data.Files.FilterFrom(data.RegTypes, (t) => t.File.Contains(txtBox.Text));
            //btnsItemsControl.Items.Cast<RadioButton>().ToList().ForEach(b => { b.IsChecked = false; });
        }

        private void txtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            data.FilterSearch = txtBox.Text;
            infoFileTypeValue.Visibility = Visibility.Hidden;
            infoPerceivedTypeValue.Visibility = Visibility.Hidden;

            if (listTypes == null) return;
            FilterList();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            
            var style = btnApply.Style;
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
                    btnApply.IsEnabled = false;
                    data.LastSelectedListItem.PercievedType = newPType;
                    MessageBox.Show(string.Format("{0} perceived type is now {1}", _key, newPType), "Perceived Type Change", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
            }
        }

        private void radio_Click(object sender, RoutedEventArgs e)
        {
            var btn = e.Source as RadioButton;
            string selText = (btn.Content.GetType().Equals(typeof(TextBox)) ? tbCustom.Text : btn.Content.ToString()).ToLower();
            
            if (btn.Name == "radioCustom")
            {
                OnCustomRadio();
            } else
            {
                btnApply.IsEnabled = LastPercievedType == selText ? false : true;
            }
            SelectedRadio = selText;
            SetButtonText(selText);
        }

        private void OnCustomRadio()
        {
            radioCustom.IsChecked = true;
            tbCustom.Focus();
            SetButtonText(tbCustom.Text.Trim());
        }

        private void SetButtonText(string str)
        {
            btnApply.Content = str == "" ? "Set preview type" : string.Format("Set preview type '{0}'", str);
            //if (str == "")
            //{
            //    btnApply.Content = "Set preview type";
            //}
            //else
            //{
            //    btnApply.Content = string.Format("Set preview type '{0}'", str);
            //}
        }

        private void tbCustom_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            OnCustomRadio();
        }

        private void tbCustom_TextChanged(object sender, TextChangedEventArgs e)
        {
            string txt = tbCustom.Text.Trim();
            if (txt == "") btnApply.IsEnabled = false;
            else btnApply.IsEnabled = true;

            SetButtonText(txt);
        }

        private void btnNewType_MouseEnter(object sender, MouseEventArgs e)
        {
            FadeInButtonNewType(false);
        }

        private void btnNewType_MouseLeave(object sender, MouseEventArgs e)
        {
            FadeOutButtonNewType(false);
        }

        private void FadeInButtonNewType(bool full)
        {
            double animFrom = full ? 0.8 : btnNewType.Opacity;
            DoubleAnimation anim = new DoubleAnimation(animFrom, 1, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd)
            {
                EasingFunction = new SineEase()
                {
                    EasingMode = EasingMode.EaseIn
                }
            };
            btnNewType.BeginAnimation(OpacityProperty, anim);
        }

        private void FadeOutButtonNewType(bool full)
        {
            double animTo = full ? 0 : 0.4;
            DoubleAnimation anim = new DoubleAnimation(btnNewType.Opacity, animTo, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd)
            {
                EasingFunction = new SineEase()
                {
                    EasingMode = EasingMode.EaseIn
                }
            };
            btnNewType.BeginAnimation(OpacityProperty, anim);
        }

        private void tbNewType_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnNewType_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            btnNewType.Visibility = Visibility.Collapsed;
            tbNewType.Visibility = Visibility.Visible;

            btnSaveNewType.Opacity = 1;
            btnSaveNewType.Visibility = Visibility.Visible;

            tbNewType.Focus();
            tbNewType.Select(tbNewType.Text.Length, 1);


            if (!TypeAnimTriggerd)
            {
                listTypes.BeginAnimation(HeightProperty, new DoubleAnimation(ListHeight, ListHeight - 50, TimeSpan.FromMilliseconds(400), FillBehavior.HoldEnd)
                {
                    EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut }
                });
            }
            TypeAnimTriggerd = true;
        }

        private void tbNewType_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                tbNewType.Text = "";
                btnNewType.Opacity = 1;
                HideTextboxNewType();
            }
        }

        public void HideTextboxNewType()
        {
            tbNewType.Text = "";
            //tbNewType.Visibility = Visibility.Collapsed;
            //btnNewType.Visibility = Visibility.Visible;

            //tbNewType.Visibility = Visibility.Collapsed;
            listTypes.BeginAnimation(HeightProperty, new DoubleAnimation(listTypes.ActualHeight, ListHeight, TimeSpan.FromMilliseconds(400), FillBehavior.Stop)
            {
                EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut }
            });

            //listTypes.BeginAnimation(HeightProperty, new DoubleAnimation(btnSaveNewType.Opacity, 0, TimeSpan.FromMilliseconds(400), FillBehavior.Stop)
            //{
            //    EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut }
            //});
            Storyboard sb = new Storyboard();

            TypeAnimTriggerd = false;
            var fade = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                FillBehavior = FillBehavior.Stop
            };

            Storyboard.SetTarget(fade, btnSaveNewType);
            Storyboard.SetTargetProperty(fade, new PropertyPath(OpacityProperty));

            sb.Completed += (s, e) =>
            {
                btnSaveNewType.Visibility = Visibility.Collapsed;
                btnNewType.Visibility = Visibility.Visible;
                tbNewType.Visibility = Visibility.Collapsed;
            };
            sb.Children.Add(fade);
            sb.Begin();


            //btnNewType.Opacity = 1;
            //btnNewType.Visibility = Visibility.Visible;
            //FadeInButtonNewType(true);
            //TypeAnimTriggerd = false;
        }

        bool TypeAnimTriggerd = false;
        double ListHeight;

        private void tbNewType_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void btnSaveNewType_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (tbNewType.Text == "")
            {
                MessageBox.Show("File type cant be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            } else
            {
                SaveNew();
            }
        }

        private void SaveNew()
        {
            var _v = tbNewType.Text.Trim();
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
                    } catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Something went wrong! Exception: {0}", ex.InnerException.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    } finally
                    {
                        if (success)
                        {
                            MessageBox.Show(string.Format("{0} is now a registered file type! Click OK to set Perceived Type", newType), "New File Type Registered", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                            HideTextboxNewType();
                            var _newRegType = new RegType(newType, newType, "none");
                            data.Files.Add(_newRegType);
                            txtBox.Text = newType;
                            data.LastSelectedListItem = _newRegType;
                            radioNone.IsChecked = true;
                            data.Refresh();
                            listTypes.SelectedIndex = listTypes.Items.IndexOf(_newRegType);
                            
                        }
                    }
                } else
                {
                    MessageBox.Show(string.Format("{0} is already a registered file type", newType), "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void ReloadList()
        {
            //data.RegTypes.Clear();
            //data.FileteredTypes.Clear();
            //using (RegistryKey tempKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes"))
            //{
            //    foreach (var key in tempKey.GetSubKeyNames())
            //    {
            //        if (key != "")
            //        {
            //            using (RegistryKey keyValue = tempKey.OpenSubKey(key))
            //            {
            //                var finalKey = keyValue.GetValue("PerceivedType");
            //                if (key[0] == '.')
            //                {
            //                    var rk = new RegType(key, string.Join("\\", tempKey.Name, key), finalKey == null ? "" : finalKey.ToString());
            //                    data.RegTypes.Add(rk);
            //                    data.FileteredTypes.Add(rk);
            //                }
            //            }
            //        }
            //    }
            //}
        }

        private void contentGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CheckMouseHover();
        }

        private void contentGrid_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CheckMouseHover();
        }

        private void CheckMouseHover()
        {
            if (TypeAnimTriggerd)
            {
                if (!(gridBottomLeft.IsMouseOver || btnApply.IsMouseOver))
                {
                    HideTextboxNewType();
                    TypeAnimTriggerd = false;
                }
            }
        }
    }
}
