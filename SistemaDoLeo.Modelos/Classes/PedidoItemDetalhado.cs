using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaDoLeo.Modelos.Classes
{
    public class PedidoItemDetalhado
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int ProdutoId { get; set; }

        public string ProdutoNome { get; set; }

        public decimal Valor { get; set; }
        public int Quantidade { get; set; }
        public decimal Desconto { get; set; }
        public decimal Total { get; set; }
    }
}
