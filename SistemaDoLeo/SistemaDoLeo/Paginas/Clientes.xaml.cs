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

namespace SistemaDoLeo.Paginas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Clientes : TabbedPage
    {
        private int Status;
        private int Visualizar = 0;
        private int Cadastro = 1;
        private int Editar = 2;

        private string Titulo = "Cliente/Fornecedor";

        private List<Cliente> listaClientes = new List<Cliente>();
        private ProximoRegistro proximoRegistro;

        private readonly HttpClient _client;
        private string url = $"{Links.ip}/Cliente";

        private bool carregando;

        public Clientes()
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

            await CarregaListaProdutos();
            await CarregaUFs();
        }

        private async Task CarregaListaProdutos()
        {
            RefreshV.IsRefreshing = true;

            var json = await _client.GetStringAsync(url);
            listaClientes = JsonConvert.DeserializeObject<List<Cliente>>(json);
            CvListagem.ItemsSource = listaClientes;

            RefreshV.IsRefreshing = false;
        }


        private async Task CarregaUFs()
        {
            List<string> ufs = new List<string>();

            ufs.Add("AC");
            ufs.Add("AL");
            ufs.Add("AP");
            ufs.Add("AM");
            ufs.Add("BA");
            ufs.Add("CE");
            ufs.Add("DF");
            ufs.Add("ES");
            ufs.Add("GO");
            ufs.Add("MA");
            ufs.Add("MT");
            ufs.Add("MS");
            ufs.Add("MG");
            ufs.Add("PA");
            ufs.Add("PB");
            ufs.Add("PR");
            ufs.Add("PE");
            ufs.Add("PI");
            ufs.Add("RJ");
            ufs.Add("RN");
            ufs.Add("RS");
            ufs.Add("RO");
            ufs.Add("RR");
            ufs.Add("SC");
            ufs.Add("SP");
            ufs.Add("SE");
            ufs.Add("TO");

            PkrUf.ItemsSource = ufs;
        }

        private void SrcBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            CvListagem.ItemsSource = listaClientes.Where(p => p.Nome.ToLower().Contains(SrcBuscar.Text.ToLower())).ToList();
        }

        private async void RefreshV_Refreshing(object sender, EventArgs e)
        {
            await CarregaListaProdutos();
        }

        private async void CvListagem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentPage = Children[1];

            var selecionado = (Cliente)e.CurrentSelection.FirstOrDefault();

            if (selecionado != null)
            {
                TxtCodigo.Text = selecionado.Id.ToString();
                ChkInativo.IsChecked = selecionado.Inativo;
                ChkCliente.IsChecked = selecionado.TipoCliente;
                ChkFornecedor.IsChecked = selecionado.TipoForncedor;
                TxtNome.Text = selecionado.Nome;
                PkrData.Date = selecionado.DtNasc;
                TxtDocumento.Text = selecionado.Documento;
                TxtCep.Text = selecionado.Cep;
                PkrUf.SelectedItem = selecionado.Uf;
                TxtCidade.Text = selecionado.Cidade;
                TxtBairro.Text = selecionado.Bairro;
                TxtEndereco.Text = selecionado.Endereco;
                TxtNumero.Text = selecionado.Numero;
                TxtComplemento.Text = selecionado.Complemento;

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
                ChkInativo.IsEnabled = false;
                ChkCliente.IsEnabled = false;
                ChkFornecedor.IsEnabled = false;
                TxtNome.IsEnabled = false;
                PkrData.IsEnabled = false;
                TxtDocumento.IsEnabled = false;
                TxtCep.IsEnabled = false;
                PkrUf.IsEnabled = false;
                TxtCidade.IsEnabled = false;
                TxtBairro.IsEnabled = false;
                TxtEndereco.IsEnabled = false;
                TxtNumero.IsEnabled = false;
                TxtComplemento.IsEnabled = false;

                BtnNovo.Text = "Novo";

                BtnEditar.IsEnabled = true;
                BtnNovo.IsEnabled = true;
                BtnSalvar.IsEnabled = false;
            }
            else if (status == Editar)
            {
                this.Status = Editar;

                TxtCodigo.IsEnabled = false;
                ChkInativo.IsEnabled = true;
                ChkCliente.IsEnabled = true;
                ChkFornecedor.IsEnabled = true;
                TxtNome.IsEnabled = true;
                PkrData.IsEnabled = true;
                TxtDocumento.IsEnabled = true;
                TxtCep.IsEnabled = true;
                PkrUf.IsEnabled = true;
                TxtCidade.IsEnabled = true;
                TxtBairro.IsEnabled = true;
                TxtEndereco.IsEnabled = true;
                TxtNumero.IsEnabled = true;
                TxtComplemento.IsEnabled = true;

                BtnEditar.IsEnabled = false;
                BtnNovo.IsEnabled = false;
                BtnSalvar.IsEnabled = true;

            }
            else
            {
                this.Status = Cadastro;

                TxtCodigo.IsEnabled = false;
                ChkInativo.IsEnabled = true;
                ChkCliente.IsEnabled = true;
                ChkFornecedor.IsEnabled = true;
                TxtNome.IsEnabled = true;
                PkrData.IsEnabled = true;
                TxtDocumento.IsEnabled = true;
                TxtCep.IsEnabled = true;
                PkrUf.IsEnabled = true;
                TxtCidade.IsEnabled = true;
                TxtBairro.IsEnabled = true;
                TxtEndereco.IsEnabled = true;
                TxtNumero.IsEnabled = true;
                TxtComplemento.IsEnabled = true;

                BtnNovo.Text = "Limpar";

                BtnEditar.IsEnabled = false;
                BtnNovo.IsEnabled = true;
                BtnSalvar.IsEnabled = true;
            }
        }

        private async void SwDeletar_Invoked(object sender, EventArgs e)
        {
            var selecionado = (sender as SwipeItem)?.BindingContext as Cliente;

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

                listaClientes.Remove(selecionado);

                // LIMPA OS CAMPOS DO CADASTRO PARA NÃO DEIXAR EDITAR O ITEM EXCLUIDO
                LimpaCampos();

                if (SrcBuscar.Text == null)
                {
                    CvListagem.ItemsSource = new List<Cliente>(listaClientes);
                }
                else
                {
                    CvListagem.ItemsSource = new List<Cliente>(listaClientes.Where(l => l.Nome.ToLower().Contains(SrcBuscar.Text.ToLower())).ToList());
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

            Cliente cliente = new Cliente();

            if (Status == Cadastro)
            {
                cliente = new Cliente
                {
                    Inativo = ChkInativo.IsChecked,
                    TipoCliente = ChkCliente.IsChecked,
                    TipoForncedor = ChkFornecedor.IsChecked,
                    Nome = TxtNome.Text,
                    DtNasc = PkrData.Date,
                    Documento = TxtDocumento.Text,
                    Cep = TxtCep.Text,
                    Uf = PkrUf.SelectedItem.ToString(),
                    Cidade = TxtCidade.Text,
                    Bairro = TxtBairro.Text,
                    Endereco = TxtEndereco.Text,
                    Numero = TxtNumero.Text,
                    Complemento = TxtComplemento.Text
                };

            }
            else if (Status == Editar)
            {
                cliente = new Cliente
                {
                    Id = Convert.ToInt32(TxtCodigo.Text),
                    Inativo = ChkInativo.IsChecked,
                    TipoCliente = ChkCliente.IsChecked,
                    TipoForncedor = ChkFornecedor.IsChecked,
                    Nome = TxtNome.Text,
                    DtNasc = PkrData.Date,
                    Documento = TxtDocumento.Text,
                    Cep = TxtCep.Text,
                    Uf = PkrUf.SelectedItem.ToString(),
                    Cidade = TxtCidade.Text,
                    Bairro = TxtBairro.Text,
                    Endereco = TxtEndereco.Text,
                    Numero = TxtNumero.Text,
                    Complemento = TxtComplemento.Text
                };
            }

            if (await SalvarRegistro(cliente))
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
            else if (!ChkCliente.IsChecked && !ChkFornecedor.IsChecked)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar um tipo para o Registro\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                ChkCliente.Color = Color.Red;
                ChkFornecedor.Color = Color.Red;

                await Task.Delay(500);

                ChkCliente.Color = Color.Black;
                ChkFornecedor.Color = Color.Black;

                return false;
            }
            else if (TxtNome.Text == "" || TxtNome.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Nome\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtNome.Focus();

                return false;
            }
            else if (PkrData.Date >= DateTime.Today)
            {
                new ToastBase(Titulo, "Campo inválido", $"Necessário informar uma data menor que o dia atual\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                PkrData.Focus();

                return false;
            }
            else if (TxtDocumento.Text == "" || TxtDocumento.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Documento\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtDocumento.Focus();

                return false;
            }
            else if (TxtCep.Text == "" || TxtCep.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo CEP\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtCep.Focus();

                return false;
            }
            else if (PkrUf.SelectedItem.ToString() == "" || PkrUf.SelectedItem == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar uma UF\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                PkrUf.Focus();

                return false;
            }
            else if (TxtCidade.Text == "" || TxtCidade.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Cidade\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtCidade.Focus();

                return false;
            }
            else if (TxtBairro.Text == "" || TxtBairro.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Bairro\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtBairro.Focus();

                return false;
            }
            else if (TxtEndereco.Text == "" || TxtEndereco.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Endereço\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtEndereco.Focus();

                return false;
            }
            else if (TxtNumero.Text == "" || TxtNumero.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Número\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtNumero.Focus();

                return false;
            }
            else if (TxtComplemento.Text == null)
            {
                TxtComplemento.Text = "";

                return false;
            }

            return true;
        }

        private async Task<bool> SalvarRegistro(Cliente cliente)
        {
            var json = JsonConvert.SerializeObject(cliente);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            if (cliente.Id == 0)
            {
                var response = await _client.PostAsync(url, conteudo);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, "Ocorreu um erro no Cadastro", "Ok");

                    return false;
                }

                var novoRegistro = JsonConvert.DeserializeObject<Cliente>(await response.Content.ReadAsStringAsync());

                await AtualizaProximoRegistro(novoRegistro.Id);

                TxtCodigo.Text = novoRegistro.Id.ToString();
                listaClientes.Add(novoRegistro);
                CvListagem.ItemsSource = new List<Cliente>(listaClientes);

                new ToastBase(Titulo, "Registro Salvo", $"Registro salvo com Sucesso!\n" +
                    $"Código: {novoRegistro.Id}\n" +
                    $"Nome: {novoRegistro.Nome}\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
            }
            else
            {
                var response = await _client.PutAsync($"{url}/{cliente.Id}", conteudo);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, "Ocorreu um erro ao fazer a Alteração", "Ok");

                    return false;
                }

                listaClientes.Remove(listaClientes.FirstOrDefault(l => l.Id == cliente.Id));
                listaClientes.Add(cliente);
                CvListagem.ItemsSource = new List<Cliente>(listaClientes).OrderBy(i => i.Id);

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
            proximoRegistro.Cliente = id;

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

            TxtNome.Focus();
        }

        private async Task<int> PegaProximoRegistro()
        {
            var json = await _client.GetStringAsync(Links.proximoRegistro);

            proximoRegistro = JsonConvert.DeserializeObject<ProximoRegistro>(json);

            return proximoRegistro.Cliente += 1;
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

        private void PkrData_DateSelected(object sender, DateChangedEventArgs e)
        {
            TxtDocumento.Focus();
        }

        private void TxtDocumento_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(TxtDocumento.Text.Length > 13)
            {
                MascaraDocumento.Mask = "XX.XXX.XXX/XXXX-XX";
            }
            else
            {
                MascaraDocumento.Mask = "XXX.XXX.XXX-XX";
            }

            if(TxtDocumento.Text.Length != 14 && TxtDocumento.Text.Length != 18)
            {
                TxtDocumento.TextColor = Color.Red;
            }
            else
            {
                TxtDocumento.TextColor = Color.Black;
            }
        }

        private async void TxtCep_Completed(object sender, EventArgs e)
        {
            telaCarregamento();

            // https://viacep.com.br/ws/95800000/json
            Regex regex = new Regex("[^a-zA-Z0-9 ]");
            var cep = regex.Replace(TxtCep.Text, "");

            if (cep.Length != 8)
            {
                new ToastBase(Titulo, "CEP Inválido", $"Cep: {TxtCep.Text} inválido, tamanho incorreto." +
                    $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            var json = await _client.GetStringAsync($"https://viacep.com.br/ws/{cep}/json");

            if(json is null)
            {
                new ToastBase(Titulo, "CEP não localizado", $"Cep: {TxtCep.Text} inválido, não foi localizado." +
                    $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            var endereco = JsonConvert.DeserializeObject<Endereco>(json);

            if(endereco is null || endereco.Erro != "" && endereco.Erro != null)
            {
                new ToastBase(Titulo, "CEP não localizado", $"Cep: {TxtCep.Text} inválido, não foi localizado." +
                    $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            PkrUf.SelectedItem = endereco.Uf;
            TxtCidade.Text = endereco.Localidade;
            TxtBairro.Text = endereco.Bairro;
            TxtEndereco.Text = endereco.Logradouro;
            TxtComplemento.Text = endereco.Complemento;

            telaCarregamento();
        }

        private void telaCarregamento()
        {
            if (carregando)
            {
                bvTelaPreta.IsVisible = false;
                actRoda.IsVisible = false;
                actRoda.IsRunning = false;

                carregando = false;
            }
            else
            {
                bvTelaPreta.IsVisible = true;
                actRoda.IsVisible = true;
                actRoda.IsRunning = true;

                carregando = true;
            }
        }

        private void LimpaCampos()
        {
            TxtCodigo.Text = string.Empty;
            ChkInativo.IsChecked = false;
            ChkCliente.IsChecked = false;
            ChkFornecedor.IsChecked = false;
            TxtNome.Text = string.Empty;
            PkrData.Date = DateTime.Today;
            TxtDocumento.Text = string.Empty;
            TxtCep.Text = string.Empty;
            PkrUf.SelectedIndex = -1;
            TxtCidade.Text = string.Empty;
            TxtBairro.Text = string.Empty;
            TxtEndereco.Text = string.Empty;
            TxtNumero.Text = string.Empty;
            TxtComplemento.Text = string.Empty;
        }

        private void TxtNumero_Unfocused(object sender, FocusEventArgs e)
        {
            if(TxtNumero.Text == "")
            {
                TxtNumero.Text = "S/N";
            }
        }
    }
}