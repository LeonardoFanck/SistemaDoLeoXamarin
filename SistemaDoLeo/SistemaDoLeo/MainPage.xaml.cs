using Newtonsoft.Json;
using Org.Apache.Http;
using SistemaDoLeo.DB;
using SistemaDoLeo.Modelos.Classes;
using SistemaDoLeo.Paginas;
using SistemaDoLeo.Seguranca;
using SistemaDoLeo.Toast;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using static Android.Icu.Text.CaseMap;

namespace SistemaDoLeo
{
    public partial class MainPage : ContentPage
    {
        private bool carregando;
        private string Titulo = "Sistema do Leo 4.0";

        List<OperadorTela> listaPermissoes = new List<OperadorTela>();
        Operador operador;

        Regex regex = new Regex("[^0-9]");

        private HttpClient _client;

        private string urlOperador = $"{Links.ip}/{Links.operador}";
        private string urlOperadorTelas = $"{Links.ip}/{Links.operadorTelas}";

        public MainPage()
        {
            InitializeComponent();

            _client = new HttpClient(PermissaoDeCertificado.GetInsecureHandler());
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

                if(!await ValidarCampos())
                {
                    telaCarregamento();

                    return;
                }

                if(await ValidarOperador())
                {
                    //await Navigation.PushAsync(new AppShell(operador, listaPermissoes));
                    //await Shell.Current.GoToAsync(nameof(AppShell));

                    Application.Current.MainPage = new AppShell(operador, listaPermissoes);

                    LimpaCampos();
                }

                telaCarregamento();
            }
            catch (HttpRequestException ex)
            {
                if(ex.Message.Equals("Response status code does not indicate success: 404 (Not Found)."))
                {
                    new ToastBase(Titulo, "Operador não localizado", $"Operador {TxtOperador.Text} não localizado, informar um operador válido" +
                           $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
                }
                else
                {
                    new ToastBase(Titulo, "Ocorreu um erro", $"Contate o suporte do sistema" +
                           $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
                }

                telaCarregamento();
            }
            catch (Exception)
            {
                new ToastBase(Titulo, "Ocorreu um erro", $"Contate o suporte do sistema" +
                           $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
                telaCarregamento();
            }
        }

        private async Task<bool> ValidarCampos()
        {
            if(TxtOperador.Text == null || TxtOperador.Text == "")
            {
                new ToastBase(Titulo, "Campo operador obrigatório", $"Necessário informar o campo operador para prosseguir" +
                        $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return false;
            }
            else if(TxtSenha.Text == null || TxtSenha.Text == "")
            {
                new ToastBase(Titulo, "Campo senha obrigatório", $"Necessário informar o campo senha para prosseguir" +
                        $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return false;
            }

            return true;
        }

        private async Task<bool> ValidarOperador()
        {
            var json = await _client.GetStringAsync($"{urlOperador}/{TxtOperador.Text}");
            operador = JsonConvert.DeserializeObject<Operador>(json);

            if(operador == null)
            {
                new ToastBase(Titulo, "Operador não localizado", $"Operador {TxtOperador.Text} não localizado, informar um operador válido" +
                        $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return false;
            }

            if (!operador.Senha.Equals(TxtSenha.Text))
            {
                new ToastBase(Titulo, "Senha inválida", $"Senha inválida para o operador: {TxtOperador.Text}, favor tente novamente" +
                        $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return false;
            }

            await CarregaPermissoesOperador(operador.Id);

            return true;
        }

        private async Task CarregaPermissoesOperador(int id)
        {
            var json = await _client.GetStringAsync($"{urlOperadorTelas}/{id}");
            listaPermissoes = JsonConvert.DeserializeObject<List<OperadorTela>>(json);
        }

        private async void btnSobre_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new Sobre(this));
        }

        private void LimpaCampos()
        {
            TxtOperador.Text = string.Empty;
            TxtSenha.Text = string.Empty;
        }

        private async Task<string> LimpaValores(string valor)
        {
            return regex.Replace(valor, "");
        }

        private async void TxtOperador_Unfocused(object sender, FocusEventArgs e)
        {
            if(TxtOperador.Text == null)
            {
                TxtOperador.Text = string.Empty;

                return;
            }

            var valorLimpo = await LimpaValores(TxtOperador.Text);

            TxtOperador.Text = valorLimpo.TrimStart('0');
        }
    }
}
