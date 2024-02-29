using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SistemaDoLeo.Paginas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Sobre : ContentPage
    {
        public Sobre(object tela)
        {
            InitializeComponent();

            if(tela is AppShell)
            {
                BtnVoltar.IsVisible = false;
            }
            else
            {
                BtnVoltar.IsVisible = true;
            }
        }

        private async void BtnVoltar_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}