namespace HotelGenericoApi.Helpers;

public static class MontoEnLetrasHelper
{
    private static readonly string[] Unidades = ["", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE"];
    private static readonly string[] Dieces = ["", "DIEZ", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA"];
    private static readonly string[] OnceAVeinte = ["ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISEIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE"];
    private static readonly string[] Centenas = ["", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS"];

    public static string Convertir(decimal monto)
    {
        long entero = (long)Math.Floor(monto);
        int centimos = (int)Math.Round((monto - entero) * 100);

        string enteroLetras = entero switch
        {
            0 => "CERO",
            1 => "UN",
            _ => ConvertirEntero(entero)
        };

        return $"{enteroLetras} CON {centimos:D2}/100 SOLES";
    }

    private static string ConvertirEntero(long n)
    {
        if (n < 10) return Unidades[n];
        if (n < 20) return n == 10 ? "DIEZ" : OnceAVeinte[n - 11];
        if (n < 100)
        {
            long d = n / 10;
            long u = n % 10;
            string decena = n switch
            {
                20 => "VEINTI",
                21 => "VEINTIUN",
                22 => "VEINTIDOS",
                23 => "VEINTITRES",
                24 => "VEINTICUATRO",
                25 => "VEINTICINCO",
                26 => "VEINTISEIS",
                27 => "VEINTISIETE",
                28 => "VEINTIOCHO",
                29 => "VEINTINUEVE",
                _ => Dieces[d]
            };
            if (n >= 20 && n <= 29) return decena;
            if (u == 0) return decena;
            return $"{decena} Y {Unidades[u]}";
        }
        if (n < 1000)
        {
            long c = n / 100;
            long r = n % 100;
            string centena = n == 100 ? "CIEN" : Centenas[c];
            if (r == 0) return centena;
            return $"{centena} {ConvertirEntero(r)}";
        }
        if (n < 1_000_000)
        {
            long m = n / 1000;
            long r = n % 1000;
            string miles = m == 1 ? "MIL" : $"{ConvertirEntero(m)} MIL";
            if (r == 0) return miles;
            return $"{miles} {ConvertirEntero(r)}";
        }
        if (n < 1_000_000_000)
        {
            long m = n / 1_000_000;
            long r = n % 1_000_000;
            string millones = m == 1 ? "UN MILLON" : $"{ConvertirEntero(m)} MILLONES";
            if (r == 0) return millones;
            return $"{millones} {ConvertirEntero(r)}";
        }

        return n.ToString();
    }
}
