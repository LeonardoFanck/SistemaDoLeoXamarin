using Newtonsoft.Json;
using SistemaDoLeo.DB;
using SistemaDoLeo.Modelos.Classes;
using SistemaDoLeo.Toast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static System.Net.Mime.MediaTypeNames;

namespace SistemaDoLeo.Paginas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddProdutos : ContentPage
    {
        public enum TipoOperacao
        {
            compra,
            venda
        }

        private HttpClient _client;

        private PedidoDetalhado pedido;
        private TipoOperacao tipoOperacao;
        private object tela;
        private Produto produto; 
        private PedidoItemDetalhado item;

        private string url = $"{Links.ip}/PedidoItems";

        private string Titulo = "Produto do Pedido";

        Regex regex = new Regex("[^0-9.,]");

        private int status;
        private int novo = 0;
        private int editar = 1;

        // UTILIZADO PARA ADICIONAR UM NOVO PRODUTO
        public AddProdutos(object tela, Produto produto, PedidoDetalhado pedido)
        {
            InitializeComponent();

            Title = "Adicionar produto";

            status = novo;

            this.tela = tela;
            this.produto = produto;
            this.pedido = pedido;

        }

        // UTILIZADO PARA ALTERAR UM ITEM
        public AddProdutos(object tela, PedidoItemDetalhado item, PedidoDetalhado pedido)
        {
            InitializeComponent();

            Title = "Editar produto";

            status = editar;

            this.tela = tela;
            this.item = item;
            this.pedido = pedido;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            HttpClientHandler httpClientHandler = PermissaoDeCertificado.GetInsecureHandler();
            _client = new HttpClient(httpClientHandler);

            await ValidaTipoOperacao();

            await PreencheProduto();   
        }

        private async Task ValidaTipoOperacao()
        {
            if (pedido.TipoOperacao.Equals("Venda"))
            {
                tipoOperacao = TipoOperacao.venda;
            }
            else
            {
                tipoOperacao = TipoOperacao.compra;
            }
        }

        private async Task PreencheProduto()
        {
            if(status == novo)
            {
                TxtCodigo.Text = produto.Id.ToString();
                TxtNome.Text = produto.Nome;
                if(tipoOperacao == TipoOperacao.venda)
                {
                    TxtValor.Text = produto.Preco.ToString("C2");
                }
                else if(tipoOperacao == TipoOperacao.compra)
                {
                    TxtValor.Text = produto.Custo.ToString("C2");
                }
                TxtQuantidade.Text = (1).ToString();
                TxtDesconto.Text = (0.00).ToString("F2") + "%";
            }
            else if(status == editar)
            {
                TxtCodigo.Text = item.ProdutoId.ToString();
                TxtNome.Text = item.ProdutoNome;
                TxtValor.Text = item.Valor.ToString("C2");
                TxtQuantidade.Text = item.Quantidade.ToString();
                TxtDesconto.Text = item.Desconto.ToString("F2") + "%";
            }

            await CalcularValor();
        }

        private async Task CalcularValor()
        {
            var valor = Convert.ToDecimal(await LimpaValores(TxtValor.Text));
            var quantidade = Convert.ToInt32(await LimpaValores(TxtQuantidade.Text));
            var desconto = Convert.ToDecimal(await LimpaValores(TxtDesconto.Text));

            var total = (valor * quantidade) - (quantidade * (valor * (desconto * Convert.ToDecimal(0.01))));

            TxtTotal.Text = total.ToString("C2");
        }

        private async Task<string> LimpaValores(string valor)
        {
            return regex.Replace(valor, "");
        }

        private async void TxtValor_Unfocused(object sender, FocusEventArgs e)
        {
            if (TxtValor.Text is null || TxtValor.Text == "")
            {
                TxtValor.Text = 0.00.ToString("C2");

                await CalcularValor();

                return;
            }

            var valor = await LimpaValores(TxtValor.Text);

            if (valor != "")
            {
                decimal valorFormatado = Convert.ToDecimal(valor);

                TxtValor.Text = valorFormatado.ToString("C2");
            }
            else
            {
                TxtValor.Text = 0.00.ToString("C2");
            }

            await CalcularValor();
        }

        private async void TxtQuantidade_Unfocused(object sender, FocusEventArgs e)
        {
            if (TxtQuantidade.Text is null || TxtQuantidade.Text == "")
            {
                TxtQuantidade.Text = (1).ToString();

                await CalcularValor();

                return;
            }

            var quantidade = await LimpaValores(TxtQuantidade.Text);

            if(quantidade != "")
            {
                TxtQuantidade.Text = quantidade.ToString();
            }
            else
            {
                TxtQuantidade.Text = (1).ToString();
            }

            await CalcularValor();
        }

        private async void TxtDesconto_Unfocused(object sender, FocusEventArgs e)
        {
            if (TxtDesconto.Text is null || TxtDesconto.Text == "")
            {
                TxtDesconto.Text = 0.00.ToString("F2") + "%";

                await CalcularValor();

                return;
            }

            var valor = await LimpaValores(TxtDesconto.Text);

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
            }

            await CalcularValor();
        }

        private async void BtnConfirmar_Clicked(object sender, EventArgs e)
        {
            PedidoItem item = new PedidoItem();

            if(status == novo)
            {
                item = new PedidoItem()
                {
                    PedidoId = pedido.Id,
                    ProdutoId = Convert.ToInt32(TxtCodigo.Text),
                    Valor = Convert.ToDecimal(await LimpaValores(TxtValor.Text)),
                    Quantidade = Convert.ToInt32(TxtQuantidade.Text),
                    Desconto = Convert.ToDecimal(await LimpaValores(TxtDesconto.Text)),
                    Total = Convert.ToDecimal(await LimpaValores(TxtTotal.Text)),
                };
            }
            else if(status == editar)
            {
                item = new PedidoItem()
                {
                    Id = this.item.Id,
                    PedidoId = pedido.Id,
                    ProdutoId = Convert.ToInt32(TxtCodigo.Text),
                    Valor = Convert.ToDecimal(await LimpaValores(TxtValor.Text)),
                    Quantidade = Convert.ToInt32(TxtQuantidade.Text),
                    Desconto = Convert.ToDecimal(await LimpaValores(TxtDesconto.Text)),
                    Total = Convert.ToDecimal(await LimpaValores(TxtTotal.Text)),
                };
            }

            if(await SalvarItem(item))
            {
                await Navigation.PopAsync();
            }
        }

        private async Task<bool> SalvarItem(PedidoItem item)
        {
            var json = JsonConvert.SerializeObject(item);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            PedidoItemDetalhado novoItemDetalhado = new PedidoItemDetalhado();

            if (status == novo)
            {
                var response = await _client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    new ToastBase(Titulo, "Ocorreu um erro ao adicionar o item", $"Ocorreu um erro ao adicionar o item, favor tente novamente" +
                        $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                    return false;
                }

                var novoItem = JsonConvert.DeserializeObject<PedidoItem>(await response.Content.ReadAsStringAsync());

                novoItemDetalhado = new PedidoItemDetalhado()
                {
                    Id = novoItem.Id,
                    PedidoId = novoItem.PedidoId,
                    ProdutoId = novoItem.ProdutoId,
                    ProdutoNome = TxtNome.Text,
                    Valor = novoItem.Valor,
                    Quantidade = novoItem.Quantidade,
                    Desconto = novoItem.Desconto,
                    Total = novoItem.Total
                };
            }
            else if (status == editar)
            {
                var response = await _client.PutAsync($"{url}/{item.Id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    new ToastBase(Titulo, "Ocorreu um erro ao alterar o item", $"Ocorreu um erro ao alterar o item, favor tente novamente" +
                        $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                    return false;
                }

                novoItemDetalhado = new PedidoItemDetalhado()
                {
                    Id = item.Id,
                    PedidoId = item.PedidoId,
                    ProdutoId = item.ProdutoId,
                    ProdutoNome = TxtNome.Text,
                    Valor = item.Valor,
                    Quantidade = item.Quantidade,
                    Desconto = item.Desconto,
                    Total = item.Total
                };
            }

            if (tela is Pedidos)
            {
                var tela = this.tela as Pedidos;
                tela.AddListaItens(novoItemDetalhado);
            }

            return true;
        }
    }
}