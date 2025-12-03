using Microsoft.AspNetCore.Identity;

namespace GestaoLoja.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string Nif { get; set; } = string.Empty;
        public string Morada { get; set; } = string.Empty;

        public string TipoUtilizador { get; set; } = "Cliente";

        public string EstadoConta {  get; set; } = "Pendente";
    }

}
