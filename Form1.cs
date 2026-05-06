
using OfficeOpenXml;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System;
using System.Linq;

namespace Civilium
{
    public partial class Form1 : Form
    {
        #region Propriedades e Campos Privados
        private DataTable tabelaDados;
        private CancellationTokenSource cts;
        private bool temaEscuro;
        private bool menuExpandido = false;
        private Button btnToggleMenu;
        private ToolTip toolTipMenu = new ToolTip();
        private ToolTip toolTipBotoesPrincipais = new ToolTip();

        private Panel _menuLateral;
        private Panel MenuLateral
        {
            get
            {
                if (_menuLateral == null)
                    _menuLateral = Controls.Find("menuLateral", true).FirstOrDefault() as Panel;
                return _menuLateral;
            }
        }
        #endregion

        #region Construtor e Inicialização
        public Form1()
        {
            InitializeComponent();

            // Configurações iniciais
            InicializarConfiguracoes();
            CriarMenuLateral();
            AjustarMenuVisual();

            // Aplicar tema e configurações visuais
            temaEscuro = AppConfig.TemaEscuro;
            TemaUtils.AplicarTema(this);
            AplicarTema(temaEscuro);

            InicializarBarraTituloPersonalizada();

            // Configurações da janela
            ConfigurarJanela();

            // Estado inicial dos botões
            InicializarEstadoBotoes();

            // Configurar tooltips
            ConfigurarTooltips();

            Logger.LogInformation("Aplicação Civilium iniciada com sucesso");
        }

        private void InicializarBarraTituloPersonalizada()
        {
            try
            {
                // Aplicar tema inicial da barra de título
                CustomTitleBar.AplicarTemaBarraTitulo(this, temaEscuro);
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Erro ao inicializar barra de título personalizada: {ex.Message}");
            }
        }

        private void InicializarConfiguracoes()
        {
            AppConfig.CarregarConfiguracoes();
            ConfigValidator.ValidarConfiguracoes();
            ConsultaService.SincronizarTimeoutsDoAppConfig();
        }

        private void ConfigurarJanela()
        {
            this.WindowState = FormWindowState.Maximized;
            this.Icon = new Icon("Civilium.ico");
            this.TopMost = false;
        }

        private void InicializarEstadoBotoes()
        {
            btnIniciar.Enabled = true;
            btnParar.Enabled = false;
            btnExportar.Enabled = false;

            AplicarTemaNosBotoesPrincipais();
            ConfigurarBotoesProcessamento(false);
        }

        private void ConfigurarTooltips()
        {
            ConfigurarTooltipsBotoesPrincipais();
            ConfigurarTooltipsMenu();
        }
        #endregion

        #region Eventos dos Botões Principais
        private async void btnIniciar_Click(object sender, EventArgs e)
        {
            using (var fonteDlg = new FonteDadosDialog(temaEscuro))
            {
                if (fonteDlg.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    if (fonteDlg.Escolha == FonteDadosPesquisa.ArquivoCsv)
                    {
                        if (openFileDialog1.ShowDialog() != DialogResult.OK)
                            return;

                        Logger.LogInformation($"Iniciando processo de consulta. Arquivo: {openFileDialog1.FileName}");
                        tabelaDados = ArquivoService.CarregarCSV(openFileDialog1.FileName);

                        if (tabelaDados.Rows.Count == 0)
                        {
                            CiviliumMessageBox.ShowWarning("Nenhum dado encontrado no arquivo selecionado.",
                                "Aviso", this);
                            return;
                        }
                    }
                    else if (fonteDlg.Escolha == FonteDadosPesquisa.InsercaoManual)
                    {
                        using (var manual = new InsercaoManualForm(temaEscuro))
                        {
                            if (manual.ShowDialog(this) != DialogResult.OK)
                                return;
                            tabelaDados = manual.TabelaPreenchida;
                        }

                        if (tabelaDados == null || tabelaDados.Rows.Count == 0)
                        {
                            CiviliumMessageBox.ShowWarning("Nenhum dado para processar.",
                                "Aviso", this);
                            return;
                        }

                        Logger.LogInformation(
                            $"Iniciando consulta com dados manuais. Total de registros: {tabelaDados.Rows.Count}");
                    }
                    else
                        return;

                    Logger.LogInformation($"Dados carregados. Total de registros: {tabelaDados.Rows.Count}");

                    ConfigurarBotoesProcessamento(true);
                    cts = new CancellationTokenSource();

                    await ProcessarConsultasAsync(cts.Token);

                    if (!cts.Token.IsCancellationRequested)
                    {
                        CiviliumMessageBox.ShowInfo("Processo concluído com sucesso!",
                            "Sucesso", this);

                        Logger.LogInformation("Processo de consulta concluído com sucesso");
                    }
                    else
                    {
                        CiviliumMessageBox.ShowInfo("Processo interrompido pelo usuário.",
                            "Informação", this);

                        Logger.LogInformation("Processo de consulta interrompido pelo usuário");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Erro durante o processamento", ex);

                    CiviliumMessageBox.ShowError($"Ocorreu um erro durante o processamento:\n\n{ex.Message}",
                        "Erro", this);
                }
                finally
                {
                    ConfigurarBotoesProcessamento(false);
                    cts?.Dispose();
                    cts = null;
                }
            }
        }

        private void btnParar_Click(object sender, EventArgs e)
        {
            try
            {
                if (cts != null && !cts.Token.IsCancellationRequested)
                {
                    var result = CiviliumMessageBox.ShowQuestion("Deseja realmente parar o processo de consulta?",
                        "Confirmação", this);

                    if (result == DialogResult.Yes)
                    {
                        cts.Cancel();
                        Logger.LogInformation("Cancelamento do processo solicitado pelo usuário");

                        // Feedback visual imediato
                        btnParar.Text = "⏳ Parando...";
                        btnParar.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Erro ao tentar parar o processo", ex);
            }
        }

        private async void btnExportar_Click(object sender, EventArgs e)
        {
            if (tabelaDados == null || tabelaDados.Rows.Count == 0)
            {
                CiviliumMessageBox.ShowWarning("Não há dados para exportar. Execute uma pesquisa primeiro.",
                    "Aviso", this);

                return;
            }

            try
            {
                SaveFileDialog salvar = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv",
                    DefaultExt = "xlsx",
                    FileName = $"Consulta_CPF_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (salvar.ShowDialog() == DialogResult.OK)
                {
                    // Feedback visual durante exportação
                    btnExportar.Text = "⏳ Exportando...";
                    btnExportar.TextAlign = ContentAlignment.MiddleCenter;
                    btnExportar.Enabled = false;

                    await Task.Run(() => ArquivoService.ExportarParaExcel(tabelaDados, salvar.FileName));

                    CiviliumMessageBox.ShowInfo($"Arquivo exportado com sucesso!\n\nLocal: {salvar.FileName}",
                        "Sucesso", this);

                    Logger.LogInformation($"Dados exportados para: {salvar.FileName}");

                    // Perguntar se deseja abrir o arquivo
                    var resultado = CiviliumMessageBox.ShowQuestion("Deseja abrir o arquivo exportado?",
                        "Abrir Arquivo", this);

                    if (resultado == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(salvar.FileName) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Erro durante exportação", ex);

                CiviliumMessageBox.ShowError($"Erro ao exportar arquivo:\n\n{ex.Message}",
                    "Erro", this);
            }
            finally
            {
                btnExportar.Text = "📊 Exportar Pesquisa";
                btnExportar.TextAlign = ContentAlignment.MiddleCenter;
                btnExportar.Enabled = true;

                AplicarTemaNosBotoesPrincipais();
            }
        }
        #endregion

        #region Processamento de Consultas
        private void ProcessarLinhaConsulta(ChromeDriver driver, DataRow linha)
        {
            string nome = linha["NOME"].ToString();
            string cpf = ArquivoService.FormatarCPF(linha["CPF"].ToString());
            string dataNasc = ArquivoService.FormatarData(linha["DATA DE NASCIMENTO"].ToString());

            try
            {
                Logger.LogDebug($"Processando CPF: {cpf.MascararCPF()}");

                var (nomeReceita, situacao) = ConsultaService.ConsultarCPF(driver, cpf, dataNasc);

                linha["NOME NA RECEITA"] = nomeReceita;
                linha["SITUAÇÃO NA RECEITA"] = situacao;
                linha["STATUS"] = (nome.ToUpper().Trim() == nomeReceita.ToUpper().Trim()) ? "CONFERE" : "NOME DIVERGENTE";

                Logger.LogDebug($"CPF {cpf.MascararCPF()} processado com sucesso");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Erro ao processar CPF {cpf.MascararCPF()}", ex);
                linha["NOME NA RECEITA"] = "ERRO";
                linha["SITUAÇÃO NA RECEITA"] = ex.Message.Truncar(100);
                linha["STATUS"] = "ERRO";
            }
        }

        private async Task ProcessarConsultasAsync(CancellationToken cancellationToken)
        {
            int total = tabelaDados.Rows.Count;
            int atual = 0;
            int sucessos = 0;
            int erros = 0;
            Stopwatch cronometro = Stopwatch.StartNew();

            try
            {
                if (ConsultaService.NovaInstanciaChromePorConsulta)
                {
                    await Task.Run(() =>
                    {
                        foreach (DataRow linha in tabelaDados.Rows)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                Logger.LogInformation($"Processo cancelado. Processados: {atual}/{total}");
                                break;
                            }

                            try
                            {
                                using (var driver = ConsultaService.CriarDriver())
                                    ProcessarLinhaConsulta(driver, linha);
                                sucessos++;
                            }
                            catch
                            {
                                erros++;
                            }
                            finally
                            {
                                atual++;
                                AtualizarInterfaceProgresso(atual, total, cronometro, sucessos, erros);
                            }

                            Thread.Sleep(Random.Shared.Next(400, 1100));
                        }
                    }, cancellationToken);
                }
                else
                {
                    using (var driver = ConsultaService.CriarDriver())
                    {
                        await Task.Run(() =>
                        {
                            foreach (DataRow linha in tabelaDados.Rows)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    Logger.LogInformation($"Processo cancelado. Processados: {atual}/{total}");
                                    break;
                                }

                                try
                                {
                                    ProcessarLinhaConsulta(driver, linha);
                                    sucessos++;
                                }
                                catch
                                {
                                    erros++;
                                }
                                finally
                                {
                                    atual++;
                                    AtualizarInterfaceProgresso(atual, total, cronometro, sucessos, erros);
                                }

                                Thread.Sleep(100);
                            }
                        }, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Erro durante o processamento: {ex.Message}", ex);
            }
            finally
            {
                cronometro.Stop();
                Logger.LogInformation($"Processamento finalizado. Total: {atual}, Sucessos: {sucessos}, Erros: {erros}");

                // Resetar interface
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        ConfigurarBotoesProcessamento(false);
                        if (atual > 0)
                        {
                            lblStatus.Text = $"Concluído! {sucessos} sucessos, {erros} erros de {atual} processados";
                            lblPercentual.Text = "100%";
                            progressBar1.Value = 100;
                            lblTempo.Text = $"Tempo total: {cronometro.Elapsed:mm\\:ss}";
                        }
                    }));
                }
            }
        }

        private void AtualizarInterfaceProgresso(int atual, int total, Stopwatch cronometro, int sucessos, int erros)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int, int, Stopwatch, int, int>(AtualizarInterfaceProgresso),
                    atual, total, cronometro, sucessos, erros);
                return;
            }

            // Atualizar barra de progresso
            int percentual = (int)((double)atual / total * 100);
            progressBar1.Value = Math.Min(percentual, 100);

            // Atualizar labels com informações detalhadas
            lblStatus.Text = $"Consultando {atual} de {total} | ✓{sucessos} ✗{erros}";
            lblPercentual.Text = $"{percentual}%";

            // Calcular e exibir tempo estimado
            if (atual > 0)
            {
                double tempoPassado = cronometro.Elapsed.TotalSeconds;
                double estimado = (tempoPassado / atual) * (total - atual);
                lblTempo.Text = $"Restante: {TimeSpan.FromSeconds(estimado):mm\\:ss} | Decorrido: {cronometro.Elapsed:mm\\:ss}";
            }

            // Atualizar título da janela com progresso
            this.Text = $"Pesquisa de Informações - {percentual}% ({atual}/{total})";
        }
        #endregion

        #region Sistema de Temas e Botões
        private void AplicarTemaNosBotoesPrincipais()
        {
            // Cores base do tema
            Color corIniciar = Color.FromArgb(97, 136, 197);
            Color corParar = temaEscuro ? Color.FromArgb(255, 192, 192) : Color.FromArgb(255, 192, 192);
            Color corExportar = temaEscuro ? Color.FromArgb(52, 199, 89) : Color.FromArgb(52, 199, 89);

            // Cores de hover
            Color corIniciarHover = Color.FromArgb(0, 100, 220);
            Color corPararHover = temaEscuro ? Color.FromArgb(230, 50, 40) : Color.FromArgb(200, 40, 50);
            Color corExportarHover = temaEscuro ? Color.FromArgb(40, 180, 75) : Color.FromArgb(30, 140, 55);

            // Cores de click
            Color corIniciarClick = Color.FromArgb(0, 80, 180);
            Color corPararClick = temaEscuro ? Color.FromArgb(200, 40, 30) : Color.FromArgb(180, 30, 40);
            Color corExportarClick = temaEscuro ? Color.FromArgb(30, 160, 65) : Color.FromArgb(25, 120, 45);

            // Aplicar configurações
            ConfigurarBotaoTema(btnIniciar, corIniciar, corIniciarHover, corIniciarClick);
            ConfigurarBotaoTema(btnParar, corParar, corPararHover, corPararClick);
            ConfigurarBotaoTema(btnExportar, corExportar, corExportarHover, corExportarClick);
        }

        private void ConfigurarBotaoTema(Button botao, Color corBase, Color corHover, Color corClick)
        {
            botao.BackColor = corBase;
            botao.ForeColor = Color.White;
            botao.FlatStyle = FlatStyle.Flat;
            botao.FlatAppearance.BorderSize = 0;
            botao.FlatAppearance.MouseOverBackColor = corHover;
            botao.FlatAppearance.MouseDownBackColor = corClick;
            botao.Cursor = Cursors.Hand;
            botao.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            botao.Height = 40;
            botao.TextAlign = ContentAlignment.MiddleCenter;

            if (temaEscuro)
            {
                botao.FlatAppearance.BorderSize = 1;
                botao.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            }
        }

        private void ConfigurarBotoesProcessamento(bool emProcessamento)
        {
            btnIniciar.Enabled = !emProcessamento;
            btnParar.Enabled = emProcessamento;
            btnExportar.Enabled = !emProcessamento && tabelaDados != null;

            if (emProcessamento)
            {
                // Estado de processamento
                btnIniciar.BackColor = temaEscuro ? Color.FromArgb(80, 80, 80) : Color.FromArgb(180, 180, 180);
                btnIniciar.ForeColor = temaEscuro ? Color.Gray : Color.DarkGray;
                btnIniciar.Text = "⏳ Processando...";
                btnIniciar.Cursor = Cursors.No;

                btnParar.BackColor = Color.FromArgb(255, 69, 58);
                btnParar.Text = "⏹ Parar Pesquisa";
                btnParar.Cursor = Cursors.Hand;

                btnExportar.BackColor = temaEscuro ? Color.FromArgb(80, 80, 80) : Color.FromArgb(180, 180, 180);
                btnExportar.ForeColor = temaEscuro ? Color.Gray : Color.DarkGray;
                btnExportar.Cursor = Cursors.No;
            }
            else
            {
                // Estado normal
                AplicarTemaNosBotoesPrincipais();
                btnIniciar.Text = "▶ Iniciar Pesquisa";
                btnParar.Text = "⏸ Parar Pesquisa";
                btnExportar.Text = "📊 Exportar Pesquisa";

                // Resetar título da janela
                this.Text = "Pesquisa de Informações";

                if (tabelaDados == null)
                {
                    btnExportar.BackColor = temaEscuro ? Color.FromArgb(80, 80, 80) : Color.FromArgb(180, 180, 180);
                    btnExportar.ForeColor = temaEscuro ? Color.Gray : Color.DarkGray;
                    btnExportar.Cursor = Cursors.No;
                }
            }
        }

        private void ConfigurarTooltipsBotoesPrincipais()
        {
            toolTipBotoesPrincipais.SetToolTip(btnIniciar,
                "Inicia a consulta na Receita: carregue um CSV ou insira os dados manualmente");
            toolTipBotoesPrincipais.SetToolTip(btnParar, "Para o processo de consulta em andamento");
            toolTipBotoesPrincipais.SetToolTip(btnExportar, "Exporta os resultados da pesquisa para arquivo Excel");

            toolTipBotoesPrincipais.InitialDelay = 500;
            toolTipBotoesPrincipais.ReshowDelay = 100;
            toolTipBotoesPrincipais.AutoPopDelay = 5000;
        }

        private void AplicarTema(bool escuro)
        {
            temaEscuro = escuro;

            Color fundo = escuro ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
            Color texto = escuro ? Color.WhiteSmoke : Color.Black;
            Color controle = escuro ? Color.FromArgb(45, 45, 48) : SystemColors.ControlLight;

            this.BackColor = fundo;

            foreach (Control ctrl in this.Controls)
                AplicarTemaNosControles(ctrl, controle, texto);

            AplicarTemaNosBotoesPrincipais();
            AplicarTemaLabelsStatus();
            AplicarTemaProgressBar();

            if (MenuLateral != null)
            {
                MenuLateral.BackColor = escuro ? Color.FromArgb(45, 45, 48) : Color.WhiteSmoke;
                foreach (var btn in MenuLateral.Controls.OfType<Button>())
                {
                    btn.ForeColor = escuro ? Color.White : Color.Black;
                    ConfigurarTemaMenuBotao(btn);
                    AtualizarBotao(btn);
                }
            }

            CustomTitleBar.AplicarTemaBarraTitulo(this, escuro);

            Properties.Settings.Default.TemaEscuro = escuro;
            Properties.Settings.Default.Save();

            Logger.LogInformation($"Tema alterado para: {(escuro ? "Escuro" : "Claro")}");
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(value);

            // Aplicar tema da barra de título quando a janela for mostrada
            if (value && this.WindowState != FormWindowState.Minimized)
            {
                try
                {
                    CustomTitleBar.AplicarTemaBarraTitulo(this, temaEscuro);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"Erro ao aplicar tema da barra ao mostrar janela: {ex.Message}");
                }
            }
        }

        private void AplicarTemaLabelsStatus()
        {
            Color corTexto = temaEscuro ? Color.WhiteSmoke : Color.Black;
            Color corDestaque = temaEscuro ? Color.FromArgb(100, 149, 237) : Color.FromArgb(0, 122, 255);

            lblStatus.ForeColor = corTexto;
            lblPercentual.ForeColor = corDestaque;
            lblTempo.ForeColor = corTexto;

            lblStatus.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular);
            lblPercentual.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTempo.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular);
        }

        private void AplicarTemaProgressBar()
        {
            progressBar1.BackColor = temaEscuro ? Color.FromArgb(60, 60, 60) : Color.FromArgb(240, 240, 240);
            progressBar1.ForeColor = temaEscuro ? Color.FromArgb(0, 122, 255) : Color.FromArgb(0, 122, 255);
            progressBar1.Height = 30;
            progressBar1.Style = ProgressBarStyle.Continuous;
        }

        private void AplicarTemaNosControles(Control controle, Color fundo, Color texto)
        {
            controle.BackColor = fundo;
            controle.ForeColor = texto;

            foreach (Control filho in controle.Controls)
                AplicarTemaNosControles(filho, fundo, texto);
        }
        #endregion

        #region Menu Lateral
        private void CriarMenuLateral()
        {
            _menuLateral = new Panel();
            _menuLateral.Name = "menuLateral";
            _menuLateral.BackColor = temaEscuro ? Color.FromArgb(45, 45, 48) : Color.WhiteSmoke;
            _menuLateral.Dock = DockStyle.Left;
            _menuLateral.Width = menuExpandido ? 200 : 60;
            _menuLateral.Margin = new Padding(0);
            _menuLateral.Padding = new Padding(0);

            Controls.Add(_menuLateral);
            Controls.SetChildIndex(_menuLateral, 0);

            btnToggleMenu = CriarBotaoMenu("toggle", "Configurações", ToggleMenu_Click);
            Button btnTema = CriarBotaoMenu("tema", "Tema do app", BtnTema_Click);
            Button btnTimeout = CriarBotaoMenu("timeout", "Tempo de resolução", btnTimeout_Click);
            Button btnContato = CriarBotaoMenu("contato", "Sobre Civilium®", BtnContato_Click);
            btnTimeout.Name = "btnTimeout";

            // Adicionar os botões na ordem inversa (já que estão usando Dock.Top)
            _menuLateral.Controls.Add(btnContato);
            _menuLateral.Controls.Add(btnTimeout);
            _menuLateral.Controls.Add(btnTema);
            _menuLateral.Controls.Add(btnToggleMenu);

            // Aplicar configurações de tema em todos os botões
            foreach (var btn in _menuLateral.Controls.OfType<Button>())
            {
                ConfigurarTemaMenuBotao(btn);
                AtualizarBotao(btn);
            }
        }

        private Button CriarBotaoMenu(string nomeIcone, string texto, EventHandler onClick)
        {
            Button btn = new Button();
            btn.Height = 60;
            btn.Dock = DockStyle.Top;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = temaEscuro ? Color.White : Color.Black;
            btn.ImageAlign = ContentAlignment.MiddleLeft;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(10, 10, 10, 10);
            btn.Tag = new string[] { nomeIcone, texto };
            btn.Click += onClick;
            btn.Cursor = Cursors.Hand;

            // Configurar cores do tema para os botões do menu
            ConfigurarTemaMenuBotao(btn);

            AtualizarBotao(btn);
            return btn;
        }

        private void ConfigurarTooltipsMenu()
        {
            foreach (var btn in MenuLateral?.Controls.OfType<Button>() ?? Enumerable.Empty<Button>())
            {
                if (btn.Tag is string[] tag)
                {
                    toolTipMenu.SetToolTip(btn, tag[1]);
                }
            }
        }

        private void ConfigurarTemaMenuBotao(Button botao)
        {
            // Cor de fundo igual ao menu lateral
            Color corFundoMenu = temaEscuro ? Color.FromArgb(45, 45, 48) : Color.WhiteSmoke;
            Color corHover = temaEscuro ? Color.FromArgb(65, 65, 68) : Color.FromArgb(235, 235, 235);
            Color corClick = temaEscuro ? Color.FromArgb(85, 85, 88) : Color.FromArgb(220, 220, 220);

            botao.BackColor = corFundoMenu;
            botao.FlatAppearance.MouseOverBackColor = corHover;
            botao.FlatAppearance.MouseDownBackColor = corClick;
            botao.FlatAppearance.BorderSize = 0;

            // Adicionar evento para manter a cor de fundo quando não há hover
            botao.MouseLeave += (s, e) =>
            {
                botao.BackColor = corFundoMenu;
            };
        }

        private void AtualizarBotao(Button btn)
        {
            if (btn.Tag is string[] tag)
            {
                string nomeIcone = tag[0];
                string texto = tag[1];
                string caminhoIcone = $"Resources/{nomeIcone}_{(temaEscuro ? "escuro" : "claro")}.png";

                btn.Image = File.Exists(caminhoIcone) ? Image.FromFile(caminhoIcone) : null;

                if (menuExpandido && nomeIcone != "toggle")
                {
                    btn.Text = $"   {texto}";
                    btn.TextAlign = ContentAlignment.MiddleLeft;
                    btn.TextImageRelation = TextImageRelation.ImageBeforeText;
                    btn.ImageAlign = ContentAlignment.MiddleLeft;
                    btn.Padding = new Padding(10, 10, 10, 10);
                }
                else
                {
                    btn.Text = "";
                    btn.TextImageRelation = TextImageRelation.Overlay;
                    btn.ImageAlign = ContentAlignment.MiddleCenter;
                    btn.Padding = new Padding(10, 10, 10, 10);
                }

                btn.Margin = new Padding(0); // Remover margem para melhor alinhamento

                // Reaplicar configurações de tema
                ConfigurarTemaMenuBotao(btn);
            }
        }

        private void ToggleMenu_Click(object sender, EventArgs e)
        {
            if (MenuLateral == null) return;

            menuExpandido = !menuExpandido;
            AjustarMenuVisual();

            foreach (var btn in MenuLateral.Controls.OfType<Button>())
                AtualizarBotao(btn);
        }

        private void AjustarMenuVisual()
        {
            Panel MenuLateral = Controls.Find("menuLateral", true).FirstOrDefault() as Panel;
            if (MenuLateral == null) return;

            MenuLateral.Width = menuExpandido ? 200 : 60;

            // Atualizar cor do menu baseado no tema atual
            MenuLateral.BackColor = temaEscuro ? Color.FromArgb(45, 45, 48) : Color.WhiteSmoke;

            // Atualizar todos os botões
            foreach (var btn in MenuLateral.Controls.OfType<Button>())
            {
                ConfigurarTemaMenuBotao(btn);
                AtualizarBotao(btn);
            }
        }

        private void BtnTema_Click(object sender, EventArgs e)
        {
            temaEscuro = !temaEscuro;
            AplicarTema(temaEscuro); // Este método já foi atualizado acima

            Panel menu = Controls.Find("menuLateral", true).FirstOrDefault() as Panel;
            if (menu != null)
            {
                menu.BackColor = temaEscuro ? Color.FromArgb(45, 45, 48) : Color.WhiteSmoke;
                foreach (var btn in menu.Controls.OfType<Button>())
                {
                    btn.ForeColor = temaEscuro ? Color.White : Color.Black;
                    ConfigurarTemaMenuBotao(btn);
                    AtualizarBotao(btn);
                }
            }
        }

        private void btnTimeout_Click(object sender, EventArgs e)
        {
            using (var timeoutForm = new TimeoutForm(temaEscuro, AppConfig.TimeoutConsulta))
            {
                var resultado = timeoutForm.ShowDialog(this);

                if (resultado == DialogResult.OK)
                {
                    try
                    {
                        AppConfig.TimeoutConsulta = timeoutForm.Timeout;
                        ConsultaService.SincronizarTimeoutsDoAppConfig();
                        CiviliumMessageBox.ShowInfo(
                            $"Tempo de espera da consulta (Receita) ajustado para {AppConfig.TimeoutConsulta} segundos.",
                            "Configuração Atualizada", this);
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        CiviliumMessageBox.ShowWarning(ex.Message, "Valor inválido", this);
                    }
                }
            }
        }

        private void BtnContato_Click(object sender, EventArgs e)
        {
            string mensagem = "Desenvolvido por\n" +
                            "Elevador Com® - Rob Gomez - 1228205\n" +
                            "Email: robgomez.sir@gmail.com - roberio.gomes@atento.com\n" +
                            "Cel: 71991452913\n\n" +
                            "Versão: 1.0.2\n" +
                            $"Data de compilação: {File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location):dd/MM/yyyy HH:mm}";

            CiviliumMessageBox.ShowInfo(mensagem, "Sobre o Civilium", this);
        }
        #endregion

        #region Cleanup Resources
        // Método para limpeza manual de recursos quando necessário
        private void LimparRecursos()
        {
            try
            {
                cts?.Cancel();
                cts?.Dispose();
                cts = null;

                toolTipMenu?.Dispose();
                toolTipBotoesPrincipais?.Dispose();

                Logger.LogInformation("Recursos limpos com sucesso");
            }
            catch (Exception ex)
            {
                Logger.LogError("Erro ao limpar recursos", ex);
            }
        }

        // Chamado quando o form está sendo fechado
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            LimparRecursos();
            base.OnFormClosed(e);
        }
        #endregion
    }

    internal static class WinAPI
    {
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SW_MINIMIZE = 6;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }

    public partial class SplashScreen : Form
    {
        public SplashScreen()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.Opacity = 0;
            this.Load += SplashScreen_Load;
        }

        private async void SplashScreen_Load(object sender, EventArgs e)
        {
            var logo = new PictureBox
            {
                Image = Image.FromFile("Civilium_splash.jpg"),
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill
            };

            Controls.Add(logo);

            for (double i = 0; i <= 1; i += 0.05)
            {
                this.Opacity = i;
                await Task.Delay(30);
            }

            await Task.Delay(1000);

            for (double i = 1; i >= 0; i -= 0.05)
            {
                this.Opacity = i;
                await Task.Delay(20);
            }

            this.Close();
        }
    }

    public static class CustomTitleBar
    {
        // APIs do Windows para personalização da barra de título
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // Constantes para DWMWINDOWATTRIBUTE
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_TEXT_COLOR = 36;

        /// <summary>
        /// Aplica o tema escuro ou claro na barra de título do Windows
        /// </summary>
        /// <param name="form">Form que terá a barra personalizada</param>
        /// <param name="temaEscuro">True para tema escuro, False para tema claro</param>
        public static void AplicarTemaBarraTitulo(Form form, bool temaEscuro)
        {
            try
            {
                // Verificar se é Windows 10 ou superior
                if (Environment.OSVersion.Version.Major >= 10)
                {
                    // Definir preferência do tema (1 = escuro, 0 = claro)
                    var preference = temaEscuro ? 1 : 0;

                    // Tentar primeiro a versão mais recente da API (Windows 10 20H1+)
                    var result = DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref preference, sizeof(int));

                    // Se falhar, tentar a versão anterior (Windows 10 versões mais antigas)
                    if (result != 0)
                    {
                        DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref preference, sizeof(int));
                    }

                    // Para Windows 11 (Build 22000+) - Cores personalizadas da barra de título
                    if (Environment.OSVersion.Version.Build >= 22000)
                    {
                        try
                        {
                            // Definir cor da barra de título
                            int corBarraTitulo = temaEscuro ?
                                ColorToWin32(Color.FromArgb(45, 45, 48)) :
                                ColorToWin32(Color.FromArgb(240, 240, 240));

                            int corTextoTitulo = temaEscuro ?
                                ColorToWin32(Color.White) :
                                ColorToWin32(Color.Black);

                            DwmSetWindowAttribute(form.Handle, DWMWA_CAPTION_COLOR, ref corBarraTitulo, sizeof(int));
                            DwmSetWindowAttribute(form.Handle, DWMWA_TEXT_COLOR, ref corTextoTitulo, sizeof(int));
                        }
                        catch
                        {
                            // Se falhar as cores personalizadas, pelo menos o modo escuro/claro funcionará
                        }
                    }

                    Logger.LogInformation($"Tema da barra de título aplicado: {(temaEscuro ? "Escuro" : "Claro")}");
                }
                else
                {
                    Logger.LogWarning("Personalização da barra de título não suportada nesta versão do Windows");
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Erro ao aplicar tema na barra de título: {ex.Message}");
            }
        }

        /// <summary>
        /// Converte Color para formato Win32 (BGR)
        /// </summary>
        private static int ColorToWin32(Color color)
        {
            return color.R | (color.G << 8) | (color.B << 16);
        }
    }
}
