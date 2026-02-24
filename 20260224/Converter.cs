using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace _20260224
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

    //public class SolidColorBrushConverter : JsonConverter<SolidColorBrush>
    //{
    //    public override SolidColorBrush? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        var colorString = reader.GetString();
    //        if (string.IsNullOrEmpty(colorString)) return null;

    //        // 文字列からBrushを復元
    //        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorString));
    //    }

    //    public override void Write(Utf8JsonWriter writer, SolidColorBrush value, JsonSerializerOptions options)
    //    {
    //        // Brushオブジェクトの中身を探索せず、".ToString()" の結果（文字列）だけを書き出す
    //        // これにより無限ループを回避できます
    //        writer.WriteStringValue(value.ToString());
    //    }
    //}
}
