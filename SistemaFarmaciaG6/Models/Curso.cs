using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class Curso
{
    public int IdCurso { get; set; }

    public string SiglaCurso { get; set; } = null!;

    public string NombreCurso { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public virtual ICollection<CursosDireccion> CursosDireccions { get; set; } = new List<CursosDireccion>();

}