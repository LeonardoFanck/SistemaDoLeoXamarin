using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaDoLeo.Modelos.Classes
{
    public class Produto
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }

        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; } = decimal.Zero;
        public decimal Custo { get; set; } = decimal.Zero;
        public int Categoria { get; set; }
        public string Unidade { get; set; } = string.Empty;
        public long Estoque { get; set; } = 0;
        public bool Inativo { get; set; } = false;
    }
}
