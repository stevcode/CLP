using System;
using System.ComponentModel;

namespace CLP.Entities
{
    public static class EnumExtensions
    {
        public static string ToDescription<T>(this T enumValue) where T : struct
        {
            var type = enumValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException($"{nameof(enumValue)} must be of Enum type", nameof(enumValue));
            }

            var memberInfo = type.GetMember(enumValue.ToString());
            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return enumValue.ToString();
        }
    }
}