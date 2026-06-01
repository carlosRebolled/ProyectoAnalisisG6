using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class Observacione
{
    public int IdObservacion { get; set; }

    public int? IdInformeDocente { get; set; }

    public int? IdInformeDireccion { get; set; }

    public int IdUsuarioRealiza { get; set; }

    public string Comentario { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public string? TipoObservacion { get; set; }

    public virtual Usuario IdUsuarioRealizaNavigation { get; set; } = null!;
}
