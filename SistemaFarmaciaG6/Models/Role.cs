using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class Role
{
    public int IdRol { get; set; }

    public string NombreRol { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public virtual ICollection<UsuarioRol> UsuarioRols { get; set; } = new List<UsuarioRol>();
}
