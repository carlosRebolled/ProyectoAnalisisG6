using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Cedula { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellido1 { get; set; } = null!;

    public string? Apellido2 { get; set; }

    public DateOnly FechaNacimiento { get; set; }

    public string Correo { get; set; } = null!;

    public string Contrasena { get; set; } = null!;

    public string? Telefono { get; set; }

    public int IdGenero { get; set; }

    public int IdDepartamento { get; set; }

    public int IdCategoria { get; set; }

    public int IdTipoNombramiento { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public bool CambiarContrasena { get; set; }

    public DateTime? UltimoInicioSesion { get; set; }

    public virtual ICollection<Auditorium> Auditoria { get; set; } = new List<Auditorium>();

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual Departamento IdDepartamentoNavigation { get; set; } = null!;

    public virtual Genero IdGeneroNavigation { get; set; } = null!;

    public virtual TiposNombramiento IdTipoNombramientoNavigation { get; set; } = null!;

    public virtual ICollection<InformeDepartamental> InformeDepartamentals { get; set; } = new List<InformeDepartamental>();

    public virtual ICollection<InformeDireccion> InformeDireccions { get; set; } = new List<InformeDireccion>();

    public virtual ICollection<InformeDocente> InformeDocentes { get; set; } = new List<InformeDocente>();

    public virtual ICollection<InformeFinalFacultad> InformeFinalFacultads { get; set; } = new List<InformeFinalFacultad>();

    public virtual ICollection<Notificacione> Notificaciones { get; set; } = new List<Notificacione>();

    public virtual ICollection<Observacione> Observaciones { get; set; } = new List<Observacione>();

    public virtual ICollection<UsuarioRol> UsuarioRols { get; set; } = new List<UsuarioRol>();
}
