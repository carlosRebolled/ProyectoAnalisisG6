using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class TiposNombramiento
{
    public int IdTipoNombramiento { get; set; }

    public string NombreNombramiento { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
