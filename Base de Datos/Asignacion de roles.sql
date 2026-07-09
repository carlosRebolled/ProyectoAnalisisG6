USE DB_FacultadFarmacia;
GO

INSERT INTO dbo.UsuarioRol (id_usuario, id_rol)
VALUES
(3, 4), -- Kevin Administrador
(4, 1), -- Fernando Docente
(5, 2), -- Nahomy Director
(6, 2), -- Cristina Director
(7, 2), -- Milania Director
(8, 1), -- Josue Docente
(9, 1), -- Tatiana Docente
(10, 1); -- Marta Docente
GO

INSERT INTO dbo.UsuarioRol (id_usuario, id_rol)
VALUES
(10, 3); -- Luis Decano
GO

SELECT * FROM dbo.UsuarioRol;

SELECT * FROM dbo.Roles;

SELECT * FROM dbo.Usuarios;

DELETE FROM dbo.UsuarioRol;
GO

DBCC CHECKIDENT ('dbo.UsuarioRol', RESEED, 0);
GO
