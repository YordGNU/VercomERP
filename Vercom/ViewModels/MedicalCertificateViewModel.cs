using VERCOM.Models;

namespace VERCOM.ViewModels;

public class MedicalCertificateViewModel
{
    public CertificadoMedico Certificado { get; set; } = new();
    public string NombreEmpleado { get; set; } = null!;

    public string Title { get; set; } = "Registrar Certificado Médico (Subsidio)";
}
