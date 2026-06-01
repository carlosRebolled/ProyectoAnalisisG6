using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class Notificacione
{
    public int IdNotificacion { get; set; }

    public int IdUsuario { get; set; }

    public string Titulo { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public string Leida { get; set; } = null!;

    public string? TipoNotificacion { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
