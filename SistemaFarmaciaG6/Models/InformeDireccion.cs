using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class InformeDireccion
{
    public int IdInformeDireccion { get; set; }

    public int IdUsuario { get; set; }

    public int IdEstado { get; set; }

    public int Anio { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public DateTime? FechaAprobacion { get; set; }

    public string? ObservacionesDirector { get; set; }

    public virtual ICollection<CursosDireccion> CursosDireccions { get; set; } = new List<CursosDireccion>();

    public virtual EstadosInforme IdEstadoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<SesionesDepartamento> SesionesDepartamentos { get; set; } = new List<SesionesDepartamento>();
}
