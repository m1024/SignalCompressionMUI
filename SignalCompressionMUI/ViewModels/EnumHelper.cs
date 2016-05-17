using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;
using System.Reflection;

namespace SignalCompressionMUI.ViewModels
{
    public class EnumToItemsSource : MarkupExtension
    {
        private readonly Type _type;

        public EnumToItemsSource(Type type)
        {
            _type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _type.GetMembers().SelectMany(member => member.GetCustomAttributes(typeof(DescriptionAttribute), true).Cast<DescriptionAttribute>()).Select(x => x.Description).ToList();
        }
    }

    public class EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            foreach (var one in Enum.GetValues(parameter as Type))
            {
                if (value.Equals(one))
                    return GetEnumDescription(one as Enum);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            foreach (var one in Enum.GetValues(parameter as Type))
            {
                if (value.ToString() == GetEnumDescription(one as Enum))
                    return one;
            }
            return null;
        }

        public static string GetEnumDescription(Enum enumValue)
        {
            string enumValueAsString = enumValue.ToString();

            var type = enumValue.GetType();
            FieldInfo fieldInfo = type.GetField(enumValueAsString);
            object[] attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
            {
                var attribute = (DescriptionAttribute)attributes[0];
                return attribute.Description;
            }

            return enumValueAsString;
        }
    }
}
