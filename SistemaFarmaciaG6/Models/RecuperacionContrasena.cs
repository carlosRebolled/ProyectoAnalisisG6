namespace SistemaFarmaciaG6.Models;

public class RecuperacionContrasena
{
    public int IdRecuperacion { get; set; }

    public int IdUsuario { get; set; }

    public string CodigoHash { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public bool Utilizado { get; set; }

    public int Intentos { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}