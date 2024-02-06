using SistemaDoLeo.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SistemaDoLeo.Paginas
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Categorias : TabbedPage
	{
        private int Visualizar = 0;
        private int Cadastro = 1;
        private int Editar = 2;


		public Categorias ()
		{
			InitializeComponent();

			BindingContext = this;

			CurrentPage = Children[0];

			CarregaLista();
        }

		private async void CarregaLista()
		{
			var lista = new List<Categoria>();

			for(int i = 0; i < 10; i++) {
				lista.Add(new Categoria()
				{
                    Id = i,
                    Nome = "Teste " + i,
                    Inativo = false
                });
            }

			CvListagem.ItemsSource = lista;
		}

        private async void CvListagem_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void SwDeletar_Invoked(object sender, EventArgs e)
        {

        }

        private void BtnNovo_Clicked(object sender, EventArgs e)
        {
            limpaCampos();
            validaStatus(Cadastro);

            // PEGA PROXIMO REGISTRO
        }

        private void BtnEditar_Clicked(object sender, EventArgs e)
        {
            validaStatus(Editar);
        }

        private async void BtnSalvar_Clicked(object sender, EventArgs e)
        {
            if(await validaCampos())
            {
                await DisplayAlert("OK", "Tudo certo - Teste", "Ok");
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
                TxtNome.IsEnabled = false;
                ChkInativo.IsEnabled = false;
            }
            else
            {
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
    }
}