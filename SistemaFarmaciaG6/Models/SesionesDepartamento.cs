using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class SesionesDepartamento
{
    public int IdSesion { get; set; }

    public int IdInformeDireccion { get; set; }

    public string NumeroSesion { get; set; } = null!;

    public DateOnly FechaSesion { get; set; }

    public string PuntosVistos { get; set; } = null!;

    public virtual InformeDireccion IdInformeDireccionNavigation { get; set; } = null!;
}
