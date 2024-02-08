using SistemaDoLeo.Modelos.Classes;
using System;

using Xamarin.Forms;

namespace SistemaDoLeo.Paginas
{
    public partial class TelaPrincipal : FlyoutPage
    {
        public TelaPrincipal()
        {
            InitializeComponent();

            flyout.lista.ItemSelected += OnSelectItem;
        }

        private void OnSelectItem(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as FlyoutItens;
            if (item != null)
            {
                // NORMAL
                if (item.PaginaAlvo == typeof(Home))
                {
                    Detail = new NavigationPage((Page)Activator.CreateInstance(item.PaginaAlvo));
                }
                else if (item.PaginaAlvo == typeof(Sobre)) // PUSH MODAL
                {
                    Navigation.PushModalAsync((Page)Activator.CreateInstance(item.PaginaAlvo));
                }
                else // PUSH
                {
                    Navigation.PushAsync((Page)Activator.CreateInstance(item.PaginaAlvo));
                }
                flyout.lista.SelectedItem = null;
                IsPresented = false;
            }
        }
    }
}