using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace Common.UI.Commands.Shortcus
{
    public class KeySequenceConverter : JsonConverter<KeySequence>
    {
        public override KeySequence Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            Key firstKey = Key.None;
            ModifierKeys firstModifiers = ModifierKeys.None;
            Key? secondKey = null;
            ModifierKeys? secondModifiers = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new KeySequence(firstKey, firstModifiers, secondKey, secondModifiers);
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                string propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case nameof(KeySequence.FirstKey):
                        firstKey = (Key)reader.GetInt32();
                        break;
                    case nameof(KeySequence.FirstModifiers):
                        firstModifiers = (ModifierKeys)reader.GetInt32();
                        break;
                    case nameof(KeySequence.SecondKey):
                        secondKey = (Key)reader.GetInt32();
                        break;
                    case nameof(KeySequence.SecondModifiers):
                        secondModifiers = (ModifierKeys)reader.GetInt32();
                        break;
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, KeySequence value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(nameof(KeySequence.FirstKey), (int)value.FirstKey);
            writer.WriteNumber(nameof(KeySequence.FirstModifiers), (int)value.FirstModifiers);
            if (value.SecondKey.HasValue)
            {
                writer.WriteNumber(nameof(KeySequence.SecondKey), (int)value.SecondKey.Value);
            }
            if (value.SecondModifiers.HasValue)
            {
                writer.WriteNumber(nameof(KeySequence.SecondModifiers), (int)value.SecondModifiers.Value);
            }
            writer.WriteEndObject();
        }
    }
}
