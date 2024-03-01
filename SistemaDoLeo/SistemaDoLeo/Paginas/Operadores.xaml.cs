using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SistemaDoLeo.DB;
using SistemaDoLeo.Modelos.Classes;
using SistemaDoLeo.Toast;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SistemaDoLeo.Paginas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Operadores : TabbedPage
    {
        private int Status;
        private int Visualizar = 0;
        private int Cadastro = 1;
        private int Editar = 2;

        List<DisplayTelas> listaTelasOperador = new List<DisplayTelas>();
        List<Tela> listaTelas = new List<Tela>();
        private List<Operador> listaBase = new List<Operador>();
        private ProximoRegistro proximoRegistro;

        OperadorTela permissoes;

        private readonly HttpClient _client;
        private string url = $"{Links.ip}/Operador";
        private string urlTelas = $"{Links.ip}/Telas";
        private string urlOperadorTelas = $"{Links.ip}/OperadorTelas";
        private string Titulo = "Operador";

        public Operadores(OperadorTela permissoes)
        {
            InitializeComponent();

            BindingContext = this;

            CurrentPage = Children[0];

            this.permissoes = permissoes;

            HttpClientHandler insecureHandler = PermissaoDeCertificado.GetInsecureHandler();
            _client = new HttpClient(insecureHandler);
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            await carregarOperadores();
            await carregarTelas();
        }

        private async Task carregarOperadores()
        {
            RefreshV.IsRefreshing = true;

            string json = await _client.GetStringAsync(url);
            List<Operador> myList = JsonConvert.DeserializeObject<List<Operador>>(json);
            listaBase = myList;
            CvListagem.ItemsSource = myList;

            RefreshV.IsRefreshing = false;
        }

        private async Task carregarTelas()
        {
            var json = await _client.GetStringAsync(urlTelas);

            listaTelas = JsonConvert.DeserializeObject<List<Tela>>(json);
        }

        private async void CvListagem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentPage = Children[1];

            var selecionado = (Operador)e.CurrentSelection.FirstOrDefault();

            if (selecionado != null)
            {
                TxtCodigo.Text = selecionado.Id.ToString();
                TxtNome.Text = selecionado.Nome;
                TxtSenha.Text = selecionado.Senha;
                ChkAdmin.IsChecked = selecionado.Admin;
                ChkInativo.IsChecked = selecionado.Inativo;
                ChkVisualizar.IsChecked = false;

                await GetOperadorTelas(selecionado.Id);

                await validaStatus(Visualizar);

                // limpa o selecionado para poder selecionar o mesmo novamente
                CvListagem.SelectedItem = null;
            }
        }

        private async Task GetOperadorTelas(int id)
        {
            var json = await _client.GetStringAsync($"{urlOperadorTelas}/{id}");
            var OperadorTelas = JsonConvert.DeserializeObject<List<OperadorTela>>(json);

            listaTelasOperador.Clear();

            foreach(var tela in OperadorTelas)
            {
                listaTelasOperador.Add(new DisplayTelas()
                {
                    Id = tela.Id,
                    OperadorId = tela.OperadorId,
                    TelaId = tela.TelaId,
                    Nome = listaTelas.FirstOrDefault(t => t.Id == tela.TelaId).Nome,
                    Ativo = tela.Ativo,
                    Novo = tela.Novo,
                    Editar = tela.Editar,
                    Excluir = tela.Excluir
                });   
            }

            var telasFaltando = listaTelas.Where(l => !listaTelasOperador.Any(x => x.TelaId == l.Id)).ToList();

            foreach (var tela in telasFaltando)
            {
                listaTelasOperador.Add(new DisplayTelas()
                {
                    Id = 0,
                    OperadorId = 0,
                    TelaId = tela.Id,
                    Nome = tela.Nome,
                    Ativo = false,
                    Novo = false,
                    Editar = false,
                    Excluir = false
                });
            }

            listaTelasOperador.OrderBy(i => i.Id);

            CollectionTelas.ItemsSource = null;

            CollectionTelas.ItemsSource = listaTelasOperador;
        }

        private void SrcBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            CvListagem.ItemsSource = listaBase.Where(l => l.Nome.ToLower().Contains(SrcBuscar.Text.ToLower())).ToList();
        }

        private async void SwDeletar_Invoked(object sender, EventArgs e)
        {
            if (!permissoes.Excluir)
            {
                new ToastBase(Titulo, "Acesso negado", $"Operador não tem permissão para excluir o registro!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            var selecionado = (sender as SwipeItem)?.BindingContext as Operador;

            if (selecionado == null)
            {
                await DisplayAlert(Titulo, "Nenhum item selecionado", "Ok");

                return;
            }

            var confirmacao = await DisplayAlert(Titulo, $"Deseja realmente fazer a exclusão do Registro {selecionado.Nome}?", "Confirmar", "Cancelar");

            if (confirmacao)
            {
                var response = await _client.DeleteAsync($"{url}/{selecionado.Id}");

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, $"Ocorreu um erro ao deletar o registro: {selecionado.Nome}", "Ok");

                    return;
                }

                // PEGA AS TELAS DO OPERADOR PARA ENTÃO DELETAR
                await GetOperadorTelas(selecionado.Id);

                foreach(var tela in listaTelasOperador)
                {
                    var reponseTelas = await _client.DeleteAsync($"{urlOperadorTelas}/{tela.Id}");

                    if (!reponseTelas.IsSuccessStatusCode)
                    {
                        await DisplayAlert(Titulo, $"Ocorreu um erro ao tentar deletar a tela: {tela.Nome} do Operador: {selecionado.Nome}, contate o Administrador!", "OK");
                    }
                }

                listaBase.Remove(selecionado);

                // LIMPA OS CAMPOS DO CADASTRO PARA NÃO DEIXAR EDITAR O ITEM EXCLUIDO
                LimpaCampos();
                LimpaCamposTelas();

                if (SrcBuscar.Text == null)
                {
                    CvListagem.ItemsSource = new List<Operador>(listaBase);
                }
                else
                {
                    CvListagem.ItemsSource = new List<Operador>(listaBase.Where(l => l.Nome.ToLower().Contains(SrcBuscar.Text.ToLower())).ToList());
                }

                new ToastBase(Titulo, "Registro Deletado", $"Registro: {selecionado.Id} - {selecionado.Nome} deletado com Sucesso" +
                    $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
            }
            else
            {
                return;
            }
        }

        private async Task<bool> validaCampos()
        {
            if (TxtCodigo.Text == "" || TxtCodigo.Text == null)
            {
                // COLOCAR UM TOAST DE NECESSARIO INFORMAR UM ID

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
            else if (TxtSenha.Text == "" || TxtSenha.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Senha\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtSenha.Focus();

                return false;
            }

            return true;
        }

        private async Task validaStatus(int status)
        {
            List<DisplayTelas> listaComValidacoes = new List<DisplayTelas>();

            if (status == Visualizar)
            {
                this.Status = Visualizar;

                TxtCodigo.IsEnabled = false;
                TxtNome.IsEnabled = false;
                TxtSenha.IsEnabled = false;
                ChkAdmin.IsEnabled = false;
                ChkVisualizar.IsEnabled = false;
                ChkInativo.IsEnabled = false;

                BtnNovo.Text = "Novo";

                BtnEditar.IsEnabled = true;
                BtnNovo.IsEnabled = true;
                BtnSalvar.IsEnabled = false;

                ChkVisualizar.IsChecked = false;

                foreach(var tela in listaTelasOperador)
                {
                    listaComValidacoes.Add(new DisplayTelas()
                    {
                        Id = tela.Id,
                        OperadorId = tela.OperadorId,
                        Nome = tela.Nome,
                        TelaId = tela.TelaId,
                        Editar = tela.Editar,
                        Excluir = tela.Excluir,
                        Novo = tela.Novo,
                        Ativo = tela.Ativo,
                        EnableAtivo = false,
                        EnableNovo = false,
                        EnableEditar = false,
                        EnableExcluir = false
                    });
                }
            }
            else if (status == Editar)
            {
                this.Status = Editar;

                TxtCodigo.IsEnabled = false;
                TxtNome.IsEnabled = true;
                TxtSenha.IsEnabled = true;
                ChkAdmin.IsEnabled = true;
                ChkVisualizar.IsEnabled = true;
                ChkInativo.IsEnabled = true;

                BtnEditar.IsEnabled = false;
                BtnNovo.IsEnabled = false;
                BtnSalvar.IsEnabled = true;

                foreach (var tela in listaTelasOperador)
                {
                    listaComValidacoes.Add(new DisplayTelas()
                    {
                        Id = tela.Id,
                        OperadorId = tela.OperadorId,
                        Nome = tela.Nome,
                        TelaId = tela.TelaId,
                        Editar = tela.Editar,
                        Excluir = tela.Excluir,
                        Novo = tela.Novo,
                        Ativo = tela.Ativo,
                        EnableAtivo = true,
                        EnableNovo = true,
                        EnableEditar = true,
                        EnableExcluir = true
                    });
                }
            }
            else
            {
                this.Status = Cadastro;

                TxtCodigo.IsEnabled = false;
                TxtNome.IsEnabled = true;
                TxtSenha.IsEnabled = true;
                ChkAdmin.IsEnabled = true;
                ChkVisualizar.IsEnabled = true;
                ChkInativo.IsEnabled = true;

                BtnNovo.Text = "Limpar";

                BtnEditar.IsEnabled = false;
                BtnNovo.IsEnabled = true;
                BtnSalvar.IsEnabled = true;

                foreach (var tela in listaTelasOperador)
                {
                    listaComValidacoes.Add(new DisplayTelas()
                    {
                        Id = tela.Id,
                        OperadorId = tela.OperadorId,
                        Nome = tela.Nome,
                        TelaId = tela.TelaId,
                        Editar = tela.Editar,
                        Excluir = tela.Excluir,
                        Novo = tela.Novo,
                        Ativo = tela.Ativo,
                        EnableAtivo = true,
                        EnableNovo = true,
                        EnableEditar = true,
                        EnableExcluir = true
                    });
                }

            }

            listaTelasOperador.Clear();

            listaTelasOperador = new List<DisplayTelas>(listaComValidacoes);

            CollectionTelas.ItemsSource = null;

            listaTelasOperador.OrderBy(l => l.Id);

            CollectionTelas.ItemsSource = listaTelasOperador;
        }

        private async void BtnSalvar_Clicked(object sender, EventArgs e)
        {
            if (await validaCampos() == false)
            {
                return;
            }

            Operador operador = new Operador();

            if (Status == Cadastro)
            {
                operador = new Operador
                {
                    Nome = TxtNome.Text,
                    Senha = TxtSenha.Text,
                    Admin = ChkAdmin.IsChecked,
                    Inativo = ChkInativo.IsChecked
                };

            }
            else if (Status == Editar)
            {
                operador = new Operador
                {
                    Id = Convert.ToInt32(TxtCodigo.Text),
                    Nome = TxtNome.Text,
                    Senha = TxtSenha.Text,
                    Admin = ChkAdmin.IsChecked,
                    Inativo = ChkInativo.IsChecked
                };
            }

            if (await SalvarRegistro(operador))
            {
                await SalvarRegistroTelas();

                await validaStatus(Visualizar);
            }
        }

        private async Task<bool> SalvarRegistro(Operador operador)
        {
            var json = JsonConvert.SerializeObject(operador);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            if (operador.Id == 0)
            {
                var response = await _client.PostAsync(url, conteudo);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, "Ocorreu um erro no Cadastro", "Ok");

                    return false;
                }

                var novoRegistro = JsonConvert.DeserializeObject<Operador>(await response.Content.ReadAsStringAsync());

                await AtualizaProximoRegistro(novoRegistro.Id);

                TxtCodigo.Text = novoRegistro.Id.ToString();
                listaBase.Add(novoRegistro);
                CvListagem.ItemsSource = new List<Operador>(listaBase);

                new ToastBase(Titulo, "Registro Salvo", $"Registro salvo com Sucesso!\n" +
                    $"Código: {novoRegistro.Id}\n" +
                    $"Nome: {novoRegistro.Nome}\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
            }
            else
            {
                var response = await _client.PutAsync($"{url}/{operador.Id}", conteudo);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, "Ocorreu um erro ao fazer a Alteração", "Ok");

                    return false;
                }

                listaBase.Remove(listaBase.FirstOrDefault(l => l.Id == operador.Id));
                listaBase.Add(operador);
                CvListagem.ItemsSource = new List<Operador>(listaBase).OrderBy(i => i.Id);

                new ToastBase(Titulo, "Registro Atualizado", $"Registro atualizado com Sucesso!\n" +
                    $"Código: {TxtCodigo.Text}\n" +
                    $"Nome: {TxtNome.Text}\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
            }

            return true;
        }

        private async Task SalvarRegistroTelas()
        {
            List<OperadorTela> listaDeTelas = new List<OperadorTela>();
            List<DisplayTelas> listaDeTelasRemover = new List<DisplayTelas>();

            foreach(var tela in listaTelasOperador)
            {
                if(tela.Id == 0)
                {
                    // CRIA UM NOVO OBJETO SEM O ID PARA NÃO DAR ERRO NO POST
                    var novaTela = new OperadorTela()
                    {
                        OperadorId = Convert.ToInt32(TxtCodigo.Text),
                        TelaId = tela.TelaId,
                        Ativo = tela.Ativo,
                        Excluir = tela.Excluir,
                        Editar = tela.Editar,
                        Novo = tela.Novo
                    };

                    var json = JsonConvert.SerializeObject(novaTela);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await _client.PostAsync(urlOperadorTelas, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        await DisplayAlert(Titulo, $"Ocorreu um erro ao tentar cadastrar a tela: {tela.Nome}", "OK");

                        return;
                    }

                    var telaAtualizada = JsonConvert.DeserializeObject<OperadorTela>(await response.Content.ReadAsStringAsync());

                    listaDeTelas.Add(telaAtualizada);
                    listaDeTelasRemover.Add(tela);
                }
                else
                {
                    var novaTela = new OperadorTela()
                    {
                        Id = tela.Id,
                        TelaId = tela.TelaId,
                        OperadorId = tela.OperadorId,
                        Ativo = tela.Ativo,
                        Editar = tela.Editar,
                        Excluir = tela.Excluir,
                        Novo = tela.Novo
                    };

                    var json = JsonConvert.SerializeObject(novaTela);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var registroAtualizado = await _client.PutAsync($"{urlOperadorTelas}/{novaTela.Id}", content);

                    if (!registroAtualizado.IsSuccessStatusCode)
                    {
                        await DisplayAlert(Titulo, $"Ocorreu um erro ao atualizar a Tela: {tela.Nome}", "Ok");

                        return;
                    }
                }
            }

            // REMOVE AS TELAS DA LISTA DE TELAS DO OPERADOR
            foreach(var tela in listaDeTelasRemover)
            {
                listaTelasOperador.Remove(tela);
            }

            // ADICIONA A LISTA ATUALIZADA
            foreach(var tela in listaDeTelas)
            {
                listaTelasOperador.Add(new DisplayTelas()
                {
                    Id = tela.Id,
                    OperadorId = tela.OperadorId,
                    TelaId = tela.TelaId,
                    Nome = listaTelas.FirstOrDefault(i => i.Id == tela.TelaId).Nome,
                    Ativo = tela.Ativo,
                    Editar = tela.Editar,
                    Excluir = tela.Excluir,
                    Novo = tela.Novo
                });
            }

            CollectionTelas.ItemsSource = null;

            listaTelasOperador.OrderBy(l => l.Id);

            CollectionTelas.ItemsSource = listaTelasOperador;
        }

        private async Task AtualizaProximoRegistro(int id)
        {
            // ATUALIZA O PROXIMO REGISTRO COM BASE NO RETORNO DO POST
            proximoRegistro.Operador = id;

            var json = JsonConvert.SerializeObject(proximoRegistro);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            await _client.PutAsync(Links.proximoRegistro, conteudo);
        }

        private async void BtnNovo_Clicked(object sender, EventArgs e)
        {
            if (!permissoes.Novo)
            {
                new ToastBase(Titulo, "Acesso negado", $"Operador não tem permissão para criar um novo registro!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            LimpaCampos();
            LimpaCamposTelas();
            await validaStatus(Cadastro);

            // PEGA PROXIMO REGISTRO
            var id = await PegaProximoRegistro();

            TxtCodigo.Text = id.ToString();
        }

        private async Task<int> PegaProximoRegistro()
        {
            var json = await _client.GetStringAsync(Links.proximoRegistro);

            proximoRegistro = JsonConvert.DeserializeObject<ProximoRegistro>(json);

            proximoRegistro.Operador += 1;

            return proximoRegistro.Operador;
        }

        private async void BtnEditar_Clicked(object sender, EventArgs e)
        {
            if (!permissoes.Editar)
            {
                new ToastBase(Titulo, "Acesso negado", $"Operador não tem permissão para editar o registro!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            if (TxtCodigo.Text == "" || TxtCodigo.Text == null)
            {
                await DisplayAlert(Titulo, "Necessário selecionar um registro", "Ok");

                return;
            }

            await validaStatus(Editar);
        }

        private async void RefreshV_Refreshing(object sender, EventArgs e)
        {
            await carregarOperadores();
        }

        private void ChkVisualizar_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            TxtSenha.IsPassword = !TxtSenha.IsPassword;
        }

        private void CvTelas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void LimpaCampos()
        {
            TxtCodigo.Text = string.Empty;
            TxtNome.Text = string.Empty;
            TxtSenha.Text = string.Empty;
            ChkAdmin.IsChecked = false;
            ChkVisualizar.IsChecked = false;
            ChkInativo.IsChecked = false;
        }

        private void LimpaCamposTelas()
        {
            listaTelasOperador.Clear();
            
            foreach(var tela in listaTelas)
            {
                listaTelasOperador.Add(new DisplayTelas()
                {
                    Id = 0,
                    OperadorId = 0,
                    TelaId = tela.Id,
                    Nome = tela.Nome,
                    Ativo = false,
                    Editar = false,
                    Excluir = false,
                    Novo = false,
                    EnableAtivo = true,
                    EnableEditar = true,
                    EnableExcluir = true,
                    EnableNovo = true
                });
            }

            listaTelasOperador.OrderBy(i => i.Id);

            CollectionTelas.ItemsSource = null;

            CollectionTelas.ItemsSource = listaTelasOperador;
        }
    }
}