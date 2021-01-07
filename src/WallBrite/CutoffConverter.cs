using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WallBrite
{
    /// <summary>
    /// Checks if given value is > Cutoff; returns true if it is, false if not
    /// For use in xaml data triggers for greater than inequalities, rather than just equalities
    /// Taken from https://stackoverflow.com/questions/793926/how-to-get-datatemplate-datatrigger-to-check-for-greater-than-or-less-than
    /// </summary>
    public class CutoffConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return (int)value > Cutoff;
                
            } else
            {
                return false;
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public int Cutoff { get; set; }
    }
}
