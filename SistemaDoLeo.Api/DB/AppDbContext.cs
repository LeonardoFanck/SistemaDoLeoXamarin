using Microsoft.EntityFrameworkCore;
using SistemaDoLeo.Modelos.Classes;

namespace XamarinAPI.DB
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Operador>? Operadores { get; set; }
        public DbSet<OperadorTela>? OperadorTelas { get; set; }
        public DbSet<Tela>? Telas { get; set; }
        public DbSet<Cliente>? Clientes { get; set; }
        public DbSet<FormaPgto>? FormaPgtos { get; set; }
        public DbSet<Pedido>? Pedidos { get; set; }
        public DbSet<PedidoItem>? PedidoItens { get; set; }
        public DbSet<Produto>? Produtos { get; set; }
        public DbSet<ProximoRegistro>? ProximoRegistros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // OPERADOR
            modelBuilder.Entity<Operador>().HasData(new Operador
            {
                Id = 1,
                Nome = "Operador Padrão - Administrador",
                Senha = "5555",
                Admin = true,
                Inativo = false
            });

            // TELAS
            modelBuilder.Entity<Tela>().HasData(new Tela
            {
                Id = 1,
                Nome = "Operador"
            });
            modelBuilder.Entity<Tela>().HasData(new Tela
            {
                Id = 2,
                Nome = "Categoria"
            });
            modelBuilder.Entity<Tela>().HasData(new Tela
            {
                Id = 3,
                Nome = "Cliente"
            });
            modelBuilder.Entity<Tela>().HasData(new Tela
            {
                Id = 4,
                Nome = "Forma Pagamento"
            });
            modelBuilder.Entity<Tela>().HasData(new Tela
            {
                Id = 5,
                Nome = "Pedido"
            });
            modelBuilder.Entity<Tela>().HasData(new Tela
            {
                Id = 6,
                Nome = "Produto"
            });
            modelBuilder.Entity<Tela>().HasData(new Tela
            {
                Id = 7,
                Nome = "Relatorio"
            });

            // telas operador

            modelBuilder.Entity<OperadorTela>().HasData(new OperadorTela
            {
                Id = 1,
                OperadorId = 1,
                Ativo = true,
                Editar = true,
                Excluir = true,
                Novo = true,
                TelaId = 1
            });

            modelBuilder.Entity<OperadorTela>().HasData(new OperadorTela
            {
                Id = 2,
                OperadorId = 1,
                Ativo = true,
                Editar = true,
                Excluir = true,
                Novo = true,
                TelaId = 2
            });

            modelBuilder.Entity<OperadorTela>().HasData(new OperadorTela
            {
                Id = 3,
                OperadorId = 1,
                Ativo = true,
                Editar = true,
                Excluir = true,
                Novo = true,
                TelaId = 3
            });

            modelBuilder.Entity<OperadorTela>().HasData(new OperadorTela
            {
                Id = 4,
                OperadorId = 1,
                Ativo = true,
                Editar = true,
                Excluir = true,
                Novo = true,
                TelaId = 4
            });

            modelBuilder.Entity<OperadorTela>().HasData(new OperadorTela
            {
                Id = 5,
                OperadorId = 1,
                Ativo = true,
                Editar = true,
                Excluir = true,
                Novo = true,
                TelaId = 5
            });

            modelBuilder.Entity<OperadorTela>().HasData(new OperadorTela
            {
                Id = 6,
                OperadorId = 1,
                Ativo = true,
                Editar = true,
                Excluir = true,
                Novo = true,
                TelaId = 6
            });

            // PROXIMO REGISTRO
            modelBuilder.Entity<ProximoRegistro>().HasData(new ProximoRegistro
            {
                Id = 1,
                Categoria = 0,
                Cliente = 0,
                FormaPgto = 0,
                Operador = 1,
                Pedido = 0,
                Produto = 0,
            });
        }
    }
}
