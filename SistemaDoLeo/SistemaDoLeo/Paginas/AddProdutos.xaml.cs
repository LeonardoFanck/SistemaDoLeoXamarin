using SistemaDoLeo.Modelos.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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

        private TipoOperacao tipoOperacao;
        private object tela;
        private Produto produto; 
        private PedidoItemDetalhado item;

        Regex regex = new Regex("[^0-9.,]");

        private int status;
        private int novo = 0;
        private int editar = 1;

        // UTILIZADO PARA ADICIONAR UM NOVO PRODUTO
        public AddProdutos(object tela, Produto produto, TipoOperacao tipoOperacao)
        {
            InitializeComponent();

            status = novo;

            this.tela = tela;
            this.produto = produto;
            this.tipoOperacao = tipoOperacao;
        }

        // UTILIZADO PARA ALTERAR UM ITEM
        public AddProdutos(object tela, PedidoItemDetalhado item, TipoOperacao tipoOperacao)
        {
            InitializeComponent();

            status = editar;

            this.tela = tela;
            this.item = item;
            this.tipoOperacao = tipoOperacao;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await PreencheProduto();
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
                TxtQuantidade.Text = item.Quantidade.ToString("F2") + "%";
                TxtDesconto.Text = item.Desconto.ToString();
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
    }
}