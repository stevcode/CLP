using System;
using System.Globalization;
using System.Windows.Data;

namespace Classroom_Learning_Partner.Converters
{
    public class BoolZeroToQuestionMarkMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var val = (int)values[0];
                var showZero = (bool)values[1];
                return !showZero && val == 0 ? "?" : val.ToString(CultureInfo.InvariantCulture);
            }
            catch(Exception)
            {
                return "?";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
