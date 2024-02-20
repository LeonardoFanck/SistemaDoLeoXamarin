
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views.Options;
using Xamarin.Forms;
using static Xamarin.Essentials.AppleSignInAuthenticator;

namespace SistemaDoLeo.Toast
{
    public class ToastBase
    {
        private string titulo;
        private string descricao;
        private string mensagem;
        private bool isClicavel;
        private string hexCor;

        public ToastBase(string titulo, string descricao, string mensagem, bool isClicavel, string hexCor)
        {
            this.titulo = titulo;
            this.descricao = descricao;
            this.mensagem = mensagem;
            this.isClicavel = isClicavel;
            this.hexCor = hexCor;

            gerarToast();
        }

        private async Task gerarToast()
        {
            // BOTÃO + AÇÃO A SER REALIZADA AO CLICAR
            var actions = new SnackBarActionOptions()
            {
                Action = () => App.Current.MainPage.DisplayAlert(titulo, mensagem, "Ok"),
                Text = "Detalhes"
            };

            // SNACK BAR
            var opcoes = new SnackBarOptions()
            {
                MessageOptions = new MessageOptions()
                {
                    Foreground = Color.Black,
                    Message = descricao
                },
                BackgroundColor = Color.FromHex("#e4e6eb"),
                Duration = TimeSpan.FromSeconds(3),
                Actions = new[] { actions }
            };

            await App.Current.MainPage.DisplaySnackBarAsync(opcoes);
        }
    }
}
