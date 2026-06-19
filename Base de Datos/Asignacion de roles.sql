INSERT INTO dbo.UsuarioRol
(
    id_usuario,
    id_rol
)
VALUES
(3,4); -- Kevin -- Administrador
GO

INSERT INTO dbo.UsuarioRol
(
    id_usuario,
    id_rol
)
VALUES
(2,1), -- Felipe -- Docente
(3,2); -- Nahomy -- Director
GO


select * from dbo.Roles

select * from dbo.UsuarioRol

SELECT id_usuario, nombre, correo
FROM dbo.Usuarios;
