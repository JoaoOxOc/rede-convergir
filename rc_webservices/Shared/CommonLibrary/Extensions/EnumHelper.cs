using System.ComponentModel;

namespace CommonLibrary.Extensions
{
    public static class EnumHelper
    {
        public static string GetDisplayValue<TEnum>(this TEnum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var descriptionAttr = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            return descriptionAttr[0].Description;
        }
    }
}
