using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaDoLeo.Modelos.Classes
{
    public class Pedido
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int FormaPgtoId { get; set; }

        public DateTime Data { get; set; }
        public decimal Valor { get; set; } = decimal.Zero;
        public decimal Desconto { get; set; } = decimal.Zero;
        public decimal Total { get; set; } = decimal.Zero;
        public string TipoOperacao { get; set; } = string.Empty;
    }
}
