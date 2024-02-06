using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SistemaDoLeo.Classes;

namespace SistemaDoLeo.Paginas
{
	public partial class TelaPrincipal : FlyoutPage
	{
		public TelaPrincipal ()
		{
			InitializeComponent ();

			flyout.lista.ItemSelected += OnSelectItem;
		}

        private void OnSelectItem(object sender, SelectedItemChangedEventArgs e)
        {
			var item = e.SelectedItem as Classes.FlyoutItem;
			if(item != null)
			{
				// NORMAL
				if(item.PaginaAlvo == typeof(Home))
				{
                    Detail = new NavigationPage((Page)Activator.CreateInstance(item.PaginaAlvo));
                }
				else if(item.PaginaAlvo == typeof(Sobre)) // PUSH MODAL
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