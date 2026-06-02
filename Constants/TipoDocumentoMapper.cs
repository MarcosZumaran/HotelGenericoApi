namespace HotelGenericoApi.Constants;

public static class TipoDocumentoMapper
{
    private static readonly Dictionary<string, string> DisplayToCode = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DNI"] = "1",
        ["RUC"] = "6",
        ["PASAPORTE"] = "7",
        ["OTROS"] = "0",
    };

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "0";

        if (value.Length == 1 && char.IsDigit(value[0]))
            return value;

        return DisplayToCode.TryGetValue(value.Trim(), out var code) ? code : "0";
    }
}
