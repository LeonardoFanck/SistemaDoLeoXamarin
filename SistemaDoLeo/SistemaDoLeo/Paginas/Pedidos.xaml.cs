using Newtonsoft.Json;
using SistemaDoLeo.DB;
using SistemaDoLeo.Modelos.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SistemaDoLeo.Paginas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Pedidos : TabbedPage
    {
        Regex regex = new Regex("[^0-9.,]");
        private readonly int compra = 0;
        private readonly int venda = 1;
        private int operacao;

        private int Status;
        private int Visualizar = 0;
        private int Cadastro = 1;
        private int Editar = 2;

        private string Titulo = "Pedidos";

        private List<PedidoDetalhado> listaPedidos = new List<PedidoDetalhado>();
        private List<Cliente> listaClientes = new List<Cliente>();
        private List<FormaPgto> listaPgtos = new List<FormaPgto>();
        private ProximoRegistro proximoRegistro;

        private readonly HttpClient _client;
        private string url = $"{Links.ip}/Pedido";
        private string urlCliente = $"{Links.ip}/Cliente";
        private string urlPgto = $"{Links.ip}/FormaPgto";

        public Pedidos()
        {
            InitializeComponent();

            BindingContext = this;

            CurrentPage = Children[0];

            HttpClientHandler httpClientHandler = PermissaoDeCertificado.GetInsecureHandler();
            _client = new HttpClient(httpClientHandler);
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            RadioVenda.IsChecked = true;

            await CarregaListaPedidos();

            await ValidarOperacao();
        }

        private async Task CarregaListaPedidos()
        {
            RefreshV.IsRefreshing = true;

            await CarregaListaClientes();
            await CarregaListaPgto();

            var json = await _client.GetStringAsync(url);
            var teste = JsonConvert.DeserializeObject<List<Pedido>>(json);

            foreach(var pedido in teste)
            {
                listaPedidos.Add(new PedidoDetalhado()
                {
                    Id = pedido.Id,
                    ClienteId = pedido.ClienteId,
                    FormaPgtoId = pedido.FormaPgtoId,
                    Data = pedido.Data,
                    Valor = pedido.Valor,
                    Desconto = pedido.Desconto,
                    Total = pedido.Total,
                    TipoOperacao = pedido.TipoOperacao,
                    ClienteNome = listaClientes.FirstOrDefault(l => l.Id == pedido.ClienteId).Nome,
                    FormaPgtoNome = listaPgtos.FirstOrDefault(l => l.Id == pedido.FormaPgtoId).Nome
                });
            }

            CvListagem.ItemsSource = listaPedidos;

            RefreshV.IsRefreshing = false;
        }

        private async Task CarregaListaClientes()
        {
            var json = await _client.GetStringAsync(urlCliente);
            listaClientes = JsonConvert.DeserializeObject<List<Cliente>>(json);
        }

        private async Task CarregaListaPgto()
        {
            var json = await _client.GetStringAsync(urlCliente);
            listaPgtos = JsonConvert.DeserializeObject<List<FormaPgto>>(json);
        }

        private Task ValidarOperacao()
        {
            if (RadioVenda.IsChecked)
            {
                operacao = venda;

                LblCliente.Text = "Cliente";
                TxtCliente.Placeholder = "Selecione um Cliente";

                return Task.CompletedTask;
            }
            else
            {
                operacao = compra;

                LblCliente.Text = "Fornec";
                TxtCliente.Placeholder = "Selecione um Fornecedor";

                return Task.CompletedTask;
            }
        }

        private void SrcBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            CvListagem.ItemsSource = listaPedidos.Where(p => p.ClienteNome.ToLower().Contains(SrcBuscar.Text.ToLower())).ToList();
        }

        private void RefreshV_Refreshing(object sender, EventArgs e)
        {

        }

        private void CvListagem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SwDeletar_Invoked(object sender, EventArgs e)
        {

        }

        private void PkrData_DateSelected(object sender, DateChangedEventArgs e)
        {

        }

        private void BtnNovo_Clicked(object sender, EventArgs e)
        {

        }

        private void BtnEditar_Clicked(object sender, EventArgs e)
        {

        }

        private void BtnSalvar_Clicked(object sender, EventArgs e)
        {

        }

        private void TxtValor_Unfocused(object sender, FocusEventArgs e)
        {
            if(TxtValor.Text is null || TxtValor.Text == "")
            {
                TxtValor.Text = 0.00.ToString("C2");

                return;
            }

            var valor = regex.Replace(TxtValor.Text, "");
              
            if (valor != "")
            {
                var valorFormatado = Convert.ToDecimal(valor);

                TxtValor.Text = valorFormatado.ToString("C2");
            }
            else
            {
                TxtValor.Text = 0.00.ToString("C2");

                return;
            }
        }

        private void TxtDesconto_Unfocused(object sender, FocusEventArgs e)
        {
            if (TxtDesconto.Text is null || TxtDesconto.Text == "")
            {
                TxtDesconto.Text = 0.00.ToString("F2") + "%";

                return;
            }

            var valor = regex.Replace(TxtDesconto.Text, "");

            if (valor != "")
            {
                decimal valorFormatado = Convert.ToDecimal(valor);
                if (valorFormatado > Convert.ToDecimal(100.00))
                {
                    valorFormatado = Convert.ToDecimal(100.00);
                }
                else
                {
                   valorFormatado = Convert.ToDecimal(valor);
                }

                TxtDesconto.Text = valorFormatado.ToString("F2") + "%";
            }
            else
            {
                TxtDesconto.Text = 0.00.ToString("F2") + "%";

                return;
            }
        }

        private void TxtTotal_Unfocused(object sender, FocusEventArgs e)
        {
            if (TxtTotal.Text is null || TxtTotal.Text == "")
            {
                TxtTotal.Text = 0.00.ToString("C2");

                return;
            }

            var valor = regex.Replace(TxtTotal.Text, "");

            if (valor != "")
            {
                var valorFormatado = Convert.ToDecimal(valor);

                TxtTotal.Text = valorFormatado.ToString("C2");
            }
            else
            {
                TxtTotal.Text = 0.00.ToString("C2");

                return;
            }
        }

        private void TapCliente_Tapped(object sender, EventArgs e)
        {
            
        }

        private void TapPgto_Tapped(object sender, EventArgs e)
        {

        }

        private async void RadioCompra_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            await ValidarOperacao();
        }

        private void SwDeleteProduto_Invoked(object sender, EventArgs e)
        {

        }

        private async void BtnFecharProdutos_Clicked(object sender, EventArgs e)
        {
            await AbrirProdutos(false);
        }

        private void BtnEditProduto_Clicked(object sender, EventArgs e)
        {

        }

        private void BtnAddProduto_Clicked(object sender, EventArgs e)
        {

        }

        private Task AbrirProdutos(bool abrir)
        {
            if (abrir)
            {
                StackLayoutProdutos.IsVisible = true;

                StackLayoutBotoes.IsVisible = false;
            }
            else
            {
                StackLayoutProdutos.IsVisible = false;

                StackLayoutBotoes.IsVisible = true;
            }

            return Task.CompletedTask;
        }

        private async void BtnProdutos_Clicked(object sender, EventArgs e)
        {
            await AbrirProdutos(true);
        }
    }
}