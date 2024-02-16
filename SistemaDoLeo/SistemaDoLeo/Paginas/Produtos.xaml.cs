using Newtonsoft.Json;
using SistemaDoLeo.DB;
using SistemaDoLeo.Modelos.Classes;
using SistemaDoLeo.Toast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
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

        private string Titulo = "Produto";

        private List<Categoria> listaCategorias = new List<Categoria>();
        private List<Produto> listaProdutos = new List<Produto>();
        private ProximoRegistro proximoRegistro;

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
            CvListagem.ItemsSource = listaProdutos.Where(p => p.Nome.ToLower().Contains(SrcBuscar.Text.ToLower())).ToList();
        }

        private async void CvListagem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentPage = Children[1];

            var selecionado = (Produto)e.CurrentSelection.FirstOrDefault();

            if (selecionado != null)
            {
                var categoriaIndex = listaCategorias.IndexOf(listaCategorias.FirstOrDefault(l => l.Id == selecionado.CategoriaId));
                
                TxtCodigo.Text = selecionado.Id.ToString();
                TxtNome.Text = selecionado.Nome;
                PkrCategoria.SelectedIndex = categoriaIndex;
                TxtPreco.Text = selecionado.Preco.ToString();
                TxtCusto.Text = selecionado.Custo.ToString();
                TxtUnidade.Text = selecionado.Unidade;
                ChkInativo.IsChecked = selecionado.Inativo;

                await validaStatus(Visualizar);

                // limpa o selecionado para poder selecionar o mesmo novamente
                CvListagem.SelectedItem = null;
            }
        }

        private async Task validaStatus(int status)
        {
            List<DisplayTelas> listaComValidacoes = new List<DisplayTelas>();

            if (status == Visualizar)
            {
                this.Status = Visualizar;

                TxtCodigo.IsEnabled = false;
                TxtNome.IsEnabled = false;
                PkrCategoria.IsEnabled = false;
                TxtPreco.IsEnabled = false;
                TxtCusto.IsEnabled = false;
                TxtUnidade.IsEnabled = false;
                ChkInativo.IsEnabled = false;

                BtnNovo.Text = "Novo";

                BtnEditar.IsEnabled = true;
                BtnNovo.IsEnabled = true;
                BtnSalvar.IsEnabled = false;
            }
            else if (status == Editar)
            {
                this.Status = Editar;

                TxtCodigo.IsEnabled = false;
                TxtNome.IsEnabled = true;
                PkrCategoria.IsEnabled = true;
                TxtPreco.IsEnabled = true;
                TxtCusto.IsEnabled = true;
                TxtUnidade.IsEnabled = true;
                ChkInativo.IsEnabled = true;

                BtnEditar.IsEnabled = false;
                BtnNovo.IsEnabled = false;
                BtnSalvar.IsEnabled = true;

            }
            else
            {
                this.Status = Cadastro;

                TxtCodigo.IsEnabled = false;
                TxtNome.IsEnabled = true;
                PkrCategoria.IsEnabled = true;
                TxtPreco.IsEnabled = true;
                TxtCusto.IsEnabled = true;
                TxtUnidade.IsEnabled = true;
                ChkInativo.IsEnabled = true;

                BtnNovo.Text = "Limpar";

                BtnEditar.IsEnabled = false;
                BtnNovo.IsEnabled = true;
                BtnSalvar.IsEnabled = true;
            }
        }

        private async void SwDeletar_Invoked(object sender, EventArgs e)
        {
            var selecionado = (sender as SwipeItem)?.BindingContext as Produto;

            if (selecionado == null)
            {
                await DisplayAlert(Titulo, "Nenhum item selecionado", "Ok");

                return;
            }

            var confirmacao = await DisplayAlert(Titulo, $"Deseja realmente fazer a exclusão do Registro: {selecionado.Nome}?", "Confirmar", "Cancelar");

            if (confirmacao)
            {
                var response = await _client.DeleteAsync($"{url}/{selecionado.Id}");

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, $"Ocorreu um erro ao deletar o registro: {selecionado.Nome}", "Ok");

                    return;
                }

                listaProdutos.Remove(selecionado);

                // LIMPA OS CAMPOS DO CADASTRO PARA NÃO DEIXAR EDITAR O ITEM EXCLUIDO
                LimpaCampos();

                if (SrcBuscar.Text == null)
                {
                    CvListagem.ItemsSource = new List<Produto>(listaProdutos);
                }
                else
                {
                    CvListagem.ItemsSource = new List<Produto>(listaProdutos.Where(l => l.Nome.ToLower().Contains(SrcBuscar.Text.ToLower())).ToList());
                }

                new ToastBase(Titulo, "Registro Deletado", $"Registro: {selecionado.Id} - {selecionado.Nome} deletado com Sucesso" +
                    $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
            }
            else
            {
                return;
            }
        }

        private async void BtnSalvar_Clicked(object sender, EventArgs e)
        {
            if (await validaCampos() == false)
            {
                return;
            }

            Produto produto = new Produto();

            var categoria = PkrCategoria.SelectedItem as Categoria;

            if (Status == Cadastro)
            {
                produto = new Produto
                {
                    Nome = TxtNome.Text,
                    CategoriaId = categoria.Id,
                    Preco = Convert.ToDecimal(TxtPreco.Text),
                    Custo = Convert.ToDecimal(TxtCusto.Text),
                    Unidade = TxtUnidade.Text,
                    Estoque = 0,
                    Inativo = ChkInativo.IsChecked
                };

            }
            else if (Status == Editar)
            {
                produto = new Produto
                {
                    Id = Convert.ToInt32(TxtCodigo.Text),
                    Nome = TxtNome.Text,
                    CategoriaId = categoria.Id,
                    Preco = Convert.ToDecimal(TxtPreco.Text),
                    Custo = Convert.ToDecimal(TxtCusto.Text),
                    Unidade = TxtUnidade.Text,
                    Estoque = Convert.ToInt32(TxtEstoque.Text),
                    Inativo = ChkInativo.IsChecked
                };
            }

            if (await SalvarRegistro(produto))
            {
                await validaStatus(Visualizar);
            }
        }

        private async Task<bool> validaCampos()
        {
            if (TxtCodigo.Text == "" || TxtCodigo.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Código\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return false;
            }
            else if (TxtNome.Text == "" || TxtNome.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Nome\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtNome.Focus();

                return false;
            }
            else if (PkrCategoria.SelectedIndex == -1 || PkrCategoria.SelectedItem == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Categoria\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                PkrCategoria.Focus();

                return false;
            }
            else if (TxtPreco.Text == "" || TxtPreco.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Preço\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtPreco.Focus();

                return false;
            }
            else if (TxtCusto.Text == "" || TxtCusto.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Custo\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtCusto.Focus();

                return false;
            }
            else if (TxtUnidade.Text == "" || TxtUnidade.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Unidade\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtUnidade.Focus();

                return false;
            }

            return true;
        }

        private async Task<bool> SalvarRegistro(Produto produto)
        {
            var json = JsonConvert.SerializeObject(produto);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            if (produto.Id == 0)
            {
                var response = await _client.PostAsync(url, conteudo);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, "Ocorreu um erro no Cadastro", "Ok");

                    return false;
                }

                var novoRegistro = JsonConvert.DeserializeObject<Produto>(await response.Content.ReadAsStringAsync());

                await AtualizaProximoRegistro(novoRegistro.Id);

                TxtCodigo.Text = novoRegistro.Id.ToString();
                listaProdutos.Add(novoRegistro);
                CvListagem.ItemsSource = new List<Produto>(listaProdutos);

                new ToastBase(Titulo, "Registro Salvo", $"Registro salvo com Sucesso!\n" +
                    $"Código: {novoRegistro.Id}\n" +
                    $"Nome: {novoRegistro.Nome}\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
            }
            else
            {
                var response = await _client.PutAsync($"{url}/{produto.Id}", conteudo);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, "Ocorreu um erro ao fazer a Alteração", "Ok");

                    return false;
                }

                listaProdutos.Remove(listaProdutos.FirstOrDefault(l => l.Id == produto.Id));
                listaProdutos.Add(produto);
                CvListagem.ItemsSource = new List<Produto>(listaProdutos).OrderBy(i => i.Id);

                new ToastBase(Titulo, "Registro Atualizado", $"Registro atualizado com Sucesso!\n" +
                    $"Código: {TxtCodigo.Text}\n" +
                    $"Nome: {TxtNome.Text}\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
            }

            return true;
        }

        private async Task AtualizaProximoRegistro(int id)
        {
            // ATUALIZA O PROXIMO REGISTRO COM BASE NO RETORNO DO POST
            proximoRegistro.Produto = id;

            var json = JsonConvert.SerializeObject(proximoRegistro);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            await _client.PutAsync(Links.proximoRegistro, conteudo);
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

            proximoRegistro.Produto += 1;

            return proximoRegistro.Produto;
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

        private async void RefreshV_Refreshing(object sender, EventArgs e)
        {
            await CarregaListaProdutos();
        }

        private void LimpaCampos()
        {
            TxtCodigo.Text = string.Empty;
            TxtNome.Text = string.Empty;
            PkrCategoria.SelectedIndex = -1;
            TxtPreco.Text = string.Empty;
            TxtCusto.Text = string.Empty;
            TxtUnidade.Text = string.Empty;
            ChkInativo.IsChecked = false;
        }

        private void PkrCategoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(Status != Visualizar)
            {
                TxtPreco.Focus();
            }
        }
    }
}