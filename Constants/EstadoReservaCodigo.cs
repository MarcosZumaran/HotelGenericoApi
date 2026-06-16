namespace HotelGenericoApi.Constants;

public static class EstadoReservaCodigo
{
    public const int Pendiente = 1;
    public const int Confirmada = 2;
    public const int Cancelada = 3;
    public const int Vencida = 4;
    public const int Completa = 5;

    public static class Code
    {
        public const string Pendiente = "Pendiente";
        public const string Confirmada = "Confirmada";
        public const string Cancelada = "Cancelada";
        public const string Vencida = "Vencida";
        public const string Completa = "Completa";
    }
}
