using System.Collections.Generic;
using SistemaDoLeo.Modelos.Classes;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SistemaDoLeo.Paginas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FlyoutMenu : ContentPage
    {
        private List<FlyoutItens> telas { get; set; } = new List<FlyoutItens>();


        public FlyoutMenu()
        {
            InitializeComponent();

            validarTelas();
        }

        private void validarTelas()
        {
            lista.ItemsSource = telas;

            telas.Add(new FlyoutItens()
            {
                Titulo = "Home",
                Icone = "home.png",
                PaginaAlvo = typeof(Home)
            });

            // FAZER SELECT PARA PEGAR QUAIS TELAS O OPERADOR TEM ACESSO....

            telas.Add(new FlyoutItens()
            {
                Titulo = "Cliente/Fornecedor",
                Icone = "cliente.png",
                PaginaAlvo = typeof(Cliente)
            });

            telas.Add(new FlyoutItens()
            {
                Titulo = "Produto",
                Icone = "produto.png",
                PaginaAlvo = typeof(Produto)
            });

            telas.Add(new FlyoutItens()
            {
                Titulo = "Categoria",
                Icone = "categoria.png",
                PaginaAlvo = typeof(Categorias)
            });

            telas.Add(new FlyoutItens()
            {
                Titulo = "Forma Pagamento",
                Icone = "formaPgto.png",
                PaginaAlvo = typeof(FormaPgto)
            });

            telas.Add(new FlyoutItens()
            {
                Titulo = "Sobre",
                Icone = "sobre.png",
                PaginaAlvo = typeof(Sobre)
            });

        }
    }
}