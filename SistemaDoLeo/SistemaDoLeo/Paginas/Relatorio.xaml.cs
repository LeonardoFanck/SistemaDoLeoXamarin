using Newtonsoft.Json;
using Plugin.XamarinFormsSaveOpenPDFPackage;
using SistemaDoLeo.DB;
using SistemaDoLeo.Modelos.Classes;
using SistemaDoLeo.Relatorios;
using SistemaDoLeo.Toast;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static SistemaDoLeo.Paginas.AddProdutos;

namespace SistemaDoLeo.Paginas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Relatorio : ContentPage
    {
        private Cliente clienteSelecionado;
        private FormaPgto pgtoSelecionado;
        
        private List<PedidoDetalhado> listaPedidosRelatorio = new List<PedidoDetalhado>();

        private List<PedidoDetalhado> listaPedidos = new List<PedidoDetalhado>();
        private List<Cliente> listaClientes = new List<Cliente>();
        private List<FormaPgto> listaPgtos = new List<FormaPgto>();

        private HttpClient _client;

        private string Titulo = "Relatório";

        private string tipoOperacao;

        private string urlPedido = $"{Links.ip}/{Links.pedido}";
        private string urlCliente = $"{Links.ip}/{Links.cliente}";
        private string urlPgto = $"{Links.ip}/{Links.formaPgto}";

        public Relatorio()
        {
            InitializeComponent();

            RadioVenda.IsChecked = true;
            
            _client = new HttpClient(PermissaoDeCertificado.GetInsecureHandler());
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await CarregaListaClientes();

            await CarregaListaPgto();

            await ValidarTipoOperacao();
        }

        private async Task CarregaListaPedidos()
        {
            var json = await _client.GetStringAsync(urlPedido);
            var pedidos = JsonConvert.DeserializeObject<List<Pedido>>(json);

            listaPedidos.Clear();

            foreach(var pedido in pedidos)
            {
                listaPedidos.Add(new PedidoDetalhado()
                {
                    Id = pedido.Id,
                    ClienteId = pedido.ClienteId,
                    ClienteNome = listaClientes.FirstOrDefault(l => l.Id == pedido.ClienteId).Nome,
                    Data = pedido.Data,
                    FormaPgtoId = pedido.FormaPgtoId,
                    FormaPgtoNome = listaPgtos.FirstOrDefault(l => l.Id == pedido.FormaPgtoId).Nome,
                    TipoOperacao = pedido.TipoOperacao,
                    Valor = pedido.Valor,
                    Desconto = pedido.Desconto,
                    Total = pedido.Total
                });
            }
        }

        private async Task CarregaListaClientes()
        {
            var json = await _client.GetStringAsync(urlCliente);
            listaClientes = JsonConvert.DeserializeObject<List<Cliente>>(json);
        }

        private async Task CarregaListaPgto()
        {
            var json = await _client.GetStringAsync(urlPgto);
            listaPgtos = JsonConvert.DeserializeObject<List<FormaPgto>>(json);
        }

        private void PkrDataInicial_DateSelected(object sender, DateChangedEventArgs e)
        {

        }

        private void PkrDataFinal_DateSelected(object sender, DateChangedEventArgs e)
        {

        }

        private async void BtnLimpar_Clicked(object sender, EventArgs e)
        {
            await LimpaCampos();
        }

        private async void RadioCompra_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            clienteSelecionado = null;
            TxtCliente.Text = string.Empty;

            await ValidarTipoOperacao();
        }

        private Task ValidarTipoOperacao()
        {
            if (RadioVenda.IsChecked)
            {
                tipoOperacao = "Venda";

                LblCliente.Text = "Cliente";
                TxtCliente.Placeholder = "Selecione um Cliente";

                return Task.CompletedTask;
            }
            else
            {
                tipoOperacao = "Compra";

                LblCliente.Text = "Fornec";
                TxtCliente.Placeholder = "Selecione um Fornecedor";

                return Task.CompletedTask;
            }
        }

        private async void TapCliente_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Pesquisar(this, Pesquisar.TiposPesquisas.Clientes));
        }

        private async void TapPgto_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Pesquisar(this, Pesquisar.TiposPesquisas.FormasPgto));
        }

        public async Task<string> GetTipoPedido()
        {
            if (RadioVenda.IsChecked)
            {
                return "Venda";
            }
            else
            {
                return "Compra";
            }
        }

        public void AtualizaCliente(int id)
        {
            clienteSelecionado = listaClientes.FirstOrDefault(c => c.Id == id);

            TxtCliente.Text = clienteSelecionado.Nome;
        }

        public void AtualizaFormaPgto(int id)
        {
            pgtoSelecionado = listaPgtos.FirstOrDefault(c => c.Id == id);

            TxtPgto.Text = pgtoSelecionado.Nome;

            TxtPgto.FontAttributes = FontAttributes.Bold;
            TxtPgto.TextColor = Color.Black;
        }

        private async void BtnPesquisar_Clicked(object sender, EventArgs e)
        {
            if(!await GerarPesquisa())
            {
                new ToastBase(Titulo, "Nenhum registro localizado", $"Nenhum registro localizado na pesquisa solicitada" +
                        $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            PDFGenerator pdf = new PDFGenerator();

            var stream = await pdf.RelatorioPedidos(listaPedidosRelatorio, tipoOperacao);

            await CrossXamarinFormsSaveOpenPDFPackage.Current.SaveAndView("RelatorioPedidos.pdf", "application/pdf",
                stream, PDFOpenContext.ChooseApp);
        }

        private async Task<bool> GerarPesquisa()
        {
            await CarregaListaPedidos();

            var dataInicio = PkrDataInicial.Date;
            var dataFinal = PkrDataFinal.Date.AddHours(23.99);

            var pedidos = listaPedidos.Where(l => l.TipoOperacao.Equals(tipoOperacao) && l.Data >= dataInicio && l.Data <= dataFinal);

            if(clienteSelecionado != null)
            {
                pedidos = pedidos.Where(l => l.ClienteId == clienteSelecionado.Id);
            }

            if(pgtoSelecionado != null)
            {
                pedidos = pedidos.Where(l => l.FormaPgtoId == pgtoSelecionado.Id);
            }

            if(pedidos.Count() == 0)
            {
                return false;
            }

            listaPedidosRelatorio = pedidos.ToList();

            return true;
        }

        private void BtnLimpaCliente_Clicked(object sender, EventArgs e)
        {
            clienteSelecionado = null;
            TxtCliente.Text = string.Empty;
        }

        private void BtnLimpaPgto_Clicked(object sender, EventArgs e)
        {
            pgtoSelecionado = null;
            TxtPgto.Text = string.Empty;
        }

        private Task LimpaCampos()
        {
            RadioVenda.IsChecked = true;
            PkrDataInicial.Date = DateTime.Today;
            PkrDataFinal.Date = DateTime.Today;
            clienteSelecionado = null;
            pgtoSelecionado = null;
            TxtCliente.Text = string.Empty;
            TxtPgto.Text = string.Empty;

            return Task.CompletedTask;
        }
    }
}