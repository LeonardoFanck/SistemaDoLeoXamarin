﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaDoLeo.DB
{
    public class Links
    {
        public static string ip = "https://10.0.2.2:7097/api"; // EMULADOR
        //public static string ip = "http://192.168.5.76:8090/api"; // COMPUMATE

        public static string proximoRegistro = $"{ip}/ProximoRegistro/1";

        public static string pedido = "Pedido";
        public static string cliente = "Cliente";
        public static string formaPgto = "FormaPgto";
        public static string operador = "Operador";
        public static string operadorTelas = "OperadorTelas";
    }
}
