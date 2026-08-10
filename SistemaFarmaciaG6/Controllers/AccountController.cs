using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Models;
using SistemaFarmaciaG6.Helpers;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using SistemaFarmaciaG6.Services;


namespace SistemaFarmaciaG6.Controllers
{
    public class AccountController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;
        private readonly IEmailService _emailService;

        private readonly PasswordHasher<RecuperacionContrasena> _codigoHasher;


        public AccountController(
    DbFacultadFarmaciaContext context,
    IEmailService emailService)
        {
            _context = context;

            _emailService = emailService;

            _passwordHasher =
                new PasswordHasher<Usuario>();

            _codigoHasher =
                new PasswordHasher<RecuperacionContrasena>();
        }


        [HttpGet]
        public IActionResult OlvideContrasena()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OlvideContrasena(
    string correo)
        {
            correo =
                correo?.Trim().ToLower() ?? "";

            var usuario =
                await _context.Usuarios
                    .FirstOrDefaultAsync(u =>
                        u.Correo.ToLower() == correo
                    );

            /*
             * Mensaje genérico para no revelar
             * si el correo existe o no.
             */
            if (usuario == null)
            {
                TempData["Mensaje"] =
                    "Si el correo está registrado, recibirá un código de recuperación.";

                return View();
            }

            int codigoNumero =
                RandomNumberGenerator.GetInt32(
                    100000,
                    1000000
                );

            string codigo =
                codigoNumero.ToString();

            var recuperacion =
                new RecuperacionContrasena
                {
                    IdUsuario =
                        usuario.IdUsuario,

                    FechaCreacion =
                        DateTime.Now,

                    FechaExpiracion =
                        DateTime.Now.AddMinutes(10),

                    Utilizado =
                        false,

                    Intentos =
                        0
                };

            recuperacion.CodigoHash =
                _codigoHasher.HashPassword(
                    recuperacion,
                    codigo
                );

            _context.RecuperacionesContrasena
                .Add(recuperacion);

            await _context.SaveChangesAsync();

            string mensaje = $@"
        <h2>Recuperación de contraseña</h2>

        <p>
            Su código de recuperación es:
        </p>

        <h1>
            {codigo}
        </h1>

        <p>
            Este código vence en 10 minutos.
        </p>
    ";

            await _emailService.EnviarAsync(
                usuario.Correo,
                "Recuperación de contraseña",
                mensaje
            );

            return RedirectToAction(
                nameof(VerificarCodigo),
                new
                {
                    correo = usuario.Correo
                }
            );
        }

        [HttpGet]
        public IActionResult VerificarCodigo(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                return RedirectToAction(nameof(OlvideContrasena));
            }

            ViewBag.Correo = correo;

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerificarCodigo(
            string correo,
            string codigo)
        {
            correo = correo?.Trim().ToLower() ?? "";
            codigo = codigo?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(codigo))
            {
                ViewBag.Error = "Debe ingresar el código de verificación.";
                ViewBag.Correo = correo;

                return View();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Correo.ToLower() == correo);

            if (usuario == null)
            {
                ViewBag.Error = "Código inválido o vencido.";
                ViewBag.Correo = correo;

                return View();
            }

            var recuperacion = await _context.RecuperacionesContrasena
                .Where(r =>
                    r.IdUsuario == usuario.IdUsuario &&
                    !r.Utilizado)
                .OrderByDescending(r => r.FechaCreacion)
                .FirstOrDefaultAsync();

            if (recuperacion == null)
            {
                ViewBag.Error = "Código inválido o vencido.";
                ViewBag.Correo = correo;

                return View();
            }

            if (recuperacion.FechaExpiracion < DateTime.Now)
            {
                ViewBag.Error =
                    "El código ha vencido. Solicite uno nuevo.";

                ViewBag.Correo = correo;

                return View();
            }

            if (recuperacion.Intentos >= 5)
            {
                ViewBag.Error =
                    "Se alcanzó el máximo de intentos. Solicite un código nuevo.";

                ViewBag.Correo = correo;

                return View();
            }

            var resultado = _codigoHasher.VerifyHashedPassword(
                recuperacion,
                recuperacion.CodigoHash,
                codigo
            );

            if (resultado == PasswordVerificationResult.Failed)
            {
                recuperacion.Intentos++;

                await _context.SaveChangesAsync();

                ViewBag.Error = "Código incorrecto.";
                ViewBag.Correo = correo;

                return View();
            }

            /*
             * Código correcto.
             * Guardamos temporalmente en sesión el usuario
             * autorizado para cambiar la contraseña.
             */
            HttpContext.Session.SetInt32(
                "IdUsuarioRecuperacion",
                usuario.IdUsuario
            );

            HttpContext.Session.SetInt32(
                "IdRecuperacion",
                recuperacion.IdRecuperacion
            );

            return RedirectToAction(
                nameof(RestablecerContrasena)
            );
        }

        [HttpGet]
        public IActionResult RestablecerContrasena()
        {
            int? idUsuario = HttpContext.Session.GetInt32(
                "IdUsuarioRecuperacion"
            );

            int? idRecuperacion = HttpContext.Session.GetInt32(
                "IdRecuperacion"
            );

            if (idUsuario == null || idRecuperacion == null)
            {
                TempData["Error"] =
                    "Debe verificar primero el código de recuperación.";

                return RedirectToAction(nameof(OlvideContrasena));
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerContrasena(
            string nuevaContrasena,
            string confirmarContrasena)
        {
            int? idUsuario = HttpContext.Session.GetInt32(
                "IdUsuarioRecuperacion"
            );

            int? idRecuperacion = HttpContext.Session.GetInt32(
                "IdRecuperacion"
            );

            if (idUsuario == null || idRecuperacion == null)
            {
                TempData["Error"] =
                    "La solicitud de recuperación no es válida.";

                return RedirectToAction(nameof(OlvideContrasena));
            }

            if (string.IsNullOrWhiteSpace(nuevaContrasena))
            {
                ViewBag.Error =
                    "Debe ingresar una nueva contraseña.";

                return View();
            }

            if (nuevaContrasena.Length < 8)
            {
                ViewBag.Error =
                    "La contraseña debe tener al menos 8 caracteres.";

                return View();
            }

            if (nuevaContrasena != confirmarContrasena)
            {
                ViewBag.Error =
                    "Las contraseñas no coinciden.";

                return View();
            }

            var recuperacion =
                await _context.RecuperacionesContrasena
                    .FirstOrDefaultAsync(r =>
                        r.IdRecuperacion == idRecuperacion.Value &&
                        r.IdUsuario == idUsuario.Value &&
                        !r.Utilizado
                    );

            if (recuperacion == null)
            {
                ViewBag.Error =
                    "La solicitud de recuperación ya no es válida.";

                return View();
            }

            if (recuperacion.FechaExpiracion < DateTime.Now)
            {
                ViewBag.Error =
                    "La solicitud de recuperación ha vencido.";

                return View();
            }

            var usuario =
                await _context.Usuarios
                    .FirstOrDefaultAsync(u =>
                        u.IdUsuario == idUsuario.Value
                    );

            if (usuario == null)
            {
                return NotFound();
            }

            /*
             * Guardar la nueva contraseña utilizando hash.
             */
            usuario.Contrasena =
                _passwordHasher.HashPassword(
                    usuario,
                    nuevaContrasena
                );

            /*
             * Marcar el código como utilizado para que
             * no pueda volver a usarse.
             */
            recuperacion.Utilizado = true;

            await _context.SaveChangesAsync();

            // Auditoría cambio de contraseña
            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "Usuarios",
                "RestablecerContraseña",
                $"Se restableció la contraseña del usuario {usuario.Nombre} {usuario.Apellido1}."
            );

            /*
             * Limpiar los datos temporales de recuperación.
             */
            HttpContext.Session.Remove(
                "IdUsuarioRecuperacion"
            );

            HttpContext.Session.Remove(
                "IdRecuperacion"
            );

            TempData["Exito"] =
                "Su contraseña fue restablecida correctamente. Ya puede iniciar sesión.";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string correo, string contrasena)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Correo == correo);

            bool accesoPermitido = false;

            if (usuario != null)
            {
                try
                {
                    var resultado = _passwordHasher.VerifyHashedPassword(
                        usuario,
                        usuario.Contrasena,
                        contrasena
                    );

                    if (resultado == PasswordVerificationResult.Success ||
                        resultado == PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        accesoPermitido = true;

                        if (resultado ==
                            PasswordVerificationResult.SuccessRehashNeeded)
                        {
                            usuario.Contrasena = _passwordHasher.HashPassword(
                                usuario,
                                contrasena
                            );

                            _context.SaveChanges();
                        }
                    }
                }
                catch
                {
                    // La contraseña probablemente está almacenada
                    // en texto plano.
                }

                // Compatibilidad temporal con contraseñas antiguas.
                if (!accesoPermitido &&
                    usuario.Contrasena == contrasena)
                {
                    accesoPermitido = true;

                    // Migración automática a hash.
                    usuario.Contrasena = _passwordHasher.HashPassword(
                        usuario,
                        contrasena
                    );

                    _context.SaveChanges();

                    AuditoriaHelper.Registrar(
                        _context,
                        HttpContext,
                        "Usuarios",
                        "MigrarContraseña",
                        $"La contraseña del usuario {usuario.Nombre} {usuario.Apellido1} fue migrada automáticamente a hash."
                    );
                }
            }

            if (!accesoPermitido)
            {
                ViewBag.Error = "Correo o contraseña incorrectos";
                return View();
            }

            if (usuario.Estado != "Activo")
            {
                ViewBag.Error = "Su cuenta se encuentra inactiva. Contacte al administrador.";
                return View();
            }

            var usuarioRol = _context.UsuarioRols
                .FirstOrDefault(ur => ur.IdUsuario == usuario.IdUsuario);

            if (usuarioRol == null)
            {
                ViewBag.Error = "El usuario no tiene un rol asignado";
                return View();
            }

            var rol = _context.Roles
                .FirstOrDefault(r => r.IdRol == usuarioRol.IdRol);

            if (rol == null)
            {
                ViewBag.Error = "El rol asignado no existe";
                return View();
            }

            HttpContext.Session.SetInt32(
                "IdUsuario",
                usuario.IdUsuario
            );

            HttpContext.Session.SetString(
                "Nombre",
                $"{usuario.Nombre} {usuario.Apellido1} {usuario.Apellido2}"
            );

            HttpContext.Session.SetString(
                "Rol",
                rol.NombreRol
            );

            // Auditoría Login
            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "Usuarios",
                "Login",
                $"Inicio de sesión del usuario {usuario.Nombre} {usuario.Apellido1} con rol {rol.NombreRol}."
            );

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario != null)
            {
                var usuario = _context.Usuarios.Find(idUsuario);

                if (usuario != null)
                {
                    AuditoriaHelper.Registrar(
                        _context,
                        HttpContext,
                        "Usuarios",
                        "Logout",
                        $"Cierre de sesión del usuario {usuario.Nombre} {usuario.Apellido1}."
                    );
                }
            }

            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}