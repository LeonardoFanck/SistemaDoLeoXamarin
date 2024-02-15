using SistemaDoLeo.Modelos.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SistemaDoLeo.Paginas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Produtos : TabbedPage
    {
        public Produtos()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //await CarregaCategorias();
            //await CarregaLista();
        }

        private void SrcBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void CvListagem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SwDeletar_Invoked(object sender, EventArgs e)
        {

        }

        private void BtnSalvar_Clicked(object sender, EventArgs e)
        {

        }

        private void BtnNovo_Clicked(object sender, EventArgs e)
        {

        }

        private void BtnEditar_Clicked(object sender, EventArgs e)
        {

        }

        private void RefreshV_Refreshing(object sender, EventArgs e)
        {

        }
    }
}