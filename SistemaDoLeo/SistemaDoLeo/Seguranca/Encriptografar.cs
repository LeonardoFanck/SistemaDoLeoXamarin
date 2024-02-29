using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SistemaDoLeo.Seguranca
{
    public class Encriptografar
    {
        private string criptografado = string.Empty;

        public Encriptografar(string texto)
        {
            var md5 = MD5.Create();
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(texto);
            byte[] hash = md5.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            criptografado = sb.ToString();
        }

        public string GetArquivoCriptografado()
        {
            return criptografado;
        }
    }
}
