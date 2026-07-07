USE DB_FacultadFarmacia;
GO

INSERT INTO dbo.UsuarioRol (id_usuario, id_rol)
VALUES
(1, 4), -- Kevin Administrador
(2, 1), -- Fernando Docente
(3, 2), -- Nahomy Director
(5, 2), -- Cristina Director
(6, 2), -- Milania Director
(7, 1), -- Josue Docente
(8, 1), -- Tatiana Docente
(9, 1); -- Marta Docente
GO

SELECT * FROM dbo.UsuarioRol;

SELECT * FROM dbo.Roles;

DELETE FROM dbo.UsuarioRol;
GO

DBCC CHECKIDENT ('dbo.UsuarioRol', RESEED, 0);
GO
