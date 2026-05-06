using System.Drawing;
using System.Windows.Forms;

namespace Civilium
{
    internal enum FonteDadosPesquisa
    {
        Nenhuma,
        ArquivoCsv,
        InsercaoManual
    }

    /// <summary>
    /// Permite escolher entre carregar CSV ou abrir a inserção manual.
    /// </summary>
    internal sealed class FonteDadosDialog : Form
    {
        internal FonteDadosPesquisa Escolha { get; private set; } = FonteDadosPesquisa.Nenhuma;

        public FonteDadosDialog(bool temaEscuro)
        {
            Text = "Origem dos dados";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(440, 232);
            ShowInTaskbar = false;

            var lbl = new Label
            {
                Text = "Como deseja informar os dados da pesquisa?",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Padding = new Padding(8, 10, 8, 4)
            };

            var btnCsv = new Button
            {
                Text = "📄  Carregar arquivo CSV",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabIndex = 0
            };
            btnCsv.Click += (_, _) =>
            {
                Escolha = FonteDadosPesquisa.ArquivoCsv;
                DialogResult = DialogResult.OK;
            };

            var btnManual = new Button
            {
                Text = "✏  Inserir dados manualmente",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabIndex = 1
            };
            btnManual.Click += (_, _) =>
            {
                Escolha = FonteDadosPesquisa.InsercaoManual;
                DialogResult = DialogResult.OK;
            };

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Dock = DockStyle.Fill,
                DialogResult = DialogResult.Cancel,
                TabIndex = 2,
                Cursor = Cursors.Hand
            };
            btnCancelar.Click += (_, _) =>
            {
                Escolha = FonteDadosPesquisa.Nenhuma;
                DialogResult = DialogResult.Cancel;
            };

            var tabela = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(20, 8, 20, 14)
            };
            tabela.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tabela.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tabela.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tabela.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tabela.Controls.Add(lbl, 0, 0);
            tabela.Controls.Add(btnCsv, 0, 1);
            tabela.Controls.Add(btnManual, 0, 2);
            tabela.Controls.Add(btnCancelar, 0, 3);

            Controls.Add(tabela);

            AplicarCores(temaEscuro, lbl, btnCsv, btnManual, btnCancelar);

            AcceptButton = btnCsv;
            CancelButton = btnCancelar;
        }

        private static void AplicarCores(bool escuro, Label lbl, Button btnCsv, Button btnManual, Button btnCancelar)
        {
            Color fundo = escuro ? Color.FromArgb(45, 45, 48) : Color.White;
            Color texto = escuro ? Color.WhiteSmoke : Color.Black;

            BackColor = fundo;
            lbl.BackColor = fundo;
            lbl.ForeColor = texto;

            btnCsv.BackColor = Color.FromArgb(97, 136, 197);
            btnCsv.ForeColor = Color.White;
            btnCsv.FlatStyle = FlatStyle.Flat;
            btnCsv.FlatAppearance.BorderSize = 0;

            btnManual.BackColor = Color.FromArgb(52, 152, 90);
            btnManual.ForeColor = Color.White;
            btnManual.FlatStyle = FlatStyle.Flat;
            btnManual.FlatAppearance.BorderSize = 0;

            btnCancelar.BackColor = escuro ? Color.FromArgb(90, 90, 90) : Color.FromArgb(150, 150, 150);
            btnCancelar.ForeColor = Color.White;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.FlatAppearance.BorderSize = 0;
        }
    }
}
