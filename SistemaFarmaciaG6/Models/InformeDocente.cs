using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class InformeDocente
{
    public int IdInformeDocente { get; set; }

    public int IdUsuario { get; set; }

    public int IdEstado { get; set; }

    public int Anio { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public DateTime? FechaAprobacion { get; set; }

    public int? CantidadCongresosActivos { get; set; }

    public string? DetalleCongresosActivos { get; set; }

    public int? CantidadCongresosPasivos { get; set; }

    public string? DetalleCongresosPasivos { get; set; }

    public int? CantidadAccionSocial { get; set; }

    public string? DetalleAccionSocial { get; set; }

    public int? CantidadInvestigacion { get; set; }

    public string? DetalleInvestigacion { get; set; }

    public int? CantidadDocencia { get; set; }

    public string? DetalleDocencia { get; set; }

    public int? CantidadPublicaciones { get; set; }

    public string? DetallePublicaciones { get; set; }

    public int? CantidadCursosGrado { get; set; }

    public string? DetalleCursosGrado { get; set; }

    public int? CantidadPosgrado { get; set; }

    public string? DetallePosgrado { get; set; }

    public int? CantidadRepresentacion { get; set; }

    public string? DetalleRepresentacion { get; set; }

    public string? DetalleOtros { get; set; }

    public string? ObservacionesDocente { get; set; }

    public virtual EstadosInforme IdEstadoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
