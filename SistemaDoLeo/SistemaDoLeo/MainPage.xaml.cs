using SistemaDoLeo.Paginas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SistemaDoLeo
{
    public partial class MainPage : ContentPage
    {
        private bool carregando;

        public MainPage()
        {
            InitializeComponent();
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

        private async void btnLogar_Clicked(object sender, EventArgs e)
        {
            try
            {
                telaCarregamento();

                //var logar = new ServicoUsers();
                //var verificarLogin = await logar.verificarLogin(txtEmail.Text, txtSenha.Text);

                //if (verificarLogin)
                //{
                //    await limpaCampos();

                //    await Navigation.PushAsync(new Contatos());

                //    telaCarregamento();
                //}
                //else
                //{
                //    await DisplayAlert("Erro", "Usuário e senha não correspndem", "Ok");
                //    telaCarregamento();
                //}

                await Navigation.PushAsync(new TelaPrincipal());

                telaCarregamento();
            }
            catch (Exception)
            {
                await DisplayAlert("Erro", "Ocorreu um erro", "Ok");
                telaCarregamento();
            }
        }

        private async void btnSobre_Clicked(object sender, EventArgs e)
        {
            //await Navigation.PushAsync(new Paginas.Sobre());
            await Navigation.PushModalAsync(new Sobre());
        }
    }
}
