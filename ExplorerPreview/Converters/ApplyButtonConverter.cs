using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace ExplorerPreview.Converters
{
    class ApplyButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            var btnsControl = System.Windows.Application.Current.MainWindow.FindName("btnsItemsControl") as ItemsControl;
            var textboxCustom = System.Windows.Application.Current.MainWindow.FindName("tbCustom") as TextBox;

            string pbValue = null;
            for (int i = 0; i < btnsControl.Items.Count; i++)
            {
                RadioButton btn = (RadioButton)btnsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (btn.IsChecked == true)
                {
                    pbValue = btn.Content.GetType().Equals(typeof(TextBox)) ? textboxCustom.Text : btn.Content.ToString();
                    break;
                }
                //if (btn.Content.ToString().ToLower() == ((RegType)value).PercievedType.ToLower()) {
                //    pbValue = btn.Content.GetType().Equals(typeof(TextBox)) ? textboxCustom.Text : btn.Content.ToString();
                //    break;
                //}
            }
            
            return (pbValue == ((RegType)value).PercievedType.ToLower()) ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
