using Newtonsoft.Json;
using SistemaDoLeo.DB;
using SistemaDoLeo.Modelos.Classes;
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
    public partial class FormasPgto : TabbedPage
    {
        private int Status;
        private int Visualizar = 0;
        private int Cadastro = 1;
        private int Editar = 2;
        private List<FormaPgto> listaBase = new List<FormaPgto>();
        private ProximoRegistro proximoRegistro;

        private readonly HttpClient _cliente;
        private string url = $"{Links.ip}/FormaPgto";
        private string Titulo = "Forma de Pagamento";

        public FormasPgto()
        {
            InitializeComponent();

            BindingContext = this;

            CurrentPage = Children[0];

            HttpClientHandler insecureHandler = PermissaoDeCertificado.GetInsecureHandler();
            _cliente = new HttpClient(insecureHandler);

            CarregaLista();
        }

        private async Task CarregaLista()
        {
            RefreshV.IsRefreshing = true;

            string json = await _cliente.GetStringAsync(url);
            List<FormaPgto> myList = JsonConvert.DeserializeObject<List<FormaPgto>>(json);
            listaBase = myList;
            CvListagem.ItemsSource = myList;

            RefreshV.IsRefreshing = false;
        }

        private void CvListagem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentPage = Children[1];

            var selecionado = (FormaPgto)e.CurrentSelection.FirstOrDefault();

            if (selecionado != null)
            {
                validaStatus(Visualizar);

                TxtCodigo.Text = selecionado.Id.ToString();
                TxtNome.Text = selecionado.Nome;
                ChkInativo.IsChecked = selecionado.Inativo;

                // limpa o selecionado para poder selecionar o mesmo novamente
                CvListagem.SelectedItem = null;
            }
        }

        private async void SwDeletar_Invoked(object sender, EventArgs e)
        {
            var selecionado = (sender as SwipeItem)?.BindingContext as FormaPgto;

            if (selecionado == null)
            {
                await DisplayAlert(Titulo, "Nenhum item selecionado", "Ok");

                return;
            }

            var confirmacao = await DisplayAlert(title: ToString(), $"Deseja realmente fazer a exclusão do Registro {selecionado.Nome}?", "Confirmar", "Cancelar");

            if (confirmacao)
            {
                var response = await _cliente.DeleteAsync($"{url}/{selecionado.Id}");

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, "Ocorreu um erro", "Ok");

                    return;
                }

                listaBase.Remove(selecionado);

                // LIMPA OS CAMPOS DO CADASTRO PARA NÃO DEIXAR EDITAR O ITEM EXCLUIDO
                limpaCampos();

                if (SrcBuscar.Text == null)
                {
                    CvListagem.ItemsSource = new List<FormaPgto>(listaBase);
                }
                else
                {
                    CvListagem.ItemsSource = new List<FormaPgto>(listaBase.Where(l => l.Nome.ToLower().Contains(SrcBuscar.Text.ToLower())).ToList());
                }

                await DisplayAlert(Titulo, $"Deletado o item id: {selecionado.Id} - Atualizar para um Toast", "Ok");
            }
            else
            {
                return;
            }
        }

        private async void BtnNovo_Clicked(object sender, EventArgs e)
        {
            limpaCampos();
            validaStatus(Cadastro);

            // PEGA PROXIMO REGISTRO
            var id = await PegaProximoRegistro();

            TxtCodigo.Text = id.ToString();
        }

        private async Task<int> PegaProximoRegistro()
        {
            var json = await _cliente.GetStringAsync(Links.proximoRegistro);

            proximoRegistro = JsonConvert.DeserializeObject<ProximoRegistro>(json);

            proximoRegistro.FormaPgto += 1;

            return proximoRegistro.FormaPgto;
        }

        private async void BtnEditar_Clicked(object sender, EventArgs e)
        {
            if (TxtCodigo.Text == "" || TxtCodigo.Text == null)
            {
                await DisplayAlert(Titulo, "Necessário selecionar um item", "Ok");

                return;
            }

            validaStatus(Editar);
        }

        private async void BtnSalvar_Clicked(object sender, EventArgs e)
        {
            if (await validaCampos() == false)
            {
                //await DisplayAlert(Titulo, "Campo faltando", "Ok");

                return;
            }

            FormaPgto formaPgto = new FormaPgto();

            if (Status == Cadastro)
            {
                formaPgto = new FormaPgto
                {
                    Nome = TxtNome.Text,
                    Inativo = ChkInativo.IsChecked
                };

            }
            else if (Status == Editar)
            {
                formaPgto = new FormaPgto
                {
                    Id = Convert.ToInt32(TxtCodigo.Text),
                    Nome = TxtNome.Text,
                    Inativo = ChkInativo.IsChecked
                };
            }

            if (await SalvarRegistro(formaPgto))
            {
                validaStatus(Visualizar);
            }
        }

        private async Task<bool> SalvarRegistro(FormaPgto formaPgto)
        {
            var json = JsonConvert.SerializeObject(formaPgto);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            if (formaPgto.Id == 0)
            {
                var response = await _cliente.PostAsync(url, conteudo);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, "Ocorreu um erro no Cadastro", "Ok");

                    return false;
                }

                var novaFormaPgto = JsonConvert.DeserializeObject<FormaPgto>(await response.Content.ReadAsStringAsync());

                await AtualizaProximoRegistro(novaFormaPgto.Id);

                TxtCodigo.Text = novaFormaPgto.Id.ToString();
                listaBase.Add(novaFormaPgto);
                CvListagem.ItemsSource = new List<FormaPgto>(listaBase);
            }
            else
            {
                var response = await _cliente.PutAsync($"{url}/{formaPgto.Id}", conteudo);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, "Ocorreu um erro ao fazer a Alteração", "Ok");

                    return false;
                }

                listaBase.Remove(listaBase.FirstOrDefault(l => l.Id == formaPgto.Id));
                listaBase.Add(formaPgto);
                CvListagem.ItemsSource = new List<FormaPgto>(listaBase).OrderBy(i => i.Id);
            }

            return true;
        }

        private async Task AtualizaProximoRegistro(int id)
        {
            // ATUALIZA O PROXIMO REGISTRO COM BASE NO RETORNO DO POST
            proximoRegistro.FormaPgto = id;

            var json = JsonConvert.SerializeObject(proximoRegistro);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            await _cliente.PutAsync(Links.proximoRegistro, conteudo);
        }

        private async Task<bool> validaCampos()
        {
            if (TxtCodigo.Text == "" || TxtCodigo.Text == null)
            {
                // COLOCAR UM TOAST DE NECESSARIO INFORMAR UM ID

                return false;
            }
            if (TxtNome.Text == "" || TxtNome.Text == null)
            {
                TxtNome.Focus();

                return false;
            }

            return true;
        }

        private void validaStatus(int status)
        {
            if (status == Visualizar)
            {
                this.Status = Visualizar;

                TxtCodigo.IsEnabled = false;
                TxtNome.IsEnabled = false;
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
                ChkInativo.IsEnabled = true;

                BtnNovo.Text = "Limpar";

                BtnEditar.IsEnabled = false;
                BtnNovo.IsEnabled = true;
                BtnSalvar.IsEnabled = true;
            }
        }

        private void limpaCampos()
        {
            TxtCodigo.Text = string.Empty;
            TxtNome.Text = string.Empty;
            ChkInativo.IsChecked = false;
        }

        private void SrcBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            CvListagem.ItemsSource = listaBase.Where(l => l.Nome.ToLower().Contains(SrcBuscar.Text.ToLower())).ToList();
        }

        private async void RefreshV_Refreshing(object sender, EventArgs e)
        {
            await CarregaLista();
        }
    }
}