using Newtonsoft.Json;
using SistemaDoLeo.DB;
using SistemaDoLeo.Modelos.Classes;
using SistemaDoLeo.Toast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SistemaDoLeo.Paginas
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Pesquisar : ContentPage
	{
		public enum TiposPesquisas
		{
			Clientes,
			FormasPgto,
			Produtos
		}

		private object tela;
		private TiposPesquisas tipo;

		HttpClient _client;

		private string url = $"{Links.ip}";
		private string urlBase = $"{Links.ip}";
		private readonly string urlCliente = "/Cliente";
		private readonly string urlPgto = "/FormaPgto";
		private readonly string urlProduto = "/Produto";

		private string Titulo = "Pesquisa";

		private List<Cliente> ListaCliente = new List<Cliente>();
		private List<FormaPgto> ListaPgto = new List<FormaPgto>();
		private List<Produto> ListaProduto = new List<Produto>();

		public Pesquisar (object tela, TiposPesquisas tipo)
		{
			InitializeComponent ();

			HttpClientHandler httpClientHandler = PermissaoDeCertificado.GetInsecureHandler();
			_client = new HttpClient(httpClientHandler);

			this.tela = tela;
			this.tipo = tipo;
		}

        protected override async void OnAppearing()
        {
            base.OnAppearing();

			await ValidarTipos();
        }

		private async Task ValidarTipos()
		{
			if(tipo == TiposPesquisas.Clientes)
			{
				url = urlBase + urlCliente;

				this.Title += " Clientes/Fornecedor";

				await ListarClientes();
			}
			else if(tipo == TiposPesquisas.FormasPgto)
			{
                url = urlBase + urlPgto;

                this.Title += " Forma de Pagamento";

                await ListarFormaPgto();
            }
			else if(tipo == TiposPesquisas.Produtos)
			{
                url = urlBase + urlProduto;

                this.Title += " Produto";

                await ListarProdutos();
            }
		}

		private async Task ListarClientes()
		{
			var json = await _client.GetStringAsync(url);
			ListaCliente = JsonConvert.DeserializeObject<List<Cliente>>(json);
			SrcBuscar.Text = string.Empty;
			Listagem.ItemsSource = ListaCliente.Where(l => l.Inativo == false);
		}

        private async Task ListarFormaPgto()
        {
            var json = await _client.GetStringAsync(url);
            ListaPgto = JsonConvert.DeserializeObject<List<FormaPgto>>(json);
            SrcBuscar.Text = string.Empty;
            Listagem.ItemsSource = ListaPgto.Where(l => l.Inativo == false);
        }

        private async Task ListarProdutos()
        {
            var json = await _client.GetStringAsync(url);
            ListaProduto = JsonConvert.DeserializeObject<List<Produto>>(json);
            SrcBuscar.Text = string.Empty;
            Listagem.ItemsSource = ListaProduto.Where(l => l.Inativo == false);
        }

        private async void RefreshListagem_Refreshing(object sender, EventArgs e)
        {
            if (tipo == TiposPesquisas.Clientes)
            {
				await ListarClientes();
            }
            else if (tipo == TiposPesquisas.FormasPgto)
            {
				await ListarFormaPgto();
            }
            else if (tipo == TiposPesquisas.Produtos)
            {
				await ListarProdutos();
            }
        }

        private void Listagem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void SrcBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tipo == TiposPesquisas.Clientes)
			{
				Listagem.ItemsSource = ListaCliente.Where(l => l.Nome.ToLower().Contains(SrcBuscar.Text.ToLower()));
			}
            else if (tipo == TiposPesquisas.FormasPgto)
			{
                Listagem.ItemsSource = ListaPgto.Where(l => l.Nome.ToLower().Contains(SrcBuscar.Text.ToLower()));
            }
            else if (tipo == TiposPesquisas.Produtos)
			{
                Listagem.ItemsSource = ListaProduto.Where(l => l.Nome.ToLower().Contains(SrcBuscar.Text.ToLower()));
            }
        }

        private async void BtnConfirmar_Clicked(object sender, EventArgs e)
        {
			if(Listagem.SelectedItem == null)
			{
                new ToastBase(Titulo, "Necessário selecionar um item", $"Necessário selecionar um item para confirmar, caso deseje cancelar " +
					$"a operação, favor utilizar o botão de voltar no canto superior esquerdo." +
                    $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
			}

            if (tipo == TiposPesquisas.Clientes)
            {
                if(tela is Pedidos)
				{
					var cliente = Listagem.SelectedItem as Cliente;
					var pedido = tela as Pedidos;
                    pedido.AtualizaCliente(cliente.Id);
				}
            }
            else if (tipo == TiposPesquisas.FormasPgto)
            {
				if(tela is Pedidos)
				{
					var pgto = Listagem.SelectedItem as FormaPgto;
					var pedido = tela as Pedidos;
					pedido.AtualizaFormaPgto(pgto.Id);
				}
            }
            else if (tipo == TiposPesquisas.Produtos)
            {
				if(tela is Pedidos)
				{
					var prod = Listagem.SelectedItem as Produto;
					var pedido = tela as Pedidos;

					await Navigation.PushAsync(new AddProdutos(pedido, prod, pedido.GetPedido()));
				}

				return;
            }
			
			await Navigation.PopAsync();
        }
    }
}