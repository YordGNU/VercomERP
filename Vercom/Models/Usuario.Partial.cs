using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Vercom.Models;

[ModelMetadataType(typeof(UsuarioMetadata))]
public partial class Usuario
{
}

public class UsuarioMetadata
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre de usuario es demasiado largo")]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = null!;

    [Required(ErrorMessage = "El nombre completo es obligatorio")]
    [StringLength(150, ErrorMessage = "El nombre es demasiado largo")]
    [Display(Name = "Nombre Completo")]
    public string NombreCompleto { get; set; } = null!;

    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
    [StringLength(150)]
    [Display(Name = "Correo Electrónico")]
    public string? Email { get; set; }

    [Display(Name = "Estado Activo")]
    public bool Activo { get; set; }
}
