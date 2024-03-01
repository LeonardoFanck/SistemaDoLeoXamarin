using SistemaDoLeo.Modelos.Classes;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SistemaDoLeo.Paginas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Home : ContentPage
    {
        public Home(Operador operador)
        {
            InitializeComponent();

            LblNome.Text += operador.Nome;
        }
    }
}