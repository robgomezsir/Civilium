using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civilium
{
    public static class StringExtensions
    {
        public static string MascararCPF(this string cpf)
        {
            if (string.IsNullOrEmpty(cpf) || cpf.Length < 4)
                return "***.***.***-**";

            return $"{cpf.Substring(0, 3)}.***.***-{cpf.Substring(cpf.Length - 2)}";
        }

        public static string Truncar(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }
    }
}
