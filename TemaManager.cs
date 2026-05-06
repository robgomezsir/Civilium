using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civilium
{
    public static class TemaManager
    {
        public static bool TemaEscuro { get; private set; } = true;

        public static void AlternarTema()
        {
            TemaEscuro = !TemaEscuro;
        }

        public static void DefinirTema(bool escuro)
        {
            TemaEscuro = escuro;
        }

        public static Color CorFundo => TemaEscuro ? Color.FromArgb(45, 45, 48) : Color.White;
        public static Color CorTexto => TemaEscuro ? Color.White : Color.Black;
        public static Color CorBotaoPrimario => Color.DodgerBlue;
        public static Color CorBotaoSecundario => Color.Gray;
    }

    public static class TemaUtils
    {
        public static void AplicarTema(Control controle)
        {
            controle.BackColor = TemaManager.CorFundo;
            controle.ForeColor = TemaManager.CorTexto;

            foreach (Control ctrl in controle.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.BackColor = btn.Text == "Cancelar" ? TemaManager.CorBotaoSecundario : TemaManager.CorBotaoPrimario;
                    btn.ForeColor = Color.White;
                    btn.FlatAppearance.BorderSize = 0;
                }
                else if (ctrl is TextBox || ctrl is ComboBox || ctrl is ListBox)
                {
                    ctrl.BackColor = TemaManager.TemaEscuro ? Color.FromArgb(30, 30, 30) : Color.White;
                    ctrl.ForeColor = TemaManager.CorTexto;
                }
                else
                {
                    AplicarTema(ctrl); // recursivo pros filhos
                }
            }
        }
    }

}
