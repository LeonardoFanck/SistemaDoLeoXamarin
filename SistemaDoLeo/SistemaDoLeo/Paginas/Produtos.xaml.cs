using Newtonsoft.Json;
using SistemaDoLeo.DB;
using SistemaDoLeo.Modelos.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        private int Status;
        private int Visualizar = 0;
        private int Cadastro = 1;
        private int Editar = 2;

        private List<Categoria> listaCategorias = new List<Categoria>();
        private List<Produto> listaProdutos = new List<Produto>();

        private readonly HttpClient _client;
        private string urlCategoria = $"{Links.ip}/Categoria";
        private string url = $"{Links.ip}/Produto";

        public Produtos()
        {
            InitializeComponent();

            BindingContext = this;

            CurrentPage = Children[0];

            HttpClientHandler httpClientHandler = PermissaoDeCertificado.GetInsecureHandler();
            _client = new HttpClient(httpClientHandler);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await CarregaListaProdutos();
            await CarregaCategorias();
        }

        private async Task CarregaCategorias()
        {
            var json = await _client.GetStringAsync(urlCategoria);
            listaCategorias = JsonConvert.DeserializeObject<List<Categoria>>(json);
            PkrCategoria.ItemsSource = listaCategorias;
        }

        private async Task CarregaListaProdutos()
        {
            RefreshV.IsRefreshing = true;

            var json = await _client.GetStringAsync(url);
            listaProdutos = JsonConvert.DeserializeObject<List<Produto>>(json);
            CvListagem.ItemsSource = listaProdutos;

            RefreshV.IsRefreshing = false;
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

        private async void RefreshV_Refreshing(object sender, EventArgs e)
        {
            await CarregaListaProdutos();
        }
    }
}