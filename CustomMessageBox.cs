using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Media;
using System.Windows.Forms;

namespace Civilium
{
    /// <summary>
    /// MessageBox customizado que segue o tema da aplicação Civilium
    /// </summary>
    public static class CiviliumMessageBox
    {
        /// <summary>
        /// Exibe um MessageBox com tema personalizado
        /// </summary>
        public static DialogResult Show(string mensagem, string titulo = "Civilium",
            MessageBoxButtons botoes = MessageBoxButtons.OK,
            MessageBoxIcon icone = MessageBoxIcon.Information,
            Form parent = null)
        {
            using (var msgBox = new CustomMessageBoxForm(mensagem, titulo, botoes, icone))
            {
                // Definir form pai se fornecido
                if (parent != null)
                {
                    msgBox.StartPosition = FormStartPosition.CenterParent;
                    return msgBox.ShowDialog(parent);
                }
                else
                {
                    msgBox.StartPosition = FormStartPosition.CenterScreen;
                    return msgBox.ShowDialog();
                }
            }
        }

        /// <summary>
        /// Versão simplificada para mensagens de informação
        /// </summary>
        public static DialogResult ShowInfo(string mensagem, string titulo = "Informação", Form parent = null)
        {
            return Show(mensagem, titulo, MessageBoxButtons.OK, MessageBoxIcon.Information, parent);
        }

        /// <summary>
        /// Versão simplificada para mensagens de erro
        /// </summary>
        public static DialogResult ShowError(string mensagem, string titulo = "Erro", Form parent = null)
        {
            return Show(mensagem, titulo, MessageBoxButtons.OK, MessageBoxIcon.Error, parent);
        }

        /// <summary>
        /// Versão simplificada para mensagens de aviso
        /// </summary>
        public static DialogResult ShowWarning(string mensagem, string titulo = "Aviso", Form parent = null)
        {
            return Show(mensagem, titulo, MessageBoxButtons.OK, MessageBoxIcon.Warning, parent);
        }

        /// <summary>
        /// Versão simplificada para confirmações
        /// </summary>
        public static DialogResult ShowQuestion(string mensagem, string titulo = "Confirmação", Form parent = null)
        {
            return Show(mensagem, titulo, MessageBoxButtons.YesNo, MessageBoxIcon.Question, parent);
        }
    }

    /// <summary>
    /// Form customizado para MessageBox com tema Civilium
    /// </summary>
    internal partial class CustomMessageBoxForm : Form
    {
        private string _mensagem;
        private MessageBoxButtons _botoes;
        private MessageBoxIcon _icone;
        private DialogResult _resultado = DialogResult.Cancel;
        private bool _temaEscuro;

        // Controles
        private Panel panelPrincipal;
        private Panel panelIcone;
        private Panel panelMensagem;
        private Panel panelBotoes;
        private Label labelMensagem;
        private PictureBox pictureBoxIcone;

        public CustomMessageBoxForm(string mensagem, string titulo, MessageBoxButtons botoes, MessageBoxIcon icone)
        {
            _mensagem = mensagem;
            _botoes = botoes;
            _icone = icone;
            _temaEscuro = AppConfig.TemaEscuro; // Usar configuração global do tema

            InitializeComponent();
            this.Text = titulo;

            CriarLayout();
            ConfigurarIcone();
            ConfigurarBotoes();
            AplicarTema();
            TocarSom();

            // Centralizar baseado no conteúdo
            AjustarTamanho();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Configurações básicas do form
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(400, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9F);
            this.KeyPreview = true;

            // Eventos
            this.KeyDown += CustomMessageBoxForm_KeyDown;

            // Aplicar tema da barra de título se disponível
            try
            {
                CustomTitleBar.AplicarTemaBarraTitulo(this, _temaEscuro);
            }
            catch { /* Ignorar se não suportado */ }

            this.ResumeLayout(false);
        }

        private void CriarLayout()
        {
            // Panel principal
            panelPrincipal = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };
            this.Controls.Add(panelPrincipal);

            // Panel do ícone (lado esquerdo)
            panelIcone = new Panel
            {
                Dock = DockStyle.Left,
                Width = 60,
                Padding = new Padding(0, 10, 10, 0)
            };
            panelPrincipal.Controls.Add(panelIcone);

            // PictureBox para o ícone
            pictureBoxIcone = new PictureBox
            {
                Size = new Size(32, 32),
                Location = new Point(0, 0),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            panelIcone.Controls.Add(pictureBoxIcone);

            // Panel da mensagem (centro)
            panelMensagem = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 10)
            };
            panelPrincipal.Controls.Add(panelMensagem);

            // Label da mensagem
            labelMensagem = new Label
            {
                Text = _mensagem,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5F),
                AutoSize = false,
                UseMnemonic = false
            };
            panelMensagem.Controls.Add(labelMensagem);

            // Panel dos botões (parte inferior)
            panelBotoes = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(0, 10, 0, 0)
            };
            panelPrincipal.Controls.Add(panelBotoes);
        }

        private void ConfigurarIcone()
        {
            Color corIcone = _temaEscuro ? Color.White : Color.Black;

            switch (_icone)
            {
                case MessageBoxIcon.Information:
                    pictureBoxIcone.Image = CriarIconeInformacao(corIcone);
                    break;
                case MessageBoxIcon.Warning:
                    pictureBoxIcone.Image = CriarIconeAviso();
                    break;
                case MessageBoxIcon.Error:
                    pictureBoxIcone.Image = CriarIconeErro();
                    break;
                case MessageBoxIcon.Question:
                    pictureBoxIcone.Image = CriarIconePergunta(corIcone);
                    break;
                default:
                    pictureBoxIcone.Visible = false;
                    panelIcone.Visible = false;
                    break;
            }
        }

        private void ConfigurarBotoes()
        {
            var botoes = new List<Button>();

            switch (_botoes)
            {
                case MessageBoxButtons.OK:
                    botoes.Add(CriarBotao("OK", DialogResult.OK, true));
                    break;

                case MessageBoxButtons.OKCancel:
                    botoes.Add(CriarBotao("Cancelar", DialogResult.Cancel, false));
                    botoes.Add(CriarBotao("OK", DialogResult.OK, true));
                    break;

                case MessageBoxButtons.YesNo:
                    botoes.Add(CriarBotao("Não", DialogResult.No, false));
                    botoes.Add(CriarBotao("Sim", DialogResult.Yes, true));
                    break;

                case MessageBoxButtons.YesNoCancel:
                    botoes.Add(CriarBotao("Cancelar", DialogResult.Cancel, false));
                    botoes.Add(CriarBotao("Não", DialogResult.No, false));
                    botoes.Add(CriarBotao("Sim", DialogResult.Yes, true));
                    break;

                case MessageBoxButtons.RetryCancel:
                    botoes.Add(CriarBotao("Cancelar", DialogResult.Cancel, false));
                    botoes.Add(CriarBotao("Repetir", DialogResult.Retry, true));
                    break;

                case MessageBoxButtons.AbortRetryIgnore:
                    botoes.Add(CriarBotao("Ignorar", DialogResult.Ignore, false));
                    botoes.Add(CriarBotao("Repetir", DialogResult.Retry, false));
                    botoes.Add(CriarBotao("Abortar", DialogResult.Abort, true));
                    break;
            }

            // Posicionar botões da direita para esquerda
            int x = panelBotoes.Width - 10;
            foreach (var botao in botoes)
            {
                x -= botao.Width + 10;
                botao.Location = new Point(x, 10);
                panelBotoes.Controls.Add(botao);
            }
        }

        private Button CriarBotao(string texto, DialogResult resultado, bool isPrimario)
        {
            var botao = new Button
            {
                Text = texto,
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Cursor = Cursors.Hand,
                Tag = resultado
            };

            // Definir cores baseadas no tema
            if (isPrimario)
            {
                // Botão primário (azul)
                botao.BackColor = Color.FromArgb(0, 122, 255);
                botao.ForeColor = Color.White;
                botao.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 100, 220);
                botao.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 80, 180);
            }
            else
            {
                // Botão secundário
                if (_temaEscuro)
                {
                    botao.BackColor = Color.FromArgb(60, 60, 60);
                    botao.ForeColor = Color.White;
                    botao.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 80);
                    botao.FlatAppearance.MouseDownBackColor = Color.FromArgb(100, 100, 100);
                }
                else
                {
                    botao.BackColor = Color.FromArgb(240, 240, 240);
                    botao.ForeColor = Color.Black;
                    botao.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 220, 220);
                    botao.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, 200, 200);
                }
            }

            botao.FlatAppearance.BorderSize = 0;
            botao.Click += (s, e) => {
                _resultado = (DialogResult)botao.Tag;
                this.DialogResult = _resultado;
                this.Close();
            };

            return botao;
        }

        private void AplicarTema()
        {
            if (_temaEscuro)
            {
                // Tema escuro
                this.BackColor = Color.FromArgb(45, 45, 48);
                panelPrincipal.BackColor = Color.FromArgb(45, 45, 48);
                panelIcone.BackColor = Color.FromArgb(45, 45, 48);
                panelMensagem.BackColor = Color.FromArgb(45, 45, 48);
                panelBotoes.BackColor = Color.FromArgb(45, 45, 48);
                labelMensagem.ForeColor = Color.White;
            }
            else
            {
                // Tema claro
                this.BackColor = Color.White;
                panelPrincipal.BackColor = Color.White;
                panelIcone.BackColor = Color.White;
                panelMensagem.BackColor = Color.White;
                panelBotoes.BackColor = Color.White;
                labelMensagem.ForeColor = Color.Black;
            }
        }

        private void AjustarTamanho()
        {
            // Calcular tamanho baseado no texto
            using (Graphics g = this.CreateGraphics())
            {
                var tamanhoTexto = g.MeasureString(_mensagem, labelMensagem.Font, 300);

                int largura = Math.Max(300, (int)tamanhoTexto.Width + 120);
                int altura = Math.Max(150, (int)tamanhoTexto.Height + 120);

                largura = Math.Min(largura, 600); // Máximo 600px
                altura = Math.Min(altura, 400);   // Máximo 400px

                this.Size = new Size(largura, altura);
            }
        }

        private void TocarSom()
        {
            try
            {
                switch (_icone)
                {
                    case MessageBoxIcon.Information:
                        SystemSounds.Asterisk.Play();
                        break;
                    case MessageBoxIcon.Warning:
                        SystemSounds.Exclamation.Play();
                        break;
                    case MessageBoxIcon.Error:
                        SystemSounds.Hand.Play();
                        break;
                    case MessageBoxIcon.Question:
                        SystemSounds.Question.Play();
                        break;
                }
            }
            catch { /* Ignorar erros de som */ }
        }

        private void CustomMessageBoxForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                _resultado = DialogResult.Cancel;
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                // Simular clique no botão primário
                var botaoPrimario = panelBotoes.Controls.OfType<Button>()
                    .FirstOrDefault(b => b.BackColor == Color.FromArgb(0, 122, 255));
                botaoPrimario?.PerformClick();
            }
        }

        // Métodos para criar ícones personalizados
        private Image CriarIconeInformacao(Color cor)
        {
            var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(0, 122, 255)))
                {
                    g.FillEllipse(brush, 2, 2, 28, 28);
                }
                using (var brush = new SolidBrush(Color.White))
                using (var font = new Font("Segoe UI", 16F, FontStyle.Bold))
                {
                    g.DrawString("i", font, brush, new RectangleF(0, 0, 32, 32),
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }
            }
            return bitmap;
        }

        private Image CriarIconeAviso()
        {
            var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(255, 193, 7)))
                {
                    PointF[] triangle = {
                        new PointF(16, 2),
                        new PointF(30, 28),
                        new PointF(2, 28)
                    };
                    g.FillPolygon(brush, triangle);
                }
                using (var brush = new SolidBrush(Color.Black))
                using (var font = new Font("Segoe UI", 14F, FontStyle.Bold))
                {
                    g.DrawString("!", font, brush, new RectangleF(0, 0, 32, 32),
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }
            }
            return bitmap;
        }

        private Image CriarIconeErro()
        {
            var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(220, 53, 69)))
                {
                    g.FillEllipse(brush, 2, 2, 28, 28);
                }
                using (var pen = new Pen(Color.White, 3))
                {
                    g.DrawLine(pen, 10, 10, 22, 22);
                    g.DrawLine(pen, 22, 10, 10, 22);
                }
            }
            return bitmap;
        }

        private Image CriarIconePergunta(Color cor)
        {
            var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(0, 122, 255)))
                {
                    g.FillEllipse(brush, 2, 2, 28, 28);
                }
                using (var brush = new SolidBrush(Color.White))
                using (var font = new Font("Segoe UI", 16F, FontStyle.Bold))
                {
                    g.DrawString("?", font, brush, new RectangleF(0, 0, 32, 32),
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }
            }
            return bitmap;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                pictureBoxIcone?.Image?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}