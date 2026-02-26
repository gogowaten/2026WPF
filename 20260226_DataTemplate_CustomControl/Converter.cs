using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace _20260226_DataTemplate_CustomControl
{
    public class SolidColorBrushConverter : JsonConverter<SolidColorBrush>
    {
        public override SolidColorBrush? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var colorString = reader.GetString();
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorString));
        }

        public override void Write(Utf8JsonWriter writer, SolidColorBrush value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
