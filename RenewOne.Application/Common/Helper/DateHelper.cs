using System.Globalization;

namespace RenewOne.Application.Common.Helper
{
    public class DateHelper
    {
        public static DateOnly? ParseDateOnly(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            return DateOnly.TryParseExact(value, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
        }
    }
}
