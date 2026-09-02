using System.ComponentModel.DataAnnotations;
using Vercom.Models;

namespace Vercom.ViewModels;

public class EntityRegistrationViewModel
{
    public Entidad Entidad { get; set; } = new();

    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    [Display(Name = "Nombre de Usuario (Admin)")]
    public string AdminUsername { get; set; } = null!;

    [Required(ErrorMessage = "El nombre completo es obligatorio")]
    [Display(Name = "Nombre Completo del Responsable")]
    public string AdminFullName { get; set; } = null!;

    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo inválido")]
    [Display(Name = "Correo Electrónico (Admin)")]
    public string AdminEmail { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Mínimo 8 caracteres")]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = null!;

    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar Contraseña")]
    public string ConfirmPassword { get; set; } = null!;
}
