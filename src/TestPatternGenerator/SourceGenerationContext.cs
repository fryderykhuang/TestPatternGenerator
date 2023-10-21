using System.Text.Json.Serialization;

namespace TestPatternGenerator;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(UserSettings))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}