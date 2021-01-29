using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace SiteMapUrlChecker.Converters
{
    public class StatusCodeConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((HttpStatusCode)value == HttpStatusCode.OK)
                return new SolidColorBrush(Colors.Green);
            else if ((HttpStatusCode)value == HttpStatusCode.InternalServerError)
                return new SolidColorBrush(Colors.Red);
            else if ((HttpStatusCode)value == HttpStatusCode.Moved)
                return new SolidColorBrush(Colors.LightGreen);
            else if ((HttpStatusCode)value == HttpStatusCode.MovedPermanently)
                return new SolidColorBrush(Colors.GreenYellow);
            else if ((HttpStatusCode)value == HttpStatusCode.NotFound)
                return new SolidColorBrush(Colors.Orange);

            return new SolidColorBrush();
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
