using Microsoft.AspNetCore.Mvc;

namespace SistemaFarmaciaG6.Helpers
{
    public static class SeguridadHelper
    {
        public static bool EsAdministrador(HttpContext httpContext)
        {
            var rol = httpContext.Session.GetString("Rol");
            return rol == "Administrador";
        }

        public static IActionResult RedirigirSiNoAdmin(Controller controller)
        {
            return controller.RedirectToAction("Index", "Home");
        }
    }
}