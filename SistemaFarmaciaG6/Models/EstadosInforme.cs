using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class EstadosInforme
{
    public int IdEstado { get; set; }

    public string NombreEstado { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<InformeDepartamental> InformeDepartamentals { get; set; } = new List<InformeDepartamental>();

    public virtual ICollection<InformeDireccion> InformeDireccions { get; set; } = new List<InformeDireccion>();

    public virtual ICollection<InformeDocente> InformeDocentes { get; set; } = new List<InformeDocente>();

    public virtual ICollection<InformeFinalFacultad> InformeFinalFacultads { get; set; } = new List<InformeFinalFacultad>();
}
