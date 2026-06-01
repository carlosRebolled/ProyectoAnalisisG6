using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class InformeDepartamental
{
    public int IdInformeDepartamental { get; set; }

    public int IdDirector { get; set; }

    public int IdDepartamento { get; set; }

    public int IdEstado { get; set; }

    public int Anio { get; set; }

    public DateTime FechaGeneracion { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public DateTime? FechaAprobacion { get; set; }

    public string? ObservacionesDirector { get; set; }

    public virtual ICollection<DetalleInformeDepartamental> DetalleInformeDepartamentals { get; set; } = new List<DetalleInformeDepartamental>();

    public virtual Departamento IdDepartamentoNavigation { get; set; } = null!;

    public virtual Usuario IdDirectorNavigation { get; set; } = null!;

    public virtual EstadosInforme IdEstadoNavigation { get; set; } = null!;
}
