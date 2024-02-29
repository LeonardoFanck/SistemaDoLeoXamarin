using SistemaDoLeo.Modelos.Classes;
using SistemaDoLeo.Paginas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SistemaDoLeo
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {
        //public Dictionary<string, Type> Routes { get; private set; } = new Dictionary<string, Type>();

        Operador operador;
        List<OperadorTela> listaPermissao = new List<OperadorTela>();

        public AppShell(Operador operador, List<OperadorTela> listaPermissao)
        {
            InitializeComponent();
            BindingContext = this;

            this.operador = operador;
            this.listaPermissao = listaPermissao;

            ValidarTelas();
        }

        private void ValidarTelas()
        {
            TabBar tabBar = new TabBar();
            OperadorTela tela;

            tabBar.Items.Add(CreateShellContent("Inicio", "inicio.png", new Home()));

            // PEDIDO
            tela = listaPermissao.FirstOrDefault(l => l.TelaId == 5);
            if (tela != null && tela.Ativo)
            {
                tabBar.Items.Add(CreateShellContent("Pedido", "pedido.png", new Pedidos()));
            }
            // CLIENTE
            tela = listaPermissao.FirstOrDefault(l => l.TelaId == 3);
            if (tela != null && tela.Ativo)
            {
                tabBar.Items.Add(CreateShellContent("Cliente", "cliente.png", new Clientes()));
            }
            // PRODUTO
            tela = listaPermissao.FirstOrDefault(l => l.TelaId == 6);
            if (tela != null && tela.Ativo)
            {
                tabBar.Items.Add(CreateShellContent("Produto", "produto.png", new Produtos()));
            }
            // OPERADOR
            tela = listaPermissao.FirstOrDefault(l => l.TelaId == 1);
            if (tela != null && tela.Ativo)
            {
                tabBar.Items.Add(CreateShellContent("Operador", "operador.png", new Operadores()));
            }
            // CATEGORIA
            tela = listaPermissao.FirstOrDefault(l => l.TelaId == 2);
            if (tela != null && tela.Ativo)
            {
                tabBar.Items.Add(CreateShellContent("Categoria", "categoria.png", new Categorias()));
            }
            // FORMA PGTO
            tela = listaPermissao.FirstOrDefault(l => l.TelaId == 4);
            if (tela != null && tela.Ativo)
            {
                tabBar.Items.Add(CreateShellContent("Forma Pgto", "pgto.png", new FormasPgto()));
            }
            // RELATORIO
            tela = listaPermissao.FirstOrDefault(l => l.TelaId == 7);
            if (tela != null && tela.Ativo)
            {
                tabBar.Items.Add(CreateShellContent("Relatório", "relatorio.png", new Relatorio()));
            }

            // SOBRE
            tabBar.Items.Add(CreateShellContent("Sobre", "sobre.png", new Sobre(this)));

            // Define a TabBar na Shell
            this.Items.Add(tabBar);
        }

        private ShellContent CreateShellContent(string title, string icon, Page page)
        {
            return new ShellContent
            {
                Title = title,
                Icon = icon,
                Content = page
            };
        }
    }
}