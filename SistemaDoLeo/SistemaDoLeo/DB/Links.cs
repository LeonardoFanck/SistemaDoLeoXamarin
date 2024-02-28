using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaDoLeo.DB
{
    public class Links
    {
        public static string ip = "https://10.0.2.2:7097/api";

        public static string proximoRegistro = $"{ip}/ProximoRegistro/1";

        public static string pedido = "Pedido";
        public static string cliente = "Cliente";
        public static string formaPgto = "FormaPgto";
    }
}
