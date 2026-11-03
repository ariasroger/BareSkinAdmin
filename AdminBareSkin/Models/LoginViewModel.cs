
using System.ComponentModel.DataAnnotations;

namespace AdminBareSkin.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingresá tu correo")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá tu contraseña")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
