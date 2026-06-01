using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class Auditorium
{
    public int IdAuditoria { get; set; }

    public int IdUsuario { get; set; }

    public string TablaAfectada { get; set; } = null!;

    public string Accion { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime Fecha { get; set; }

    public string? IpEquipo { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
