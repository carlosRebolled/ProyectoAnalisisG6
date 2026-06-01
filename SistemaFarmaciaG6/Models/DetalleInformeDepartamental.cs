using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class DetalleInformeDepartamental
{
    public int IdDetalleDepartamental { get; set; }

    public int IdInformeDepartamental { get; set; }

    public string TipoActividad { get; set; } = null!;

    public int? Cantidad { get; set; }

    public string? DetalleActividad { get; set; }

    public virtual InformeDepartamental IdInformeDepartamentalNavigation { get; set; } = null!;
}
