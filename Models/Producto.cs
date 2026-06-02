using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public string? CodigoSunat { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? ImagenUrl { get; set; }

    public decimal PrecioUnitario { get; set; }

    public string IdAfectacionIgv { get; set; } = null!;

    public int? IdCategoria { get; set; }

    public int Stock { get; set; }

    public int StockMinimo { get; set; }

    public string UnidadMedida { get; set; } = null!;

    public bool EsAmenidad { get; set; }

    public bool EsVendibleEnTienda { get; set; }

    public int? StockPorHabitacion { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<HabitacionAmenidad> HabitacionAmenidades { get; set; } = new List<HabitacionAmenidad>();

    public virtual AfectacionIgv IdAfectacionIgvNavigation { get; set; } = null!;

    public virtual CategoriaProducto? IdCategoriaNavigation { get; set; }

    public virtual ICollection<ItemEstancia> ItemsEstancia { get; set; } = new List<ItemEstancia>();

    public virtual ICollection<ItemVentum> ItemVenta { get; set; } = new List<ItemVentum>();

    public virtual ICollection<MovimientoStock> MovimientoStocks { get; set; } = new List<MovimientoStock>();

    public virtual ICollection<StockHabitacion> StockHabitacions { get; set; } = new List<StockHabitacion>();

    // Convenience navigation properties
    public AfectacionIgv? AfectacionIgv => IdAfectacionIgvNavigation;
}
