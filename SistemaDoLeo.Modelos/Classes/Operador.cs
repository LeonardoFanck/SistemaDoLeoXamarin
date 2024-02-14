using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaDoLeo.Modelos.Classes
{
    public class Operador
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public bool Admin { get; set; } = false;
        public bool Inativo { get; set; } = false;
    }
}
