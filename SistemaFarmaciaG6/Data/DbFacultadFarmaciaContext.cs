using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaG6.Models;

namespace SistemaFarmaciaG6.Data;

public partial class DbFacultadFarmaciaContext : DbContext
{
    public DbFacultadFarmaciaContext()
    {
    }

    public DbFacultadFarmaciaContext(DbContextOptions<DbFacultadFarmaciaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Auditorium> Auditoria { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<CursosDireccion> CursosDireccions { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<DetalleInformeDepartamental> DetalleInformeDepartamentals { get; set; }

    public virtual DbSet<DetalleInformeFinal> DetalleInformeFinals { get; set; }

    public virtual DbSet<EstadosInforme> EstadosInformes { get; set; }

    public virtual DbSet<Genero> Generos { get; set; }

    public virtual DbSet<InformeDepartamental> InformeDepartamentals { get; set; }

    public virtual DbSet<InformeDireccion> InformeDireccions { get; set; }

    public virtual DbSet<InformeDocente> InformeDocentes { get; set; }

    public virtual DbSet<InformeFinalFacultad> InformeFinalFacultads { get; set; }

    public virtual DbSet<Notificacione> Notificaciones { get; set; }

    public virtual DbSet<Observacione> Observaciones { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SesionesDepartamento> SesionesDepartamentos { get; set; }

    public virtual DbSet<TiposNombramiento> TiposNombramientos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuarioRol> UsuarioRols { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=(local);Database=DB_FacultadFarmacia;Integrated Security=true;Encrypt=False;TrustServerCertificate=True");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Name=DB_FacultadFarmacia");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auditorium>(entity =>
        {
            entity.HasKey(e => e.IdAuditoria).HasName("PK__Auditori__9644A3CE73AC203B");

            entity.Property(e => e.IdAuditoria).HasColumnName("id_auditoria");
            entity.Property(e => e.Accion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("accion");
            entity.Property(e => e.Descripcion)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IpEquipo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ip_equipo");
            entity.Property(e => e.TablaAfectada)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("tabla_afectada");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Auditoria)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auditoria_Usuarios");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__Categori__CD54BC5AEFB05AE2");

            entity.HasIndex(e => e.NombreCategoria, "UQ__Categori__4EBF6259E40A9AE8").IsUnique();

            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Activo")
                .HasColumnName("estado");
            entity.Property(e => e.NombreCategoria)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre_categoria");
        });

        modelBuilder.Entity<CursosDireccion>(entity =>
        {
            entity.HasKey(e => e.IdCursoDireccion).HasName("PK__CursosDi__601FA8F5C7E94B99");

            entity.ToTable("CursosDireccion");

            entity.Property(e => e.IdCursoDireccion).HasColumnName("id_curso_direccion");
            entity.Property(e => e.ActividadesAnalisisContextoCantidad)
                .HasDefaultValue(0)
                .HasColumnName("actividades_analisis_contexto_cantidad");
            entity.Property(e => e.ActividadesAnalisisContextoDetalle)
                .IsUnicode(false)
                .HasColumnName("actividades_analisis_contexto_detalle");
            entity.Property(e => e.ActividadesDocenciaIntegradasCantidad)
                .HasDefaultValue(0)
                .HasColumnName("actividades_docencia_integradas_cantidad");
            entity.Property(e => e.ActividadesDocenciaIntegradasDetalle)
                .IsUnicode(false)
                .HasColumnName("actividades_docencia_integradas_detalle");
            entity.Property(e => e.AsistentesCurso)
                .HasDefaultValue(0)
                .HasColumnName("asistentes_curso");
            entity.Property(e => e.ColaboradoresCantidad)
                .HasDefaultValue(0)
                .HasColumnName("colaboradores_cantidad");
            entity.Property(e => e.ColaboradoresDetalle)
                .IsUnicode(false)
                .HasColumnName("colaboradores_detalle");
            entity.Property(e => e.CoordinacionCantidad)
                .HasDefaultValue(0)
                .HasColumnName("coordinacion_cantidad");
            entity.Property(e => e.CoordinacionDetalle)
                .IsUnicode(false)
                .HasColumnName("coordinacion_detalle");
            entity.Property(e => e.ExperienciasPracticasCantidad)
                .HasDefaultValue(0)
                .HasColumnName("experiencias_practicas_cantidad");
            entity.Property(e => e.ExperienciasPracticasDetalle)
                .IsUnicode(false)
                .HasColumnName("experiencias_practicas_detalle");
            entity.Property(e => e.IdInformeDireccion).HasColumnName("id_informe_direccion");
            entity.Property(e => e.InvitadosCantidad)
                .HasDefaultValue(0)
                .HasColumnName("invitados_cantidad");
            entity.Property(e => e.InvitadosDetalle)
                .IsUnicode(false)
                .HasColumnName("invitados_detalle");
            entity.Property(e => e.NombreCurso)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nombre_curso");
            entity.Property(e => e.SiglaCurso)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("sigla_curso");
            entity.Property(e => e.TecnicasDidacticasCantidad)
                .HasDefaultValue(0)
                .HasColumnName("tecnicas_didacticas_cantidad");
            entity.Property(e => e.TecnicasDidacticasDetalle)
                .IsUnicode(false)
                .HasColumnName("tecnicas_didacticas_detalle");

            entity.HasOne(d => d.IdInformeDireccionNavigation).WithMany(p => p.CursosDireccions)
                .HasForeignKey(d => d.IdInformeDireccion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CursosDireccion_InformeDireccion");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.IdDepartamento).HasName("PK__Departam__64F37A1614D3C9F6");

            entity.HasIndex(e => e.NombreDepartamento, "UQ__Departam__D90350F94915EEF9").IsUnique();

            entity.Property(e => e.IdDepartamento).HasColumnName("id_departamento");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Activo")
                .HasColumnName("estado");
            entity.Property(e => e.NombreDepartamento)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("nombre_departamento");
        });

        modelBuilder.Entity<DetalleInformeDepartamental>(entity =>
        {
            entity.HasKey(e => e.IdDetalleDepartamental).HasName("PK__DetalleI__B3B3EC1CD7FB6327");

            entity.ToTable("DetalleInformeDepartamental");

            entity.Property(e => e.IdDetalleDepartamental).HasColumnName("id_detalle_departamental");
            entity.Property(e => e.Cantidad)
                .HasDefaultValue(0)
                .HasColumnName("cantidad");
            entity.Property(e => e.DetalleActividad)
                .IsUnicode(false)
                .HasColumnName("detalle_actividad");
            entity.Property(e => e.IdInformeDepartamental).HasColumnName("id_informe_departamental");
            entity.Property(e => e.TipoActividad)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("tipo_actividad");

            entity.HasOne(d => d.IdInformeDepartamentalNavigation).WithMany(p => p.DetalleInformeDepartamentals)
                .HasForeignKey(d => d.IdInformeDepartamental)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleInformeDepartamental");
        });

        modelBuilder.Entity<DetalleInformeFinal>(entity =>
        {
            entity.HasKey(e => e.IdDetalleFinal).HasName("PK__DetalleI__6DBE438A40E1FAAC");

            entity.ToTable("DetalleInformeFinal");

            entity.Property(e => e.IdDetalleFinal).HasColumnName("id_detalle_final");
            entity.Property(e => e.Cantidad)
                .HasDefaultValue(0)
                .HasColumnName("cantidad");
            entity.Property(e => e.DetalleActividad)
                .IsUnicode(false)
                .HasColumnName("detalle_actividad");
            entity.Property(e => e.IdInformeFinal).HasColumnName("id_informe_final");
            entity.Property(e => e.TipoActividad)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("tipo_actividad");

            entity.HasOne(d => d.IdInformeFinalNavigation).WithMany(p => p.DetalleInformeFinals)
                .HasForeignKey(d => d.IdInformeFinal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleInformeFinal");
        });

        modelBuilder.Entity<EstadosInforme>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PK__EstadosI__86989FB2D16E1870");

            entity.ToTable("EstadosInforme");

            entity.HasIndex(e => e.NombreEstado, "UQ__EstadosI__2F8C63750843ACF2").IsUnique();

            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.NombreEstado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre_estado");
        });

        modelBuilder.Entity<Genero>(entity =>
        {
            entity.HasKey(e => e.IdGenero).HasName("PK__Generos__99A8E4F9E715C4A3");

            entity.HasIndex(e => e.NombreGenero, "UQ__Generos__50D32CEF6EC01A76").IsUnique();

            entity.Property(e => e.IdGenero).HasColumnName("id_genero");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Activo")
                .HasColumnName("estado");
            entity.Property(e => e.NombreGenero)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre_genero");
        });

        modelBuilder.Entity<InformeDepartamental>(entity =>
        {
            entity.HasKey(e => e.IdInformeDepartamental).HasName("PK__InformeD__5FE8B6DAFF6D0752");

            entity.ToTable("InformeDepartamental");

            entity.Property(e => e.IdInformeDepartamental).HasColumnName("id_informe_departamental");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.FechaAprobacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_aprobacion");
            entity.Property(e => e.FechaEnvio)
                .HasColumnType("datetime")
                .HasColumnName("fecha_envio");
            entity.Property(e => e.FechaGeneracion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_generacion");
            entity.Property(e => e.IdDepartamento).HasColumnName("id_departamento");
            entity.Property(e => e.IdDirector).HasColumnName("id_director");
            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.ObservacionesDirector)
                .IsUnicode(false)
                .HasColumnName("observaciones_director");

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.InformeDepartamentals)
                .HasForeignKey(d => d.IdDepartamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InformeDepartamental_Departamentos");

            entity.HasOne(d => d.IdDirectorNavigation).WithMany(p => p.InformeDepartamentals)
                .HasForeignKey(d => d.IdDirector)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InformeDepartamental_Usuarios");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.InformeDepartamentals)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InformeDepartamental_Estados");
        });

        modelBuilder.Entity<InformeDireccion>(entity =>
        {
            entity.HasKey(e => e.IdInformeDireccion).HasName("PK__InformeD__469014509539ED47");

            entity.ToTable("InformeDireccion");

            entity.Property(e => e.IdInformeDireccion).HasColumnName("id_informe_direccion");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.FechaAprobacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_aprobacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaEnvio)
                .HasColumnType("datetime")
                .HasColumnName("fecha_envio");
            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.ObservacionesDirector)
                .IsUnicode(false)
                .HasColumnName("observaciones_director");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.InformeDireccions)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InformeDireccion_Estados");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.InformeDireccions)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InformeDireccion_Usuarios");
        });

        modelBuilder.Entity<InformeDocente>(entity =>
        {
            entity.HasKey(e => e.IdInformeDocente).HasName("PK__InformeD__9D75A7460215A928");

            entity.ToTable("InformeDocente");

            entity.Property(e => e.IdInformeDocente).HasColumnName("id_informe_docente");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.CantidadAccionSocial)
                .HasDefaultValue(0)
                .HasColumnName("cantidad_accion_social");
            entity.Property(e => e.CantidadCongresosActivos)
                .HasDefaultValue(0)
                .HasColumnName("cantidad_congresos_activos");
            entity.Property(e => e.CantidadCongresosPasivos)
                .HasDefaultValue(0)
                .HasColumnName("cantidad_congresos_pasivos");
            entity.Property(e => e.CantidadCursosGrado)
                .HasDefaultValue(0)
                .HasColumnName("cantidad_cursos_grado");
            entity.Property(e => e.CantidadDocencia)
                .HasDefaultValue(0)
                .HasColumnName("cantidad_docencia");
            entity.Property(e => e.CantidadInvestigacion)
                .HasDefaultValue(0)
                .HasColumnName("cantidad_investigacion");
            entity.Property(e => e.CantidadPosgrado)
                .HasDefaultValue(0)
                .HasColumnName("cantidad_posgrado");
            entity.Property(e => e.CantidadPublicaciones)
                .HasDefaultValue(0)
                .HasColumnName("cantidad_publicaciones");
            entity.Property(e => e.CantidadRepresentacion)
                .HasDefaultValue(0)
                .HasColumnName("cantidad_representacion");
            entity.Property(e => e.DetalleAccionSocial)
                .IsUnicode(false)
                .HasColumnName("detalle_accion_social");
            entity.Property(e => e.DetalleCongresosActivos)
                .IsUnicode(false)
                .HasColumnName("detalle_congresos_activos");
            entity.Property(e => e.DetalleCongresosPasivos)
                .IsUnicode(false)
                .HasColumnName("detalle_congresos_pasivos");
            entity.Property(e => e.DetalleCursosGrado)
                .IsUnicode(false)
                .HasColumnName("detalle_cursos_grado");
            entity.Property(e => e.DetalleDocencia)
                .IsUnicode(false)
                .HasColumnName("detalle_docencia");
            entity.Property(e => e.DetalleInvestigacion)
                .IsUnicode(false)
                .HasColumnName("detalle_investigacion");
            entity.Property(e => e.DetalleOtros)
                .IsUnicode(false)
                .HasColumnName("detalle_otros");
            entity.Property(e => e.DetallePosgrado)
                .IsUnicode(false)
                .HasColumnName("detalle_posgrado");
            entity.Property(e => e.DetallePublicaciones)
                .IsUnicode(false)
                .HasColumnName("detalle_publicaciones");
            entity.Property(e => e.DetalleRepresentacion)
                .IsUnicode(false)
                .HasColumnName("detalle_representacion");
            entity.Property(e => e.FechaAprobacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_aprobacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaEnvio)
                .HasColumnType("datetime")
                .HasColumnName("fecha_envio");
            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.ObservacionesDocente)
                .IsUnicode(false)
                .HasColumnName("observaciones_docente");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.InformeDocentes)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InformeDocente_Estados");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.InformeDocentes)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InformeDocente_Usuarios");
        });

        modelBuilder.Entity<InformeFinalFacultad>(entity =>
        {
            entity.HasKey(e => e.IdInformeFinal).HasName("PK__InformeF__AB5D83238FE27B12");

            entity.ToTable("InformeFinalFacultad");

            entity.Property(e => e.IdInformeFinal).HasColumnName("id_informe_final");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.FechaAprobacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_aprobacion");
            entity.Property(e => e.FechaGeneracion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_generacion");
            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.IdUsuarioGenera).HasColumnName("id_usuario_genera");
            entity.Property(e => e.Observaciones)
                .IsUnicode(false)
                .HasColumnName("observaciones");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.InformeFinalFacultads)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InformeFinal_Estados");

            entity.HasOne(d => d.IdUsuarioGeneraNavigation).WithMany(p => p.InformeFinalFacultads)
                .HasForeignKey(d => d.IdUsuarioGenera)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InformeFinal_Usuarios");
        });

        modelBuilder.Entity<Notificacione>(entity =>
        {
            entity.HasKey(e => e.IdNotificacion).HasName("PK__Notifica__8270F9A5C0073718");

            entity.Property(e => e.IdNotificacion).HasColumnName("id_notificacion");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Leida)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("No")
                .HasColumnName("leida");
            entity.Property(e => e.Mensaje)
                .IsUnicode(false)
                .HasColumnName("mensaje");
            entity.Property(e => e.TipoNotificacion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo_notificacion");
            entity.Property(e => e.Titulo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("titulo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notificaciones_Usuarios");
        });

        modelBuilder.Entity<Observacione>(entity =>
        {
            entity.HasKey(e => e.IdObservacion).HasName("PK__Observac__4CA8E723565441E1");

            entity.Property(e => e.IdObservacion).HasColumnName("id_observacion");
            entity.Property(e => e.Comentario)
                .IsUnicode(false)
                .HasColumnName("comentario");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdInformeDireccion).HasColumnName("id_informe_direccion");
            entity.Property(e => e.IdInformeDocente).HasColumnName("id_informe_docente");
            entity.Property(e => e.IdUsuarioRealiza).HasColumnName("id_usuario_realiza");
            entity.Property(e => e.TipoObservacion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo_observacion");

            entity.HasOne(d => d.IdUsuarioRealizaNavigation).WithMany(p => p.Observaciones)
                .HasForeignKey(d => d.IdUsuarioRealiza)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Observaciones_Usuarios");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Roles__6ABCB5E0E92EABDD");

            entity.HasIndex(e => e.NombreRol, "UQ__Roles__673CB435CE333FD4").IsUnique();

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Activo")
                .HasColumnName("estado");
            entity.Property(e => e.NombreRol)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre_rol");
        });

        modelBuilder.Entity<SesionesDepartamento>(entity =>
        {
            entity.HasKey(e => e.IdSesion).HasName("PK__Sesiones__8D3F9DFEF03782CF");

            entity.ToTable("SesionesDepartamento");

            entity.Property(e => e.IdSesion).HasColumnName("id_sesion");
            entity.Property(e => e.FechaSesion).HasColumnName("fecha_sesion");
            entity.Property(e => e.IdInformeDireccion).HasColumnName("id_informe_direccion");
            entity.Property(e => e.NumeroSesion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("numero_sesion");
            entity.Property(e => e.PuntosVistos)
                .IsUnicode(false)
                .HasColumnName("puntos_vistos");

            entity.HasOne(d => d.IdInformeDireccionNavigation).WithMany(p => p.SesionesDepartamentos)
                .HasForeignKey(d => d.IdInformeDireccion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SesionesDepartamento_InformeDireccion");
        });

        modelBuilder.Entity<TiposNombramiento>(entity =>
        {
            entity.HasKey(e => e.IdTipoNombramiento).HasName("PK__TiposNom__16B300D7A9151E9E");

            entity.ToTable("TiposNombramiento");

            entity.HasIndex(e => e.NombreNombramiento, "UQ__TiposNom__B3F062C52E237B08").IsUnique();

            entity.Property(e => e.IdTipoNombramiento).HasColumnName("id_tipo_nombramiento");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Activo")
                .HasColumnName("estado");
            entity.Property(e => e.NombreNombramiento)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre_nombramiento");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__4E3E04AD921295B0");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__2A586E0B129D7D36").IsUnique();

            entity.HasIndex(e => e.Cedula, "UQ__Usuarios__415B7BE5668B295E").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Apellido1)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellido1");
            entity.Property(e => e.Apellido2)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellido2");
            entity.Property(e => e.CambiarContrasena)
                .HasDefaultValue(true)
                .HasColumnName("cambiar_contrasena");
            entity.Property(e => e.Cedula)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("cedula");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("contrasena");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Activo")
                .HasColumnName("estado");
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.IdDepartamento).HasColumnName("id_departamento");
            entity.Property(e => e.IdGenero).HasColumnName("id_genero");
            entity.Property(e => e.IdTipoNombramiento).HasColumnName("id_tipo_nombramiento");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.UltimoInicioSesion)
                .HasColumnType("datetime")
                .HasColumnName("ultimo_inicio_sesion");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Categorias");

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdDepartamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Departamentos");

            entity.HasOne(d => d.IdGeneroNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdGenero)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Generos");

            entity.HasOne(d => d.IdTipoNombramientoNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdTipoNombramiento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_TiposNombramiento");
        });

        modelBuilder.Entity<UsuarioRol>(entity =>
        {
            entity.HasKey(e => e.IdUsuarioRol).HasName("PK__UsuarioR__D1F881FE94DD56E1");

            entity.ToTable("UsuarioRol");

            entity.Property(e => e.IdUsuarioRol).HasColumnName("id_usuario_rol");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.UsuarioRols)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuarioRol_Roles");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.UsuarioRols)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuarioRol_Usuarios");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
