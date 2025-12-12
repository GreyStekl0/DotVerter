using System.Text.Json.Serialization;

namespace Data.Dto;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(CbrResponseDto))]
internal partial class CbrJsonContext : JsonSerializerContext
{
}

