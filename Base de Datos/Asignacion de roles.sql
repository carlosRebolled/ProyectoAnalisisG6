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
(4,1), -- Felipe -- Docente
(5,2); -- Nahomy -- Director
GO

select * from dbo.UsuarioRol