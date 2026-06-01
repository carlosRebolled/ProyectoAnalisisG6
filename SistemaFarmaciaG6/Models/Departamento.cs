using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class Departamento
{
    public int IdDepartamento { get; set; }

    public string NombreDepartamento { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public virtual ICollection<InformeDepartamental> InformeDepartamentals { get; set; } = new List<InformeDepartamental>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
