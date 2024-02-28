using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaDoLeo.Relatorios
{
    internal static class Extensions
    {
        public static float ToDpi(this float centimer)
        {
            var inch = centimer / 2.54;
            return (float)(inch * 72);
        }
    }
}
