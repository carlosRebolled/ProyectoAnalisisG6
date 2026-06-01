using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class DetalleInformeFinal
{
    public int IdDetalleFinal { get; set; }

    public int IdInformeFinal { get; set; }

    public string TipoActividad { get; set; } = null!;

    public int? Cantidad { get; set; }

    public string? DetalleActividad { get; set; }

    public virtual InformeFinalFacultad IdInformeFinalNavigation { get; set; } = null!;
}
