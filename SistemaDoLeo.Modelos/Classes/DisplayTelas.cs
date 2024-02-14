using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaDoLeo.Modelos.Classes
{
    public class DisplayTelas
    {
        public int Id { get; set; }
        public int OperadorId { get; set; }
        public int TelaId { get; set; }
        public bool Ativo { get; set; }
        public bool EnableAtivo { get; set; }
        public bool Editar { get; set; }
        public bool EnableEditar { get; set; }
        public bool Excluir { get; set; }
        public bool EnableExcluir { get; set; }
        public bool Novo { get; set; }
        public bool EnableNovo { get; set; }
        public string Nome { get; set; }
    }
}
