using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SprayTeaching.BaseClassLib
{
    /// <summary>
    /// 将Radio转换为字符串类型
    /// </summary>
    public class MyRadioConverter : IValueConverter
    {
        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            string checkvalue = value.ToString();
            string targetvalue = parameter.ToString();
            bool r = checkvalue.Equals(targetvalue, StringComparison.InvariantCultureIgnoreCase);
            return r;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;
            bool usevalue = (bool)value;
            if (usevalue)
                return parameter.ToString();
            return null;
        }

        #endregion        
    }

    /// <summary>
    /// 取反类型转换
    /// </summary>
    public class MyNegativeConverte : IValueConverter
    {

        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is bool ? !(bool)value : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }

        #endregion
    }
}
