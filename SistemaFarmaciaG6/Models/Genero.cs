using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class Genero
{
    public int IdGenero { get; set; }

    public string NombreGenero { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
