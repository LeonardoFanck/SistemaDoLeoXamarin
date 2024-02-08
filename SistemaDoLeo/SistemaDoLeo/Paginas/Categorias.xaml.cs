using Newtonsoft.Json;
using SistemaDoLeo.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SistemaDoLeo.Paginas
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Categorias : TabbedPage
	{
        private int Status;
        private int Visualizar = 0;
        private int Cadastro = 1;
        private int Editar = 2;
        private List<Categoria> listaBase = new List<Categoria>();

        private readonly HttpClient _cliente;
        private const string url = "https://10.0.2.2:7097/api/categorias";

        public HttpClientHandler GetInsecureHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
            return handler;
        }

        public Categorias ()
		{
			InitializeComponent();

			BindingContext = this;

            CurrentPage = Children[0];

            HttpClientHandler insecureHandler = GetInsecureHandler();
            _cliente = new HttpClient(insecureHandler);

            CarregaLista();
        }

        private async Task CarregaLista()
		{
            RefreshV.IsRefreshing = true;

            string json = await _cliente.GetStringAsync(url);
            List<Categoria> myList = JsonConvert.DeserializeObject<List<Categoria>>(json);
            listaBase = myList;
            CvListagem.ItemsSource = myList;

            RefreshV.IsRefreshing = false;
        }

        private void CvListagem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			CurrentPage = Children[1];

			var selecionado = (Categoria)e.CurrentSelection.FirstOrDefault();

			if(selecionado != null)
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
            var selecionado = (sender as SwipeItem)?.BindingContext as Categoria;

            if(selecionado == null)
            {
                await DisplayAlert("Erro", "Nenhum item selecionado", "Ok");

                return;
            }

            var confirmacao = await DisplayAlert("Confirmação", $"Deseja realmente fazer a exclusão do Registro {selecionado.Nome}?", "Confirmar", "Cancelar");

            if (confirmacao)
            {
                var response = await _cliente.DeleteAsync($"{url}/{selecionado.Id}");

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert("AFF", "Deu erro", "Ok");

                    return;
                }

                listaBase.Remove(selecionado);
                CvListagem.ItemsSource = listaBase;

                await DisplayAlert("Teste", $"Testes Ok, irá ser deletado o item id: {selecionado.Id}", "Ok");
            }
            else
            {
                return;
            }
        }

        private void BtnNovo_Clicked(object sender, EventArgs e)
        {
            limpaCampos();
            validaStatus(Cadastro);

            // PEGA PROXIMO REGISTRO
        }

        private async void BtnEditar_Clicked(object sender, EventArgs e)
        {
            if(TxtCodigo.Text == "")
            {
                await DisplayAlert("Erro", "Necessário selecionar um item", "Ok");
                return;
            }

            validaStatus(Editar);
        }

        private async void BtnSalvar_Clicked(object sender, EventArgs e)
        {
            if(await validaCampos() == false)
            {
                await DisplayAlert("Erro", "Campo faltando", "Ok");

                return;
            }

            Categoria categoria = new Categoria();

            if(Status == Cadastro)
            {
                categoria = new Categoria
                {
                    Nome = TxtNome.Text,
                    Inativo = ChkInativo.IsChecked
                };

            }
            else if(Status == Editar)
            {
                categoria = new Categoria
                {
                    Id = Convert.ToInt32(TxtCodigo.Text),
                    Nome = TxtNome.Text,
                    Inativo = ChkInativo.IsChecked
                };
            }

            await SalvarRegistro(categoria);
        }

        private async Task SalvarRegistro(Categoria categoria)
        {
            var json = JsonConvert.SerializeObject(categoria);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");


            if(categoria.Id == 0)
            {
                var response = await _cliente.PostAsync(url, conteudo);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert("AFF", "Deu erro o cadastro", "Ok");
                }
            }
            else
            {
                var response = await _cliente.PutAsync($"{url}/{categoria.Id}", conteudo);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert("AFF", "Deu erro a alteração", "Ok");
                }
            }
        }

        private async Task<bool> validaCampos()
        {
            if(TxtCodigo.Text == "" || TxtCodigo.Text == null)
            {
                // COLOCAR MENSAGEM DE NECESSARIO INFORMAR UM ID

                return false;
            }
            if(TxtNome.Text == "" || TxtNome.Text == null)
            {
                TxtNome.Focus();

                return false;
            }

            return true;
        }

        private void validaStatus(int status)
        {
            if(status == Visualizar)
            {
                this.Status = Visualizar;

                TxtNome.IsEnabled = false;
                ChkInativo.IsEnabled = false;
            }
            else if(status == Editar)
            {
                this.Status = Editar;

                TxtCodigo.IsEnabled = true;
                TxtNome.IsEnabled = true;
                ChkInativo.IsEnabled = true;
            }
            else
            {
                this.Status = Cadastro;

                TxtCodigo.IsEnabled = true;
                TxtNome.IsEnabled = true;
                ChkInativo.IsEnabled = true;
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
            var listaFiltro = listaBase.Where(l => l.Nome.ToLower().Contains(SrcBuscar.Text.ToLower())).ToList();

            CvListagem.ItemsSource = listaFiltro;
        }

        private async void RefreshV_Refreshing(object sender, EventArgs e)
        {
            await CarregaLista();
        }
    }
}