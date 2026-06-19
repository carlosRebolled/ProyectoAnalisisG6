using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Models;

namespace SistemaFarmaciaG6.Helpers
{
    public static class AuditoriaHelper
    {
        public static void Registrar(
            DbFacultadFarmaciaContext context,
            HttpContext httpContext,
            string tabla,
            string accion,
            string descripcion)
        {
            int? idUsuario = httpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return;
            }

            var auditoria = new Auditorium
            {
                IdUsuario = idUsuario.Value,
                TablaAfectada = tabla,
                Accion = accion,
                Descripcion = descripcion,
                Fecha = DateTime.Now,
                IpEquipo = httpContext.Connection.RemoteIpAddress?.ToString()
            };

            context.Auditoria.Add(auditoria);
            context.SaveChanges();
        }
    }
}