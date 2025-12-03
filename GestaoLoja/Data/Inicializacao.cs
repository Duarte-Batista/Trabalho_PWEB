using GestaoLoja.Data;
using Microsoft.AspNetCore.Identity;

namespace GestaoLoja.Data
{
	public static class Inicializacao
	{
		public static async Task CriaDadosIniciais(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			// Adicionar Roles (Cargos) padrão se não existirem
			string[] roles = { "Administrador", "Cliente", "Fornecedor", "Funcionario" };

			foreach (var role in roles)
			{
				if (!await roleManager.RoleExistsAsync(role))
				{
					await roleManager.CreateAsync(new IdentityRole(role));
				}
			}

			// Atribuir Cargo de Admin ao teu email específico
			var emailAdmin = "admin@localhost.com";

			var adminUser = await userManager.FindByEmailAsync(emailAdmin);

			if (adminUser != null)
			{
				// Adiciona o cargo de Administrador se ainda não tiver
				if (!await userManager.IsInRoleAsync(adminUser, "Administrador"))
				{
					await userManager.AddToRoleAsync(adminUser, "Administrador");
				}

				// Atualiza a propriedade de texto para ficar visualmente bonito na lista
				if (adminUser.TipoUtilizador != "Administrador")
				{
					adminUser.TipoUtilizador = "Administrador";
					adminUser.EstadoConta = "Ativo"; // Garante que o admin nunca fica pendente
					await userManager.UpdateAsync(adminUser);
				}
			}
		}
	}
}