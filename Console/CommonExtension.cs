using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace Console
{
    public static class CommonExtension
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        #region Formatting

        /// <summary>
        ///     A DbEntityValidationException extension method that formates validation errors to string.
        /// </summary>
        public static string DbEntityValidationExceptionToString(this DbEntityValidationException e)
        {
            var validationErrors = e.DbEntityValidationResultToString();
            var exceptionMessage = string.Format("{0}{1}Validation errors:{1}{2}", e, Environment.NewLine, validationErrors);
            return exceptionMessage;
        }

        /// <summary>
        ///     A DbEntityValidationException extension method that aggregate database entity validation results to string.
        /// </summary>
        public static string DbEntityValidationResultToString(this DbEntityValidationException e)
        {
            return e.EntityValidationErrors
                    .Select(dbEntityValidationResult => dbEntityValidationResult.DbValidationErrorsToString(dbEntityValidationResult.ValidationErrors))
                    .Aggregate(string.Empty, (current, next) => string.Format("{0}{1}{2}", current, Environment.NewLine, next));
        }

        /// <summary>
        ///     A DbEntityValidationResult extension method that to strings database validation errors.
        /// </summary>
        public static string DbValidationErrorsToString(this DbEntityValidationResult dbEntityValidationResult, IEnumerable<DbValidationError> dbValidationErrors)
        {
            var entityName = string.Format("[{0}]", dbEntityValidationResult.Entry.Entity.GetType().Name);
            const string indentation = "\t - ";
            var aggregatedValidationErrorMessages = dbValidationErrors.Select(error => string.Format("[{0} - {1}]", error.PropertyName, error.ErrorMessage))
                                                   .Aggregate(string.Empty, (current, validationErrorMessage) => current + (Environment.NewLine + indentation + validationErrorMessage));
            return string.Format("{0}{1}", entityName, aggregatedValidationErrorMessages);
        }

        public static string Log(this Object obj)
        {
            try
            {
                return new JavaScriptSerializer().Serialize(obj);
            }
            catch (Exception ex)
            {
                //return string.Format("{0}",ex);
                logger.InfoFormat(ex.ToString());
                return "";
            }
        }

        public static string GetTime()
        {
            return DateTime.Now.ToString("HHmmss");
        }

        public static string GetRandomString()
        {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", "");
            return path;
        }

        public static string ConvertDateTimeToDetailedString(this DateTime value)
        {
            return value.ToString("yyyyMMddhhmmss");
        }

        public static string ConvertDateTimeToString(this DateTime? value)
        {
            if (value.HasValue)
                return value.Value.ToString("dd/MM/yyyy hh:mm:ss tt");

            return "";
        }

        public static string ConvertDateTimeToString(this DateTime value)
        {
            return value.ToString("dd/MM/yyyy hh:mm:ss tt");
        }

        public static string ConvertDateToString(this DateTime? value)
        {
            if (value.HasValue)
                return value.Value.ToString("dd/MM/yyyy");

            return "";
        }

        public static string ConvertDateToString(this DateTime value)
        {
            return value.ToString("dd/MM/yyyy");
        }

        public static string ConvertTimeToString(this DateTime value)
        {
            return value.ToString("HHmmss");
        }

        public static string ConvertTimeSpanToDays(this TimeSpan value)
        {
            return string.Format("{0:D2} days, {1:D2} hrs, {2:D2} mins", value.Days, value.Hours, value.Minutes);
        }

        public static string ConvertDecimalsToCurrency(this decimal value)
        {
            return $"RM {value.ToString()}";
        }

        public static string ConvertDecimalToString(this decimal? value)
        {
            if (value.HasValue)
                return Convert.ToDecimal(Math.Round(value.Value, 2)).ToString();

            return "0.00";
        }

        public static string ConvertDecimalToString(this decimal value)
        {
            return Convert.ToDecimal(Math.Round(value, 2)).ToString();
        }

        public static string RoundDecimalToOneDecimalPlaceAsString(this decimal value)
        {
            return $"{value.ToString("0.#")}";
        }

        public static string ConvertBoolToFlag(this bool value)
        {
            return value ? "Yes" : "No";
        }

        public static int ToInt(this string value)
        {
            return Convert.ToInt32(value);
        }

        public static int ToInt(this int? value)
        {
            return value.HasValue ? Convert.ToInt32(value) : 0;
        }

        public static double ToDouble(this decimal value)
        {
            return Convert.ToDouble(value);
        }

        public static double ToDouble(this string value)
        {
            return Convert.ToDouble(value);
        }

        public static string ToString2(this object value)
        {
            return value == null ? "" : value.ToString();
        }

        public static string FormatBooleanFlag(this bool value)
        {
            return value ? "Yes" : "No";
        }

        public static T TryGetElement<T>(this T[] array, int index, T defaultElement)
        {
            if (index < array.Length)
                return array[index];

            return defaultElement;
        }

        public static string ToEnumString<T>(this T instance)
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("instance", "Must be enum type");

            var enumString = instance.ToString();
            var field = typeof(T).GetField(enumString);

            if (field != null) // instance can be a number that was cast to T, instead of a named value, or could be a combination of flags instead of a single value
            {
                var attr = (EnumMemberAttribute)field.GetCustomAttributes(typeof(EnumMemberAttribute), false).SingleOrDefault();
                if (attr != null) // if there's no EnumMember attr, use the default value
                    enumString = attr.Value;
            }

            return enumString;
        }

        public static decimal ToOneDecimals(this decimal value)
        {
            return decimal.Round(value, 1);
        }

        public static decimal ToTwoDecimals(this decimal value)
        {
            return decimal.Round(value, 2);
        }

        public static decimal ToTwoDecimals(this int value)
        {
            return decimal.Round(value, 2);
        }

        public static DateTime CalculateFutureDateTime(DateTime dateStart, decimal hours)
        {
            return dateStart.AddHours((double)hours);
        }

        public static string ConvertIntegerToOrdinal(this int number)
        {
            int ones = number % 10;
            int tens = (int)Math.Floor(number / 10M) % 10;

            string suffix;
            if (tens == 1)
            {
                suffix = "th";
            }
            else
            {
                switch (ones)
                {
                    case 1:
                        suffix = "st";
                        break;

                    case 2:
                        suffix = "nd";
                        break;

                    case 3:
                        suffix = "rd";
                        break;

                    default:
                        suffix = "th";
                        break;
                }
            }

            return String.Format("{0}{1}", number, suffix);
        }

        #endregion

        #region Checking

        public static bool IsEmpty(this object value)
        {
            return value == null;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> value)
        {
            return (value == null || value.Count() == 0);
        }

        public static bool IsEmpty(this byte[] value)
        {
            return (value == null || value.Length == 0);
        }

        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool IsEmpty(this int? value)
        {
            return !(value.HasValue && value.Value > 0);
        }

        public static bool IsEmpty(this long? value)
        {
            return !(value.HasValue && value.Value > 0);
        }

        public static bool IsEmpty(this DateTime? value)
        {
            return !value.HasValue;
        }

        public static bool IsEmpty(this HttpPostedFileBase file)
        {
            return !(file != null || file?.ContentLength > 0);
        }

        public static bool HasSpecialChar(this string value)
        {
            var regexItem = new Regex("^[a-zA-Z0-9 ]*$");
            return !regexItem.IsMatch(value);
        }

        public static bool IsValidInt(this string value)
        {
            ulong number;
            return UInt64.TryParse(value, out number);
        }

        public static bool HasValue(this int? value)
        {
            return value.HasValue && value.Value > 0;
        }

        public static bool IsFileUploadDate(string format, string value)
        {
            DateTime temp;
            return DateTime.TryParseExact(value, format, null, DateTimeStyles.None, out temp);
        }

        public static bool IsValidDecimal(this string value)
        {
            decimal number;
            return Decimal.TryParse(value, out number);
        }

        #endregion
    }
}
