USE DB_FacultadFarmacia;
GO
CREATE TABLE Roles (
    id_rol INT PRIMARY KEY IDENTITY(1,1),
    nombre_rol VARCHAR(50) NOT NULL UNIQUE,
    estado VARCHAR(20) NOT NULL DEFAULT 'Activo'
);
GO
CREATE TABLE Departamentos (
    id_departamento INT PRIMARY KEY IDENTITY(1,1),
    nombre_departamento VARCHAR(150) NOT NULL UNIQUE,
    estado VARCHAR(20) NOT NULL DEFAULT 'Activo'
);
GO

CREATE TABLE Generos (
    id_genero INT PRIMARY KEY IDENTITY(1,1),
    nombre_genero VARCHAR(50) NOT NULL UNIQUE,
    estado VARCHAR(20) NOT NULL DEFAULT 'Activo'
);
GO
CREATE TABLE Categorias (
    id_categoria INT PRIMARY KEY IDENTITY(1,1),
    nombre_categoria VARCHAR(100) NOT NULL UNIQUE,
    estado VARCHAR(20) NOT NULL DEFAULT 'Activo'
);
GO
CREATE TABLE TiposNombramiento (
    id_tipo_nombramiento INT PRIMARY KEY IDENTITY(1,1),
    nombre_nombramiento VARCHAR(100) NOT NULL UNIQUE,
    estado VARCHAR(20) NOT NULL DEFAULT 'Activo'
);
GO
CREATE TABLE EstadosInforme (
    id_estado INT PRIMARY KEY IDENTITY(1,1),
    nombre_estado VARCHAR(50) NOT NULL UNIQUE,
    descripcion VARCHAR(255)
);
GO
CREATE TABLE Usuarios (
    id_usuario INT PRIMARY KEY IDENTITY(1,1),

    cedula VARCHAR(20) NOT NULL UNIQUE,

    nombre VARCHAR(100) NOT NULL,
    apellido1 VARCHAR(100) NOT NULL,
    apellido2 VARCHAR(100),

    fecha_nacimiento DATE NOT NULL,

    correo VARCHAR(150) NOT NULL UNIQUE,

    contrasena VARCHAR(255) NOT NULL,

    telefono VARCHAR(20),

    id_genero INT NOT NULL,
    id_departamento INT NOT NULL,
    id_categoria INT NOT NULL,
    id_tipo_nombramiento INT NOT NULL,

    estado VARCHAR(20) NOT NULL DEFAULT 'Activo',

    fecha_registro DATETIME NOT NULL DEFAULT GETDATE(),

    cambiar_contrasena BIT NOT NULL DEFAULT 1,

    ultimo_inicio_sesion DATETIME NULL,

    CONSTRAINT FK_Usuarios_Generos
        FOREIGN KEY (id_genero)
        REFERENCES Generos(id_genero),

    CONSTRAINT FK_Usuarios_Departamentos
        FOREIGN KEY (id_departamento)
        REFERENCES Departamentos(id_departamento),

    CONSTRAINT FK_Usuarios_Categorias
        FOREIGN KEY (id_categoria)
        REFERENCES Categorias(id_categoria),

    CONSTRAINT FK_Usuarios_TiposNombramiento
        FOREIGN KEY (id_tipo_nombramiento)
        REFERENCES TiposNombramiento(id_tipo_nombramiento),

    CONSTRAINT CHK_Correo_UCR
        CHECK (correo LIKE '%@ucr.ac.cr')
);
GO
CREATE TABLE UsuarioRol (
    id_usuario_rol INT PRIMARY KEY IDENTITY(1,1),

    id_usuario INT NOT NULL,
    id_rol INT NOT NULL,

    CONSTRAINT FK_UsuarioRol_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario),

    CONSTRAINT FK_UsuarioRol_Roles
        FOREIGN KEY (id_rol)
        REFERENCES Roles(id_rol)
);
GO
CREATE TABLE InformeDocente (
    id_informe_docente INT PRIMARY KEY IDENTITY(1,1),

    id_usuario INT NOT NULL,
    id_estado INT NOT NULL,

    anio INT NOT NULL,

    fecha_creacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_envio DATETIME NULL,
    fecha_aprobacion DATETIME NULL,

    cantidad_congresos_activos INT DEFAULT 0,
    detalle_congresos_activos VARCHAR(MAX),

    cantidad_congresos_pasivos INT DEFAULT 0,
    detalle_congresos_pasivos VARCHAR(MAX),

    cantidad_accion_social INT DEFAULT 0,
    detalle_accion_social VARCHAR(MAX),

    cantidad_investigacion INT DEFAULT 0,
    detalle_investigacion VARCHAR(MAX),

    cantidad_docencia INT DEFAULT 0,
    detalle_docencia VARCHAR(MAX),

    cantidad_publicaciones INT DEFAULT 0,
    detalle_publicaciones VARCHAR(MAX),

    cantidad_cursos_grado INT DEFAULT 0,
    detalle_cursos_grado VARCHAR(MAX),

    cantidad_posgrado INT DEFAULT 0,
    detalle_posgrado VARCHAR(MAX),

    cantidad_representacion INT DEFAULT 0,
    detalle_representacion VARCHAR(MAX),

    detalle_otros VARCHAR(MAX),

    observaciones_docente VARCHAR(MAX),

    CONSTRAINT FK_InformeDocente_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario),

    CONSTRAINT FK_InformeDocente_Estados
        FOREIGN KEY (id_estado)
        REFERENCES EstadosInforme(id_estado)
);
GO
CREATE TABLE InformeDireccion (
    id_informe_direccion INT PRIMARY KEY IDENTITY(1,1),

    id_usuario INT NOT NULL,
    id_estado INT NOT NULL,

    anio INT NOT NULL,

    fecha_creacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_envio DATETIME NULL,
    fecha_aprobacion DATETIME NULL,

    observaciones_director VARCHAR(MAX),

    CONSTRAINT FK_InformeDireccion_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario),

    CONSTRAINT FK_InformeDireccion_Estados
        FOREIGN KEY (id_estado)
        REFERENCES EstadosInforme(id_estado)
);
GO
CREATE TABLE SesionesDepartamento (
    id_sesion INT PRIMARY KEY IDENTITY(1,1),

    id_informe_direccion INT NOT NULL,

    numero_sesion VARCHAR(50) NOT NULL,

    fecha_sesion DATE NOT NULL,

    puntos_vistos VARCHAR(MAX) NOT NULL,

    CONSTRAINT FK_SesionesDepartamento_InformeDireccion
        FOREIGN KEY (id_informe_direccion)
        REFERENCES InformeDireccion(id_informe_direccion)
);
GO
CREATE TABLE CursosDireccion (
    id_curso_direccion INT PRIMARY KEY IDENTITY(1,1),

    id_informe_direccion INT NOT NULL,

    sigla_curso VARCHAR(50) NOT NULL,
    nombre_curso VARCHAR(255) NOT NULL,

    coordinacion_cantidad INT DEFAULT 0,
    coordinacion_detalle VARCHAR(MAX),

    colaboradores_cantidad INT DEFAULT 0,
    colaboradores_detalle VARCHAR(MAX),

    invitados_cantidad INT DEFAULT 0,
    invitados_detalle VARCHAR(MAX),

    experiencias_practicas_cantidad INT DEFAULT 0,
    experiencias_practicas_detalle VARCHAR(MAX),

    actividades_docencia_integradas_cantidad INT DEFAULT 0,
    actividades_docencia_integradas_detalle VARCHAR(MAX),

    actividades_analisis_contexto_cantidad INT DEFAULT 0,
    actividades_analisis_contexto_detalle VARCHAR(MAX),

    tecnicas_didacticas_cantidad INT DEFAULT 0,
    tecnicas_didacticas_detalle VARCHAR(MAX),

    asistentes_curso INT DEFAULT 0,

    CONSTRAINT FK_CursosDireccion_InformeDireccion
        FOREIGN KEY (id_informe_direccion)
        REFERENCES InformeDireccion(id_informe_direccion)
);
GO
CREATE TABLE InformeDepartamental (
    id_informe_departamental INT PRIMARY KEY IDENTITY(1,1),

    id_director INT NOT NULL,
    id_departamento INT NOT NULL,
    id_estado INT NOT NULL,

    anio INT NOT NULL,

    fecha_generacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_envio DATETIME NULL,
    fecha_aprobacion DATETIME NULL,

    observaciones_director VARCHAR(MAX),

    CONSTRAINT FK_InformeDepartamental_Usuarios
        FOREIGN KEY (id_director)
        REFERENCES Usuarios(id_usuario),

    CONSTRAINT FK_InformeDepartamental_Departamentos
        FOREIGN KEY (id_departamento)
        REFERENCES Departamentos(id_departamento),

    CONSTRAINT FK_InformeDepartamental_Estados
        FOREIGN KEY (id_estado)
        REFERENCES EstadosInforme(id_estado)
);
GO
CREATE TABLE DetalleInformeDepartamental (
    id_detalle_departamental INT PRIMARY KEY IDENTITY(1,1),

    id_informe_departamental INT NOT NULL,

    tipo_actividad VARCHAR(150) NOT NULL,

    cantidad INT DEFAULT 0,

    detalle_actividad VARCHAR(MAX),

    CONSTRAINT FK_DetalleInformeDepartamental
        FOREIGN KEY (id_informe_departamental)
        REFERENCES InformeDepartamental(id_informe_departamental)
);
GO
CREATE TABLE InformeFinalFacultad (
    id_informe_final INT PRIMARY KEY IDENTITY(1,1),

    id_usuario_genera INT NOT NULL,
    id_estado INT NOT NULL,

    anio INT NOT NULL,

    fecha_generacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_aprobacion DATETIME NULL,

    observaciones VARCHAR(MAX),

    CONSTRAINT FK_InformeFinal_Usuarios
        FOREIGN KEY (id_usuario_genera)
        REFERENCES Usuarios(id_usuario),

    CONSTRAINT FK_InformeFinal_Estados
        FOREIGN KEY (id_estado)
        REFERENCES EstadosInforme(id_estado)
);
GO
CREATE TABLE DetalleInformeFinal (
    id_detalle_final INT PRIMARY KEY IDENTITY(1,1),

    id_informe_final INT NOT NULL,

    tipo_actividad VARCHAR(150) NOT NULL,

    cantidad INT DEFAULT 0,

    detalle_actividad VARCHAR(MAX),

    CONSTRAINT FK_DetalleInformeFinal
        FOREIGN KEY (id_informe_final)
        REFERENCES InformeFinalFacultad(id_informe_final)
);
GO
CREATE TABLE Observaciones (
    id_observacion INT PRIMARY KEY IDENTITY(1,1),

    id_informe_docente INT NULL,
    id_informe_direccion INT NULL,

    id_usuario_realiza INT NOT NULL,

    comentario VARCHAR(MAX) NOT NULL,

    fecha DATETIME NOT NULL DEFAULT GETDATE(),

    tipo_observacion VARCHAR(50),

    CONSTRAINT FK_Observaciones_Usuarios
        FOREIGN KEY (id_usuario_realiza)
        REFERENCES Usuarios(id_usuario)
);
GO
CREATE TABLE Notificaciones (
    id_notificacion INT PRIMARY KEY IDENTITY(1,1),

    id_usuario INT NOT NULL,

    titulo VARCHAR(255) NOT NULL,

    mensaje VARCHAR(MAX) NOT NULL,

    fecha DATETIME NOT NULL DEFAULT GETDATE(),

    leida VARCHAR(20) NOT NULL DEFAULT 'No',

    tipo_notificacion VARCHAR(50),

    CONSTRAINT FK_Notificaciones_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario)
);
GO
CREATE TABLE Auditoria (
    id_auditoria INT PRIMARY KEY IDENTITY(1,1),

    id_usuario INT NOT NULL,

    tabla_afectada VARCHAR(100) NOT NULL,

    accion VARCHAR(50) NOT NULL,

    descripcion VARCHAR(MAX),

    fecha DATETIME NOT NULL DEFAULT GETDATE(),

    ip_equipo VARCHAR(100),

    CONSTRAINT FK_Auditoria_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario)
);
GO