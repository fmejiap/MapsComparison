using System.Text.RegularExpressions;
using Maps.Cartociudad;
namespace Maps
{
    public sealed class CartoCiudadRouteConverter
    {
        public static string Convert(string address, string portalNumber)
        {
            string result;

            var splitResult = address.Split(',');
            if (splitResult.Length > 1)
            {
                result = Regex.Replace(splitResult[0], "[" + portalNumber.Trim() + "]+", "").Trim();
            }
            else
            {
                result = address;
            }

            return result;
        }

    }
    public sealed class UpperCaseConverter
    {
        public static string Convert(string value)
        {
            value = value.ToLower();

            char[] array = value.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }
    }
    public sealed class CartoCiudadToGeocodeCountryConverter
    {
        public static string CountryConvert(string source)
        {
            var result = string.Empty;
            if (source.Equals("011"))
                result = "España";

            return result;
        }
        public static string CountryCodeConvert(string source)
        {
            var result = string.Empty;
            if (source.Equals("011"))
                result = "ES";

            return result;
        }
    }

    public sealed class CartociudadToGeocodeStatusConverter
    {
        public static AddressStatusType Convert(CartociudadState source)
        {
            var result = AddressStatusType.PENDING;
            switch (source)
            {
                case CartociudadState.ValueOne:
                    result = AddressStatusType.OK;
                    break;
                case CartociudadState.ValueTwo:
                case CartociudadState.ValueThree:
                case CartociudadState.ValueFour:
                case CartociudadState.ValueFive:
                case CartociudadState.ValueTen:
                    result = AddressStatusType.TO_CHECK;
                    break;
                case CartociudadState.ValueSix:
                    result = AddressStatusType.ZERO_RESULTS;
                    break;
            }
            return result;
        }
    }
}
