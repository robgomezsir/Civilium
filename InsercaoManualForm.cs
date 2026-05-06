using System.Data;
using System.Globalization;
using System.Windows.Forms;

namespace Civilium
{
    /// <summary>
    /// Diálogo para preencher NOME, CPF e data de nascimento sem arquivo.
    /// </summary>
    public sealed class InsercaoManualForm : Form
    {
        private readonly DataGridView _grid;
        private readonly bool _temaEscuro;

        /// <summary>Tabela pronta para processamento (somente após OK).</summary>
        public DataTable? TabelaPreenchida { get; private set; }

        public InsercaoManualForm(bool temaEscuro)
        {
            _temaEscuro = temaEscuro;
            Text = "Inserir Dados Manualmente";
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(720, 480);
            Size = new Size(880, 560);
            ShowInTaskbar = false;

            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(20, 16, 20, 8) };

            var lblTitulo = new Label
            {
                Text = "📝  Inserção Manual de Dados",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lblAjuda = new Label
            {
                Text = "Insira os dados nas colunas abaixo. Pode adicionar quantas linhas quiser. " +
                       "Linhas totalmente vazias são ignoradas. Divida lotes grandes em várias pesquisas se desejar.",
                Font = new Font("Segoe UI", 9.25F),
                AutoSize = false,
                Width = 820,
                Height = 44,
                Location = new Point(0, 36)
            };

            headerPanel.Controls.Add(lblTitulo);
            headerPanel.Controls.Add(lblAjuda);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                RowHeadersWidth = 56,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 32 }
            };

            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNome", HeaderText = "NOME", FillWeight = 40 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCpf", HeaderText = "CPF", FillWeight = 25 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colData",
                HeaderText = "DATA NASCIMENTO (dd/MM/aaaa)",
                FillWeight = 25
            });

            for (var i = 0; i < 10; i++)
                _grid.Rows.Add();

            var rodape = new Panel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(16, 8, 16, 10) };

            var btnCancelar = new Button
            {
                Text = "✕  Cancelar",
                Width = 130,
                Height = 38,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(0, 12),
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.BackColor = _temaEscuro ? Color.FromArgb(90, 90, 90) : Color.FromArgb(150, 150, 150);
            btnCancelar.ForeColor = Color.White;
            btnCancelar.Click += (_, _) => DialogResult = DialogResult.Cancel;

            var btnApagar = new Button
            {
                Text = "🗑  Apagar Tudo",
                Width = 150,
                Height = 38,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
            btnApagar.Left = rodape.Width - 300 - 16;
            btnApagar.Top = 12;
            btnApagar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnApagar.Click += BtnApagar_Click;

            var btnEnviar = new Button
            {
                Text = "✓  Enviar Dados",
                Width = 150,
                Height = 38,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
            btnEnviar.Left = rodape.Width - 140;
            btnEnviar.Top = 12;
            btnEnviar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            rodape.Resize += (_, _) =>
            {
                btnEnviar.Left = rodape.Width - btnEnviar.Width - 8;
                btnApagar.Left = btnEnviar.Left - btnApagar.Width - 12;
            };

            btnEnviar.Click += BtnEnviar_Click;

            btnApagar.FlatAppearance.BorderSize = 0;
            btnApagar.BackColor = Color.FromArgb(220, 68, 55);
            btnApagar.ForeColor = Color.White;

            btnEnviar.FlatAppearance.BorderSize = 0;
            btnEnviar.BackColor = Color.FromArgb(52, 168, 83);
            btnEnviar.ForeColor = Color.White;

            rodape.Controls.Add(btnCancelar);
            rodape.Controls.Add(btnApagar);
            rodape.Controls.Add(btnEnviar);

            var fill = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 0) };
            fill.Controls.Add(_grid);

            Controls.Add(fill);
            Controls.Add(rodape);
            Controls.Add(headerPanel);

            AplicarTemaFormulario(headerPanel, lblTitulo, lblAjuda);
            EstilizarGrid();
            rodape.BackColor = headerPanel.BackColor;

            CancelButton = btnCancelar;
        }

        private void BtnApagar_Click(object? sender, EventArgs e)
        {
            if (CiviliumMessageBox.ShowQuestion(
                    "Limpar todas as linhas da tabela?",
                    "Apagar Tudo", this) != DialogResult.Yes)
                return;

            _grid.Rows.Clear();
            for (var i = 0; i < 10; i++)
                _grid.Rows.Add();
        }

        private void BtnEnviar_Click(object? sender, EventArgs e)
        {
            _grid.EndEdit();

            var cultura = new CultureInfo("pt-BR");
            var erros = new List<string>();
            var dt = ArquivoService.CriarEstruturaTabelaConsultaVazia();
            var linhaUi = 0;

            foreach (DataGridViewRow row in _grid.Rows)
            {
                linhaUi++;
                if (row.IsNewRow) continue;

                var nome = row.Cells[0].Value?.ToString()?.Trim() ?? "";
                var cpf = row.Cells[1].Value?.ToString()?.Trim() ?? "";
                var dataTxt = row.Cells[2].Value?.ToString()?.Trim() ?? "";

                if (nome.Length == 0 && cpf.Length == 0 &&
                    string.IsNullOrWhiteSpace(dataTxt))
                    continue;

                var partesErr = new List<string>();
                if (nome.Length < 2)
                    partesErr.Add("nome (mín. 2 caracteres)");
                var digitosCpf = new string(cpf.Where(char.IsDigit).ToArray());
                if (digitosCpf.Length != 11)
                    partesErr.Add("CPF com 11 dígitos");

                var dataOk = false;
                if (!string.IsNullOrWhiteSpace(dataTxt))
                {
                    var normalizada = dataTxt.Replace("-", "/").Replace(".", "/");
                    var partes = normalizada.Split('/');
                    if (partes.Length == 3)
                    {
                        var ds =
                            $"{partes[0].PadLeft(2, '0')}/{partes[1].PadLeft(2, '0')}/{(partes[2].Length == 2 ? "20" + partes[2] : partes[2])}";
                        if (DateTime.TryParseExact(ds, "dd/MM/yyyy", cultura, DateTimeStyles.None, out _))
                            dataOk = true;
                    }
                }
                if (!dataOk)
                    partesErr.Add("data válida (dd/MM/aaaa)");

                if (partesErr.Count > 0)
                {
                    erros.Add($"Linha {linhaUi}: informe {string.Join(", ", partesErr)}.");
                    continue;
                }

                dt.Rows.Add(nome, cpf, dataTxt);
            }

            if (erros.Count > 0)
            {
                CiviliumMessageBox.ShowWarning(string.Join(Environment.NewLine, erros), "Corrija os dados", this);
                return;
            }

            if (dt.Rows.Count == 0)
            {
                CiviliumMessageBox.ShowWarning(
                    "Nenhuma linha preenchida. Informe ao menos um registro completo.",
                    "Aviso", this);
                return;
            }

            TabelaPreenchida = dt;
            DialogResult = DialogResult.OK;
        }

        private void AplicarTemaFormulario(Panel headerPanel, Label lblTitulo, Label lblAjuda)
        {
            Color fundo = _temaEscuro ? Color.FromArgb(45, 45, 48) : Color.White;
            Color texto = _temaEscuro ? Color.WhiteSmoke : Color.Black;

            BackColor = fundo;
            ForeColor = texto;
            headerPanel.BackColor = fundo;
            lblTitulo.ForeColor = texto;
            lblAjuda.ForeColor = _temaEscuro ? Color.FromArgb(200, 200, 200) : Color.FromArgb(60, 60, 60);
        }

        private void EstilizarGrid()
        {
            _grid.BackgroundColor = _temaEscuro ? Color.FromArgb(30, 30, 30) : Color.White;
            _grid.GridColor = _temaEscuro ? Color.FromArgb(70, 70, 70) : Color.LightGray;
            _grid.EnableHeadersVisualStyles = false;
            _grid.ColumnHeadersDefaultCellStyle.BackColor =
                _temaEscuro ? Color.FromArgb(55, 55, 58) : Color.FromArgb(245, 245, 245);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = _temaEscuro ? Color.White : Color.Black;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            _grid.DefaultCellStyle.BackColor = _temaEscuro ? Color.FromArgb(30, 30, 30) : Color.White;
            _grid.DefaultCellStyle.ForeColor = _temaEscuro ? Color.WhiteSmoke : Color.Black;
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            _grid.DefaultCellStyle.SelectionForeColor = Color.White;
            _grid.RowHeadersDefaultCellStyle.BackColor =
                _temaEscuro ? Color.FromArgb(50, 50, 52) : Color.FromArgb(235, 235, 235);
            _grid.RowHeadersDefaultCellStyle.ForeColor = _temaEscuro ? Color.WhiteSmoke : Color.Black;
            _grid.AlternatingRowsDefaultCellStyle.BackColor =
                _temaEscuro ? Color.FromArgb(38, 38, 40) : Color.FromArgb(252, 252, 252);
            _grid.AlternatingRowsDefaultCellStyle.ForeColor = _temaEscuro ? Color.WhiteSmoke : Color.Black;
        }
    }
}
