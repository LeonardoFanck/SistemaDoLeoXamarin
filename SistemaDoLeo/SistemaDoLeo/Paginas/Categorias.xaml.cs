﻿using Newtonsoft.Json;
using SistemaDoLeo.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SistemaDoLeo.Modelos.Classes;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SistemaDoLeo.Toast;

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
        private ProximoRegistro proximoRegistro;

        OperadorTela permissoes;

        private readonly HttpClient _cliente;
        private string url = $"{Links.ip}/Categoria";
        private string Titulo = "Categoria";

        public Categorias(OperadorTela permissoes)
        {
            InitializeComponent();

            BindingContext = this;

            CurrentPage = Children[0];

            HttpClientHandler insecureHandler = PermissaoDeCertificado.GetInsecureHandler();
            _cliente = new HttpClient(insecureHandler);

            this.permissoes = permissoes;

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

            if (selecionado != null)
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
            if (!permissoes.Excluir)
            {
                new ToastBase(Titulo, "Acesso negado", $"Operador não tem permissão para excluir o registro!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            var selecionado = (sender as SwipeItem)?.BindingContext as Categoria;

            if (selecionado == null)
            {
                await DisplayAlert(Titulo, "Nenhum item selecionado", "Ok");

                return;
            }

            var confirmacao = await DisplayAlert(Titulo, $"Deseja realmente fazer a exclusão do Registro {selecionado.Nome}?", "Confirmar", "Cancelar");

            if (confirmacao)
            {
                var response = await _cliente.DeleteAsync($"{url}/{selecionado.Id}");

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, "Ocorreu um erro", "Ok");

                    return;
                }

                listaBase.Remove(selecionado);

                // LIMPA OS CAMPOS DO CADASTRO PARA NÃO DEIXAR EDITAR O ITEM EXCLUIDO
                limpaCampos();

                if(SrcBuscar.Text == null)
                {
                    CvListagem.ItemsSource = new List<Categoria>(listaBase);
                }
                else
                {
                    CvListagem.ItemsSource = new List<Categoria>(listaBase.Where(l => l.Nome.ToLower().Contains(SrcBuscar.Text.ToLower())).ToList());
                }

                new ToastBase(Titulo, "Registro Deletado", $"Registro: {selecionado.Id} - {selecionado.Nome} deletado com Sucesso" +
                    $"\n\n\n {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
            }
            else
            {
                return;
            }
        }

        private async void BtnNovo_Clicked(object sender, EventArgs e)
        {
            if (!permissoes.Novo)
            {
                new ToastBase(Titulo, "Acesso negado", $"Operador não tem permissão para criar um novo registro!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            limpaCampos();
            validaStatus(Cadastro);

            // PEGA PROXIMO REGISTRO
            var id = await PegaProximoRegistro();

            TxtCodigo.Text = id.ToString();
        }

        private async Task<int> PegaProximoRegistro()
        {
            var json = await _cliente.GetStringAsync(Links.proximoRegistro);

            proximoRegistro = JsonConvert.DeserializeObject<ProximoRegistro>(json);

            proximoRegistro.Categoria += 1;

            return proximoRegistro.Categoria;
        }

        private async void BtnEditar_Clicked(object sender, EventArgs e)
        {
            if (!permissoes.Editar)
            {
                new ToastBase(Titulo, "Acesso negado", $"Operador não tem permissão para editar o registro!" +
                    $"\n\n\n{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return;
            }

            if (TxtCodigo.Text == "" || TxtCodigo.Text == null)
            {
                await DisplayAlert(Titulo, "Necessário selecionar um item", "Ok");

                return;
            }

            validaStatus(Editar);
        }

        private async void BtnSalvar_Clicked(object sender, EventArgs e)
        {
            if (await validaCampos() == false)
            {
                return;
            }

            Categoria categoria = new Categoria();

            if (Status == Cadastro)
            {
                categoria = new Categoria
                {
                    Nome = TxtNome.Text,
                    Inativo = ChkInativo.IsChecked
                };

            }
            else if (Status == Editar)
            {
                categoria = new Categoria
                {
                    Id = Convert.ToInt32(TxtCodigo.Text),
                    Nome = TxtNome.Text,
                    Inativo = ChkInativo.IsChecked
                };
            }

            if (await SalvarRegistro(categoria))
            {
                validaStatus(Visualizar);
            }
        }

        private async Task<bool> SalvarRegistro(Categoria categoria)
        {
            var json = JsonConvert.SerializeObject(categoria);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            if (categoria.Id == 0)
            {
                var response = await _cliente.PostAsync(url, conteudo);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, "Ocorreu um erro no Cadastro", "Ok");

                    return false;
                }

                var novaCategoria = JsonConvert.DeserializeObject<Categoria>(await response.Content.ReadAsStringAsync());

                await AtualizaProximoRegistro(novaCategoria.Id);

                TxtCodigo.Text = novaCategoria.Id.ToString();
                listaBase.Add(novaCategoria);
                CvListagem.ItemsSource = new List<Categoria>(listaBase);

                new ToastBase(Titulo, "Registro Salvo", $"Registro salvo com Sucesso!\n" +
                    $"Código: {novaCategoria.Id}\n" +
                    $"Nome: {novaCategoria.Nome}\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
            }
            else
            {
                var response = await _cliente.PutAsync($"{url}/{categoria.Id}", conteudo);

                if (!response.IsSuccessStatusCode)
                {
                    await DisplayAlert(Titulo, "Ocorreu um erro ao fazer a Alteração", "Ok");

                    return false;
                }

                listaBase.Remove(listaBase.FirstOrDefault(l => l.Id == categoria.Id));
                listaBase.Add(categoria);
                CvListagem.ItemsSource = new List<Categoria>(listaBase).OrderBy(i => i.Id);

                new ToastBase(Titulo, "Registro Atualizado", $"Registro atualizado com Sucesso!\n" +
                    $"Código: {TxtCodigo.Text}\n" +
                    $"Nome: {TxtNome.Text}\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());
            }

            return true;
        }

        private async Task AtualizaProximoRegistro(int id)
        {
            // ATUALIZA O PROXIMO REGISTRO COM BASE NO RETORNO DO POST
            proximoRegistro.Categoria = id;

            var json = JsonConvert.SerializeObject(proximoRegistro);
            var conteudo = new StringContent(json, Encoding.UTF8, "application/json");

            await _cliente.PutAsync(Links.proximoRegistro, conteudo);
        }

        private async Task<bool> validaCampos()
        {
            if (TxtCodigo.Text == "" || TxtCodigo.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Código\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                return false;
            }
            if (TxtNome.Text == "" || TxtNome.Text == null)
            {
                new ToastBase(Titulo, "Campo Obrigatório", $"Necessário informar o campo Nome\n\n\n " +
                    $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", true, Color.White.ToHex());

                TxtNome.Focus();

                return false;
            }

            return true;
        }

        private void validaStatus(int status)
        {
            if (status == Visualizar)
            {
                this.Status = Visualizar;

                TxtCodigo.IsEnabled = false;
                TxtNome.IsEnabled = false;
                ChkInativo.IsEnabled = false;

                BtnNovo.Text = "Novo";

                BtnEditar.IsEnabled = true;
                BtnNovo.IsEnabled = true;
                BtnSalvar.IsEnabled = false;
            }
            else if (status == Editar)
            {
                this.Status = Editar;

                TxtCodigo.IsEnabled = false;
                TxtNome.IsEnabled = true;
                ChkInativo.IsEnabled = true;

                BtnEditar.IsEnabled = false;
                BtnNovo.IsEnabled = false;
                BtnSalvar.IsEnabled = true;
            }
            else
            {
                this.Status = Cadastro;

                TxtCodigo.IsEnabled = false;
                TxtNome.IsEnabled = true;
                ChkInativo.IsEnabled = true;

                BtnNovo.Text = "Limpar";

                BtnEditar.IsEnabled = false;
                BtnNovo.IsEnabled = true;
                BtnSalvar.IsEnabled = true;
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
            CvListagem.ItemsSource = listaBase.Where(l => l.Nome.ToLower().Contains(SrcBuscar.Text.ToLower())).ToList();
        }

        private async void RefreshV_Refreshing(object sender, EventArgs e)
        {
            await CarregaLista();
        }
    }
}