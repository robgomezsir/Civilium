using System;
using System.Drawing;
using System.Windows.Forms;

namespace Civilium
{
    public partial class TimeoutForm : Form
    {
        public int Timeout { get; private set; }

        private TextBox txtTimeout;
        private Button btnOk;
        private Button btnCancelar;
        private Label lblTitulo;

        private readonly bool temaEscuro;

        public TimeoutForm(bool temaEscuro, int timeoutConsultaSegundosInicial)
        {
            this.temaEscuro = temaEscuro;

            // Form config
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(320, 180);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Definir Timeout";

            InicializarControles();
            txtTimeout.Text = timeoutConsultaSegundosInicial.ToString();
            AplicarTema();
        }

        private void InicializarControles()
        {
            lblTitulo = new Label()
            {
                Text = "Tempo de espera da consulta na Receita (10 a 300 s):",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblTitulo);

            txtTimeout = new TextBox()
            {
                Width = 240,
                Height = 32,
                Top = 50,
                Left = (this.ClientSize.Width - 240) / 2,
                Font = new Font("Segoe UI", 12),
                TextAlign = HorizontalAlignment.Center
            };
            this.Controls.Add(txtTimeout);

            btnOk = new Button()
            {
                Text = "Confirmar",
                Width = 120,
                Height = 36,
                Top = 100,
                Left = 30,
                FlatStyle = FlatStyle.Flat
            };
            btnOk.Click += BtnOk_Click;
            this.Controls.Add(btnOk);

            btnCancelar = new Button()
            {
                Text = "Cancelar",
                Width = 120,
                Height = 36,
                Top = 100,
                Left = 170,
                FlatStyle = FlatStyle.Flat
            };
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancelar);
        }

        private void AplicarTema()
        {
            Color backColor = temaEscuro ? Color.FromArgb(45, 45, 48) : Color.White;
            Color foreColor = temaEscuro ? Color.White : Color.Black;

            this.BackColor = backColor;

            lblTitulo.ForeColor = foreColor;

            txtTimeout.BackColor = temaEscuro ? Color.FromArgb(30, 30, 30) : Color.White;
            txtTimeout.ForeColor = foreColor;
            txtTimeout.BorderStyle = BorderStyle.FixedSingle;

            btnOk.BackColor = Color.DodgerBlue;
            btnOk.ForeColor = Color.White;
            btnOk.FlatAppearance.BorderSize = 0;

            btnCancelar.BackColor = Color.Gray;
            btnCancelar.ForeColor = Color.White;
            btnCancelar.FlatAppearance.BorderSize = 0;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtTimeout.Text, out int valor) && valor >= 10 && valor <= 300)
            {
                Timeout = valor;
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Digite um valor entre 10 e 300 segundos.", "Erro");
            }
        }
    }
}
