using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SistemaDoLeo.Paginas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Sobre : ContentPage
    {
        public Sobre()
        {
            InitializeComponent();
        }

        private async void BtnVoltar_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}