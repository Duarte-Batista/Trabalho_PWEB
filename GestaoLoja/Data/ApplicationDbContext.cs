using GestaoLoja.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestaoLoja.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Encomenda> Encomendas { get; set; }
        public DbSet<ItemEncomenda> ItensEncomenda { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }
    }
}
