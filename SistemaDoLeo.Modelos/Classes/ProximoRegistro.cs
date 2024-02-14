using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaDoLeo.Modelos.Classes
{
    public class ProximoRegistro
    {
        public int Id { get; set; }
        public int Categoria { get; set; }
        public int Cliente { get; set; }
        public int FormaPgto { get; set; }
        public int Operador { get; set; }
        public int Pedido { get; set; }
        public int Produto { get; set; }
    }
}
