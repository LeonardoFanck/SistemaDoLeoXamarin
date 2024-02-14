using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaDoLeo.Modelos.Classes
{
    public class FormaPgto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Inativo { get; set; } = false;
    }
}
