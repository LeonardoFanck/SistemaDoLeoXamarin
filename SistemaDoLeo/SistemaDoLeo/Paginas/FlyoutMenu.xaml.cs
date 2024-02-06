using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SistemaDoLeo.Paginas
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FlyoutMenu : ContentPage
	{
		private List<Classes.FlyoutItem> telas { get; set; } = new List<Classes.FlyoutItem>();


        public FlyoutMenu ()
		{
			InitializeComponent();

			validarTelas();
		}

		private void validarTelas()
		{
			lista.ItemsSource = telas;

			telas.Add(new Classes.FlyoutItem()
			{
				Titulo = "Home",
				Icone = "home.png",
				PaginaAlvo = typeof(Home)
			});

			// FAZER SELECT PARA PEGAR QUAIS TELAS O OPERADOR TEM ACESSO....

			telas.Add(new Classes.FlyoutItem()
			{
				Titulo = "Cliente/Fornecedor",
				Icone = "cliente.png",
				PaginaAlvo = typeof(Cliente)
			});

            telas.Add(new Classes.FlyoutItem()
            {
                Titulo = "Produto",
                Icone = "produto.png",
                PaginaAlvo = typeof(Produto)
            });

            telas.Add(new Classes.FlyoutItem()
            {
                Titulo = "Categoria",
                Icone = "categoria.png",
                PaginaAlvo = typeof(Categorias)
            });

            telas.Add(new Classes.FlyoutItem()
            {
                Titulo = "Forma Pagamento",
                Icone = "formaPgto.png",
                PaginaAlvo = typeof(FormaPgto)
            });

            telas.Add(new Classes.FlyoutItem()
            {
                Titulo = "Sobre",
                Icone = "sobre.png",
                PaginaAlvo = typeof(Sobre)
            });

        }
    }
}