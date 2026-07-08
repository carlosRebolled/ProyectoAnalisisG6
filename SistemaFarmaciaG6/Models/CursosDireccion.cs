using System;
using System.Collections.Generic;

namespace SistemaFarmaciaG6.Models;

public partial class CursosDireccion
{
    public int IdCursoDireccion { get; set; }

    public int IdInformeDireccion { get; set; }

    public int IdCurso { get; set; }

    public int? CoordinacionCantidad { get; set; }

    public string? CoordinacionDetalle { get; set; }

    public int? ColaboradoresCantidad { get; set; }

    public string? ColaboradoresDetalle { get; set; }

    public int? InvitadosCantidad { get; set; }

    public string? InvitadosDetalle { get; set; }

    public int? ExperienciasPracticasCantidad { get; set; }

    public string? ExperienciasPracticasDetalle { get; set; }

    public int? ActividadesDocenciaIntegradasCantidad { get; set; }

    public string? ActividadesDocenciaIntegradasDetalle { get; set; }

    public int? ActividadesAnalisisContextoCantidad { get; set; }

    public string? ActividadesAnalisisContextoDetalle { get; set; }

    public int? TecnicasDidacticasCantidad { get; set; }

    public string? TecnicasDidacticasDetalle { get; set; }

    public int? AsistentesCurso { get; set; }

    public virtual Curso IdCursoNavigation { get; set; } = null!;

    public virtual InformeDireccion IdInformeDireccionNavigation { get; set; } = null!;
}
