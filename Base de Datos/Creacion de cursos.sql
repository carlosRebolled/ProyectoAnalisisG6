USE DB_FacultadFarmacia;
GO

INSERT INTO dbo.Cursos
(
    sigla_curso,
    nombre_curso,
    estado
)
VALUES
(
    'FA2009',
    'Introducción a la Farmacia',
    'Activo'
),
(
    'FA0222',
    'Análisis de Medicamentos 1',
    'Activo'
);
GO

SELECT *
FROM dbo.Cursos;