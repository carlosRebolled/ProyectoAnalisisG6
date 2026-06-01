using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class InformeFinalFacultad
{
    public int IdInformeFinal { get; set; }

    public int IdUsuarioGenera { get; set; }

    public int IdEstado { get; set; }

    public int Anio { get; set; }

    public DateTime FechaGeneracion { get; set; }

    public DateTime? FechaAprobacion { get; set; }

    public string? Observaciones { get; set; }

    public virtual ICollection<DetalleInformeFinal> DetalleInformeFinals { get; set; } = new List<DetalleInformeFinal>();

    public virtual EstadosInforme IdEstadoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioGeneraNavigation { get; set; } = null!;
}
