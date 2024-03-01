using Newtonsoft.Json;
using Plugin.XamarinFormsSaveOpenPDFPackage;
using SistemaDoLeo.DB;
using SistemaDoLeo.Modelos.Classes;
using SistemaDoLeo.Relatorios;
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
using static SistemaDoLeo.Paginas.AddProdutos;

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
        private List<PedidoItemDetalhado> listaItens = new List<PedidoItemDetalhado>();
        private List<Produto> listaProdutos = new List<Produto>();
        private List<Cliente> listaClientes = new List<Cliente>();
        private List<FormaPgto> listaPgtos = new List<FormaPgto>();
        private ProximoRegistro proximoRegistro;

        private Cliente clienteAtual;
        private FormaPgto pgtoAtual;
        private PedidoDetalhado pedidoAtual;

        OperadorTela permissoes;

        private readonly HttpClient _client;
        private string url = $"{Links.ip}/Pedido";
        private string urlCliente = $"{Links.ip}/Cliente";
        private string urlPgto = $"{Links.ip}/FormaPgto";
        private string urlItens = $"{Links.ip}/PedidoItems";
        private string urlProdutos = $"{Links.ip}/Produto";

        public Pedidos(OperadorTela permissoes)
        {
            InitializeComponent();

            BindingContext = this;

            CurrentPage = Children[0];

            HttpClientHandler httpClientHandler = PermissaoDeCertificado.GetInsecureHandler();
            _client = new HttpClient(httpClientHandler);

            this.permissoes = permissoes;

            AbrirProdutos(false);
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            RadioVenda.IsChecked = true;

            await CarregaListaPedidos();

            await CarregaListaProdutos();

            await ValidarOperacao();
        }

        private async Task CarregaListaPedidos()
        {
            RefreshV.IsRefreshing = true;

            await CarregaListaClientes();
            await CarregaListaPgto();

            var json = await _client.GetStringAsync(url);
            var pedidos = JsonConvert.DeserializeObject<List<Pedido>>(json);

            listaPedidos.Clear();

            foreach(var pedido in pedidos)
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

        private async Task<List<PedidoItem>> GetItensParaDelete(int idPedido)
        {
            await CarregaListaProdutos();

            var json = await _client.GetStringAsync($"{urlItens}/Pedido/{idPedido}");
            var itens = JsonConvert.DeserializeObject<List<PedidoItem>>(json);

            List<PedidoItem> lista = new List<PedidoItem>();

            foreach (var item in itens)
            {
                lista.Add(new PedidoItem()
                {
                    Id = item.Id,
                    PedidoId = item.PedidoId,
                    ProdutoId = item.ProdutoId,
                    Valor = item.Valor,
                    Quantidade = item.Quantidade,
                    Desconto = item.Desconto,
                    Total = item.Total
                });
            }

            return lista;
        }


        private async Task CarregaListaItens(int idPedido)
        {
            RefreshItens.IsRefreshing = true;

            await CarregaListaProdutos();

            var json = await _client.GetStringAsync($"{urlItens}/Pedido/{idPedido}");
            var itens = JsonConvert.DeserializeObject<List<PedidoItem>>(json);

            listaItens.Clear();

            foreach (var item in itens)
            {
                listaItens.Add(new PedidoItemDetalhado()
                {
                    Id = item.Id,
                    PedidoId = item.PedidoId,
                    ProdutoId = item.ProdutoId,
                    ProdutoNome = listaProdutos.FirstOrDefault(l => l.Id == item.ProdutoId).Nome,
                    Valor = item.Valor,
                    Quantidade = item.Quantidade,
                    Desconto = item.Desconto,
                    Total = item.Total
                });
            }

            CollectionItens.ItemsSource = null;

            CollectionItens.ItemsSource = listaItens;

            RefreshItens.IsRefreshing = false;
        }

        private async Task CarregaListaProdutos()
        {
            var json = await _client.GetStringAsync(urlProdutos);
            listaProdutos = JsonConvert.DeserializeObject<List<Produto>>(json);
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
                pedidoAtual = selecionado;

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

                await CarregaListaItens(selecionado.Id);

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

                BtnEditar.IsEnabled = permissoes.Editar ? true : false;
                BtnNovo.IsEnabled = permissoes.Novo ? true : false;
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
                BtnNovo.IsEnabled = permissoes.Novo ? true : false;
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
                BtnNovo.IsEnabled = permissoes.Novo ? true : false;
                BtnSalvar.IsEnabled = true;
            }
        }

        private async void SwDeletar_Invoked(object sender, EventArgs e)
        {
            if (!permissoes.Excluir)
            {
                new ToastBase(Titulo, "Acesso negado", $"Operador não tem permissão para excluir pedidos!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

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

                await DeletarItensPedido(selecionado.Id);

                listaPedidos.Remove(listaPedidos.FirstOrDefault(l => l.Id == selecionado.Id));

                CvListagem.ItemsSource = null;

                CvListagem.ItemsSource = listaPedidos;

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

        private async Task DeletarItensPedido(int idPedido)
        {
            var itensDelete = await GetItensParaDelete(idPedido);

            foreach(var item in itensDelete)
            {
                await _client.DeleteAsync($"{urlItens}/{item.Id}");
            }
        }

        private void PkrData_DateSelected(object sender, DateChangedEventArgs e)
        {

        }

        private async void BtnNovo_Clicked(object sender, EventArgs e)
        {
            if (!permissoes.Novo)
            {
                new ToastBase(Titulo, "Acesso negado", $"Operador não tem permissão para adicionar novos pedidos!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

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
            if (!permissoes.Editar)
            {
                new ToastBase(Titulo, "Acesso negado", $"Operador não tem permissão para editar pedidos!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            if (TxtCodigo.Text == "" || TxtCodigo.Text == null)
            { 
                new ToastBase(Titulo, "Necessário selecionar um registro", $"Necessário selecionar um registro para estar efetuando a edição!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

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

                pedidoAtual = listaPedidos.FirstOrDefault(l => l.Id == novoRegistro.Id);

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

                pedidoAtual = listaPedidos.FirstOrDefault(l => l.Id == pedido.Id);

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

        private async void SwDeleteProduto_Invoked(object sender, EventArgs e)
        {
            if (!permissoes.Excluir)
            {
                new ToastBase(Titulo, "Acesso negado", $"Operador não tem permissão para excluir itens!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            var selecionado = (sender as SwipeItem)?.BindingContext as PedidoItemDetalhado;

            if (selecionado == null)
            {
                await DisplayAlert(Titulo, "Nenhum item selecionado", "Ok");

                return;
            }

            var confirmacao = await DisplayAlert(Titulo, $"Deseja realmente fazer a exclusão do item:\n" +
                $"Código: {selecionado.ProdutoId}\n" +
                $"Nome: {selecionado.ProdutoNome}?", "Confirmar", "Cancelar");

            if (!confirmacao)
            {
                return;
            }

            var response = await _client.DeleteAsync($"{urlItens}/{selecionado.Id}");

            if (!response.IsSuccessStatusCode)
            {
                new ToastBase(Titulo, "Ocorreu um erro ao deletar", $"Ocorreu um erro ao tentar deletar o item, tente novamente!\n" +
                    $"Código: {TxtCodigo.Text}\n" +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            listaItens.Remove(selecionado);

            await RecalcularPedido();

            CollectionItens.SelectedItem = null;

            CollectionItens.ItemsSource = new List<PedidoItemDetalhado>(listaItens);
            
            new ToastBase(Titulo, "Item deletado com sucesso", $"Item: {selecionado.ProdutoId} - {selecionado.ProdutoNome} deletado com sucesso!" +
                $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
        }

        private async void BtnFecharProdutos_Clicked(object sender, EventArgs e)
        {
            await AbrirProdutos(false);
        }

        private void BtnEditProduto_Clicked(object sender, EventArgs e)
        {
            if (!permissoes.Editar)
            {
                new ToastBase(Titulo, "Acesso negado", $"Operador não tem permissão para editar os itens!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            if(CollectionItens.SelectedItem == null)
            {
                new ToastBase(Titulo, "Necessário selecionar um item", $"Nenhum item selecionado para estar realizando a edição!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            var item = CollectionItens.SelectedItem as PedidoItemDetalhado;

            Navigation.PushAsync(new AddProdutos(this, item, pedidoAtual, listaProdutos));
        }

        private async void BtnAddProduto_Clicked(object sender, EventArgs e)
        {
            if (!permissoes.Novo)
            {
                new ToastBase(Titulo, "Acesso negado", $"Operador não tem permissão para adicionar novos itens!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            if(TxtCodigo.Text == null || TxtCodigo.Text == "" || TxtCodigo.Text == (0).ToString())
            {
                return;
            }

            await Navigation.PushAsync(new Pesquisar(this, Pesquisar.TiposPesquisas.Produtos));
        }

        public PedidoDetalhado GetPedido()
        {
            return listaPedidos.FirstOrDefault(l => l.Id == Convert.ToInt32(TxtCodigo.Text));
        }

        public async void AddListaItens(PedidoItemDetalhado item)
        {
            var itemExiste = listaItens.FirstOrDefault(l => l.Id == item.Id);

            if(itemExiste != null)
            {
                var index = listaItens.IndexOf(itemExiste);
                listaItens.Remove(itemExiste);
                
                listaItens.Insert(index, item);
            }
            else
            {
                listaItens.Add(item);
            }

            await RecalcularPedido();

            CollectionItens.ItemsSource = null;

            CollectionItens.ItemsSource = listaItens;
        }

        private async Task RecalcularPedido()
        {
            var valor = listaItens.Sum(l => l.Total);

            TxtValor.Text = valor.ToString("C2");

            await RecalcularValores();

            await AtualizarPedido();
        }

        private async Task AtualizarPedido()
        {
            Pedido pedido = new Pedido();

            var valor = Convert.ToDecimal(await LimpaValores(TxtValor.Text));
            var desconto = Convert.ToDecimal(await LimpaValores(TxtDesconto.Text));
            var total = Convert.ToDecimal(await LimpaValores(TxtTotal.Text));

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

            if (await SalvarRegistro(pedido))
            {
                await validaStatus(Visualizar);
            }
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
            if(Status == Cadastro)
            {
                new ToastBase(Titulo, "Necessário finalizar o cadastro", $"Necessário finalizar o cadastro do pedido para estar" +
                    $"incluindo os itens!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }
            await AbrirProdutos(true);
        }

        private async void LimpaCampos()
        {
            RadioVenda.IsChecked = true;
            TxtCodigo.Text = (0).ToString();
            PkrData.Date = DateTime.Today;
            TxtCliente.Text = string.Empty;
            TxtPgto.Text = string.Empty;
            TxtValor.Text = (0.00).ToString("C2");
            TxtDesconto.Text = (0.00).ToString("F2") + "%";
            TxtTotal.Text = (0.00).ToString("C2");

            CollectionItens.ItemsSource = null;
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

        private async void RefreshItens_Refreshing(object sender, EventArgs e)
        {
            await CarregaListaItens(Convert.ToInt32(TxtCodigo.Text));
        }

        private async void BtnImprimir_Clicked(object sender, EventArgs e)
        {
            if(Status == Cadastro)
            {
                new ToastBase(Titulo, "Necessário finalizar o cadastro", $"Necessário finalizar o cadastro do pedido para estar" +
                    $" gerando a impressão do pedido!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            if(pedidoAtual == null)
            {
                new ToastBase(Titulo, "Pedido inválido", $"O pedido selecionado é inválido para estar fazendo a impressão" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            await GerarImpressaoPedido();
        }

        private async Task GerarImpressaoPedido()
        {
            PDFGenerator pdf = new PDFGenerator();

            var stream = await pdf.RelatorioDetalhesPedido(pedidoAtual, listaItens);

            await CrossXamarinFormsSaveOpenPDFPackage.Current.SaveAndView($"ImpressaoPedido_{pedidoAtual.Id}.pdf", "application/pdf",
                stream, PDFOpenContext.ChooseApp);
        }
    }
}