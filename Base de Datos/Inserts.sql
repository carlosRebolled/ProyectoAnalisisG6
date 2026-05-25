USE DB_FacultadFarmacia;
GO

INSERT INTO dbo.Roles (nombre_rol)
VALUES
('Docente'),
('Director'),
('Decano'),
('Administrador'),
('Jefatura'),
('Visualizador');
GO

INSERT INTO dbo.Departamentos (nombre_departamento)
VALUES
('Departamento de Farmacia Industrial'),
('Departamento de Farmacología, Toxicología y Farmacodependencia'),
('Departamento de Atención Farmacéutica y Farmacia Clínica'),
('Otro');
GO

INSERT INTO dbo.Generos (nombre_genero)
VALUES
('Masculino'),
('Femenino'),
('Otro');
GO

INSERT INTO dbo.Categorias (nombre_categoria)
VALUES
('Interino Licenciado'),
('Asociado'),
('Adjunto'),
('Instructor'),
('Catedrático'),
('Invitado I año'),
('Invitado II año'),
('Exbec.Doctor'),
('Administrativo'),
('Exbecarío Doctor (088)');
GO

INSERT INTO dbo.TiposNombramiento (nombre_nombramiento)
VALUES
('Propiedad'),
('Interino');
GO

INSERT INTO dbo.EstadosInforme (nombre_estado, descripcion)
VALUES
('Borrador', 'Informe en edición'),
('Pendiente', 'Informe enviado para revisión'),
('Devuelto', 'Informe devuelto con observaciones'),
('Aprobado', 'Informe aprobado'),
('Finalizado', 'Informe consolidado y finalizado');
GO
