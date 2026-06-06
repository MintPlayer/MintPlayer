using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MintPlayer.Fetcher.Converters
{
    public class MultiDateFormatConverter : JsonConverter
    {
        public List<string> DateTimeFormats { get; set; }

        public MultiDateFormatConverter()
        {
            //DateTimeFormats = formats.Select(f => (string)f).ToList();
            DateTimeFormats = new List<string>() { "yyyy-MM-dd", "yyyy", "" };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        #region Read
        public override bool CanRead => true;
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string dateString = (string)reader.Value;

            if (string.IsNullOrEmpty(dateString)) return null;

            DateTime date;
            foreach (string format in DateTimeFormats)
            {
                // adjust this as necessary to fit your needs
                if (DateTime.TryParseExact(dateString, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                    return date;
            }
            throw new JsonException("Unable to parse \"" + dateString + "\" as a date.");
        }
        #endregion
        #region Write
        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
