using Newtonsoft.Json;
using SistemaDoLeo.DB;
using SistemaDoLeo.Modelos.Classes;
using SistemaDoLeo.Toast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
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
        
        private string operacao;

        private int Status;
        private int Visualizar = 0;
        private int Cadastro = 1;
        private int Editar = 2;

        private string Titulo = "Pedidos";

        private List<PedidoDetalhado> listaPedidos = new List<PedidoDetalhado>();
        private List<Cliente> listaClientes = new List<Cliente>();
        private List<FormaPgto> listaPgtos = new List<FormaPgto>();
        private ProximoRegistro proximoRegistro;

        private Cliente clienteAtual;
        private FormaPgto pgtoAtual;

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

            listaPedidos.Clear();

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
            var json = await _client.GetStringAsync(urlPgto);
            listaPgtos = JsonConvert.DeserializeObject<List<FormaPgto>>(json);
        }

        private Task ValidarOperacao()
        {
            if (RadioVenda.IsChecked)
            {
                operacao = "Venda";

                LblCliente.Text = "Cliente";
                TxtCliente.Placeholder = "Selecione um Cliente";

                return Task.CompletedTask;
            }
            else
            {
                operacao = "Compra";

                LblCliente.Text = "Fornec";
                TxtCliente.Placeholder = "Selecione um Fornecedor";

                return Task.CompletedTask;
            }
        }

        private void SrcBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            CvListagem.ItemsSource = listaPedidos.Where(p => p.ClienteNome.ToLower().Contains(SrcBuscar.Text.ToLower())).ToList();
        }

        private async void RefreshV_Refreshing(object sender, EventArgs e)
        {
            await CarregaListaPedidos();
        }

        private async void CvListagem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentPage = Children[1];

            var selecionado = (PedidoDetalhado)e.CurrentSelection.FirstOrDefault();

            if (selecionado != null)
            {
                await AbrirProdutos(false);

                if (selecionado.TipoOperacao.Equals("Venda"))
                {
                    RadioVenda.IsChecked = true;
                }
                else
                {
                    RadioCompra.IsChecked = true;
                }
                TxtCodigo.Text = selecionado.Id.ToString();
                PkrData.Date = selecionado.Data;
                AtualizaCliente(selecionado.ClienteId);
                AtualizaFormaPgto(selecionado.FormaPgtoId);
                TxtValor.Text = selecionado.Valor.ToString("C2");
                TxtDesconto.Text = selecionado.Desconto.ToString("F2") + "%";
                TxtTotal.Text = selecionado.Total.ToString("C2");

                await validaStatus(Visualizar);

                // limpa o selecionado para poder selecionar o mesmo novamente
                CvListagem.SelectedItem = null;
            }
        }

        public void AtualizaCliente(int id)
        {
            clienteAtual = listaClientes.FirstOrDefault(c => c.Id == id);

            TxtCliente.Text = clienteAtual.Nome;
        }

        public void AtualizaFormaPgto(int id)
        {
            pgtoAtual = listaPgtos.FirstOrDefault(c => c.Id == id);

            TxtPgto.Text = pgtoAtual.Nome;
        }

        private async Task validaStatus(int status)
        {
            if (status == Visualizar)
            {
                this.Status = Visualizar;

                RadioCompra.IsEnabled = false;
                RadioVenda.IsEnabled = false;
                TxtCodigo.IsEnabled = false;
                PkrData.IsEnabled = false;
                TxtCliente.IsEnabled = false;
                TxtPgto.IsEnabled = false;
                TxtValor.IsEnabled = false;
                TxtDesconto.IsEnabled = false;
                TxtTotal.IsEnabled = false;

                BtnNovo.Text = "Novo";

                BtnEditar.IsEnabled = true;
                BtnNovo.IsEnabled = true;
                BtnSalvar.IsEnabled = false;
            }
            else if (status == Editar)
            {
                this.Status = Editar;

                RadioCompra.IsEnabled = false;
                RadioVenda.IsEnabled = false;
                TxtCodigo.IsEnabled = false;
                PkrData.IsEnabled = true;
                TxtCliente.IsEnabled = false;
                TxtPgto.IsEnabled = false;
                TxtValor.IsEnabled = true;
                TxtDesconto.IsEnabled = true;
                TxtTotal.IsEnabled = false;

                BtnEditar.IsEnabled = false;
                BtnNovo.IsEnabled = true;
                BtnSalvar.IsEnabled = true;

            }
            else
            {
                this.Status = Cadastro;

                RadioCompra.IsEnabled = true;
                RadioVenda.IsEnabled = true;
                TxtCodigo.IsEnabled = false;
                PkrData.IsEnabled = true;
                TxtCliente.IsEnabled = false;
                TxtPgto.IsEnabled = false;
                TxtValor.IsEnabled = true;
                TxtDesconto.IsEnabled = true;
                TxtTotal.IsEnabled = false;

                BtnNovo.Text = "Limpar";

                BtnEditar.IsEnabled = false;
                BtnNovo.IsEnabled = true;
                BtnSalvar.IsEnabled = true;
            }
        }

        private async void SwDeletar_Invoked(object sender, EventArgs e)
        {
            var selecionado = (sender as SwipeItem)?.BindingContext as PedidoDetalhado;

            if (selecionado == null)
            {
                await DisplayAlert(Titulo, "Nenhum item selecionado", "Ok");

                return;
            }

            var confirmacao = await DisplayAlert(Titulo, $"Deseja realmente fazer a exclusão do Registro: {selecionado.Id}\n" +
                $"Cliente: {selecionado.ClienteNome}\n" +
                $"Forma Pgto: {selecionado.FormaPgtoNome}?", "Confirmar", "Cancelar");

            if (confirmacao)
            {
                var response = await _client.DeleteAsync($"{url}/{selecionado.Id}");

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, $"Ocorreu um erro ao deletar o registro: {selecionado.Id}\n" +
                        $"Cliente: {selecionado.ClienteNome}\n" +
                        $"Forma Pgto: {selecionado.FormaPgtoNome}", "Ok");

                    return;
                }

                listaPedidos.Remove(selecionado);

                // LIMPA OS CAMPOS DO CADASTRO PARA NÃO DEIXAR EDITAR O ITEM EXCLUIDO
                LimpaCampos();

                if (SrcBuscar.Text == null)
                {
                    CvListagem.ItemsSource = new List<PedidoDetalhado>(listaPedidos);
                }
                else
                {
                    CvListagem.ItemsSource = new List<PedidoDetalhado>(listaPedidos.Where(l => l.ClienteNome.ToLower().Contains(SrcBuscar.Text.ToLower())).ToList());
                }

                new ToastBase(Titulo, "Registro Deletado", $"Registro: {selecionado.Id} - {selecionado.ClienteNome} deletado com Sucesso" +
                    $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
            }
            else
            {
                return;
            }
        }

        private void PkrData_DateSelected(object sender, DateChangedEventArgs e)
        {

        }

        private async void BtnNovo_Clicked(object sender, EventArgs e)
        {
            LimpaCampos();

            await validaStatus(Cadastro);

            // PEGA PROXIMO REGISTRO
            var id = await PegaProximoRegistro();

            TxtCodigo.Text = id.ToString();
        }

        private async Task<int> PegaProximoRegistro()
        {
            var json = await _client.GetStringAsync(Links.proximoRegistro);

            proximoRegistro = JsonConvert.DeserializeObject<ProximoRegistro>(json);

            return proximoRegistro.Pedido += 1;
        }

        private async void BtnEditar_Clicked(object sender, EventArgs e)
        {
            if (TxtCodigo.Text == "" || TxtCodigo.Text == null)
            {
                await DisplayAlert(Titulo, "Necessário selecionar um registro", "Ok");

                return;
            }

            await validaStatus(Editar);
        }

        private async Task<bool> validaCampos()
        {
            if (TxtCodigo.Text == "" || TxtCodigo.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Código\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return false;
            }
            else if (TxtCliente.Text == "" || TxtCliente.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Cliente\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtCliente.TextColor = Color.Red;

                await Task.Delay(500);

                TxtCliente.TextColor = Color.Black;

                return false;
            }
            else if (TxtPgto.Text == "" || TxtPgto.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Forma de Pagamento\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtPgto.TextColor = Color.Red;

                await Task.Delay(500);

                TxtPgto.TextColor = Color.Black;

                return false;
            }
            else if (TxtValor.Text == "" || TxtValor.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Valor\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtValor.Focus();

                return false;
            }
            else if (TxtDesconto.Text == "" || TxtDesconto.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Desconto\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtDesconto.Focus();

                return false;
            }

            return true;
        }

        private async void BtnSalvar_Clicked(object sender, EventArgs e)
        {
            if (await validaCampos() == false)
            {
                return;
            }

            Pedido pedido = new Pedido();

            var valor = Convert.ToDecimal(await LimpaValores(TxtValor.Text));
            var desconto = Convert.ToDecimal(await LimpaValores(TxtDesconto.Text));
            var total = Convert.ToDecimal(await LimpaValores(TxtTotal.Text));

            if (Status == Cadastro)
            {
                pedido = new Pedido
                {
                    ClienteId = clienteAtual.Id,
                    FormaPgtoId = pgtoAtual.Id,
                    TipoOperacao = operacao,
                    Data = PkrData.Date,
                    Valor = valor,
                    Desconto = desconto,
                    Total = total
                };

            }
            else if (Status == Editar)
            {
                pedido = new Pedido
                {
                    Id = Convert.ToInt32(TxtCodigo.Text),
                    ClienteId = clienteAtual.Id,
                    FormaPgtoId = pgtoAtual.Id,
                    TipoOperacao = operacao,
                    Data = PkrData.Date,
                    Valor = valor,
                    Desconto = desconto,
                    Total = total
                };
            }

            if (await SalvarRegistro(pedido))
            {
                await validaStatus(Visualizar);
            }
        }

        private async Task<bool> SalvarRegistro(Pedido pedido)
        {
            var json = JsonConvert.SerializeObject(pedido);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            if (pedido.Id == 0)
            {
                var response = await _client.PostAsync(url, conteudo);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, "Ocorreu um erro no Cadastro", "Ok");

                    return false;
                }

                var novoRegistro = JsonConvert.DeserializeObject<Pedido>(await response.Content.ReadAsStringAsync());

                await AtualizaProximoRegistro(novoRegistro.Id);

                TxtCodigo.Text = novoRegistro.Id.ToString();

                listaPedidos.Add(new PedidoDetalhado()
                {
                    Id = novoRegistro.Id,
                    ClienteId = novoRegistro.ClienteId,
                    FormaPgtoId = novoRegistro.FormaPgtoId,
                    ClienteNome = listaClientes.FirstOrDefault(c => c.Id == novoRegistro.ClienteId).Nome,
                    FormaPgtoNome = listaPgtos.FirstOrDefault(c => c.Id == novoRegistro.FormaPgtoId).Nome,
                    Data = novoRegistro.Data,
                    TipoOperacao = novoRegistro.TipoOperacao,
                    Valor = novoRegistro.Valor,
                    Desconto = novoRegistro.Desconto,
                    Total = novoRegistro.Total
                });

                CvListagem.ItemsSource = new List<PedidoDetalhado>(listaPedidos);

                new ToastBase(Titulo, "Registro Salvo", $"Registro salvo com Sucesso!\n" +
                    $"Código: {novoRegistro.Id}\n" +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
            }
            else
            {
                var response = await _client.PutAsync($"{url}/{pedido.Id}", conteudo);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, "Ocorreu um erro ao fazer a Alteração", "Ok");

                    return false;
                }

                listaPedidos.Remove(listaPedidos.FirstOrDefault(l => l.Id == pedido.Id));

                listaPedidos.Add(new PedidoDetalhado()
                {
                    Id = pedido.Id,
                    ClienteId = pedido.ClienteId,
                    FormaPgtoId = pedido.FormaPgtoId,
                    ClienteNome = listaClientes.FirstOrDefault(c => c.Id == pedido.ClienteId).Nome,
                    FormaPgtoNome = listaPgtos.FirstOrDefault(c => c.Id == pedido.FormaPgtoId).Nome,
                    Data = pedido.Data,
                    TipoOperacao = pedido.TipoOperacao,
                    Valor = pedido.Valor,
                    Desconto = pedido.Desconto,
                    Total = pedido.Total
                });

                CvListagem.ItemsSource = new List<PedidoDetalhado>(listaPedidos).OrderBy(i => i.Id);

                new ToastBase(Titulo, "Registro Atualizado", $"Registro atualizado com Sucesso!\n" +
                    $"Código: {TxtCodigo.Text}\n" +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
            }

            return true;
        }

        private async Task AtualizaProximoRegistro(int id)
        {
            // ATUALIZA O PROXIMO REGISTRO COM BASE NO RETORNO DO POST
            proximoRegistro.Pedido = id;

            var json = JsonConvert.SerializeObject(proximoRegistro);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            await _client.PutAsync(Links.proximoRegistro, conteudo);
        }

        private async void TxtValor_Unfocused(object sender, FocusEventArgs e)
        {
            if(TxtValor.Text is null || TxtValor.Text == "")
            {
                TxtValor.Text = 0.00.ToString("C2");

                return;
            }

            var valor = await LimpaValores(TxtValor.Text);
              
            if (valor != "")
            {
                var valorFormatado = Convert.ToDecimal(valor);

                TxtValor.Text = valorFormatado.ToString("C2");
            }
            else
            {
                TxtValor.Text = 0.00.ToString("C2");
            }

            await RecalcularValores();
        }

        private async void TxtDesconto_Unfocused(object sender, FocusEventArgs e)
        {
            if (TxtDesconto.Text is null || TxtDesconto.Text == "")
            {
                TxtDesconto.Text = 0.00.ToString("F2") + "%";

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

            await RecalcularValores();
        }

        private async void TxtTotal_Unfocused(object sender, FocusEventArgs e)
        {
            if (TxtTotal.Text is null || TxtTotal.Text == "")
            {
                TxtTotal.Text = 0.00.ToString("C2");

                return;
            }

            var valor = await LimpaValores(TxtTotal.Text);

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

        private async void TapCliente_Tapped(object sender, EventArgs e)
        {
            if(Status != Visualizar)
            {
                await Navigation.PushAsync(new Pesquisar(this, Pesquisar.TiposPesquisas.Clientes));
            }
        }

        private async void TapPgto_Tapped(object sender, EventArgs e)
        {
            if (Status != Visualizar)
            {
                await Navigation.PushAsync(new Pesquisar(this, Pesquisar.TiposPesquisas.FormasPgto));
            }
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

        private async void BtnAddProduto_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Pesquisar(this, Pesquisar.TiposPesquisas.Produtos));
        }

        public PedidoDetalhado GetPedido()
        {
            return listaPedidos.FirstOrDefault(l => l.Id == Convert.ToInt32(TxtCodigo.Text));
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

        private void LimpaCampos()
        {
            RadioVenda.IsChecked = true;
            TxtCodigo.Text = string.Empty;
            PkrData.Date = DateTime.Today;
            TxtCliente.Text = string.Empty;
            TxtPgto.Text = string.Empty;
            TxtValor.Text = (0.00).ToString("C2");
            TxtDesconto.Text = (0.00).ToString("F2") + "%";
            TxtTotal.Text = (0.00).ToString("C2");
        }
        
        private async Task<string> LimpaValores(string valor)
        {
            return regex.Replace(valor, "");
        }

        private async Task RecalcularValores()
        {
            var valor = Convert.ToDecimal(await LimpaValores(TxtValor.Text));
            var desconto = Convert.ToDecimal(await LimpaValores(TxtDesconto.Text));

            var total = valor - (valor * (desconto *  Convert.ToDecimal(0.01)));

            TxtTotal.Text = total.ToString("C2");
        }
    }
}