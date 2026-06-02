using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaFarmaciaG6.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }


    [Required(ErrorMessage = "La cédula es obligatoria.")]
    public string Cedula { get; set; } = null!;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El primer apellido es obligatorio.")]
    public string Apellido1 { get; set; } = null!;

    public string? Apellido2 { get; set; }

    public DateOnly FechaNacimiento { get; set; }
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    public string Correo { get; set; } = null!;
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
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
