using Civilium;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

public static class ConsultaService
{
    // Configurações (espelho em memória; persistência via AppConfig)
    public static int TempoCaptcha { get; set; } = 8;
    public static int TempoConsulta { get; set; } = 30;

    /// <summary>
    /// Aplica timeouts salvos nas configurações do usuário ao Selenium.
    /// </summary>
    public static void SincronizarTimeoutsDoAppConfig()
    {
        TempoConsulta = AppConfig.TimeoutConsulta;
        TempoCaptcha = AppConfig.TimeoutCaptcha;
    }

    // Método público para criar driver (adicionado para resolver CS0117)
    public static ChromeDriver CriarDriver()
    {
        try
        {
            VerificarDependenciasSelenium();
            var options = ConfigurarChromeOptions();
            return new ChromeDriver(options);
        }
        catch (Exception ex)
        {
            Logger.LogError("Erro ao criar ChromeDriver", ex);
            throw new ApplicationException("Falha ao iniciar o navegador para consulta.", ex);
        }
    }

    public static (string nomeReceita, string situacao) ConsultarCPF(ChromeDriver driver, string cpf, string dataNasc)
    {
        try
        {
            Logger.LogInformation($"Iniciando consulta para CPF: {cpf.MascararCPF()}");

            NavegarParaPaginaConsulta(driver);
            PreencherDadosConsulta(driver, cpf, dataNasc);
            ResolverCaptcha(driver);
            SubmeterConsulta(driver);

            var resultado = ObterResultadoConsulta(driver);
            Logger.LogInformation($"Consulta concluída para CPF: {cpf.MascararCPF()} - Resultado: {resultado}");

            return resultado;
        }
        catch (NoSuchElementException ex)
        {
            Logger.LogWarning($"Elemento não encontrado na consulta do CPF {cpf.MascararCPF()} (possível mudança de layout na Receita)", ex);
            return ("NÃO LOCALIZADO", "PÁGINA ALTERADA OU ELEMENTO NÃO ENCONTRADO");
        }
        catch (WebDriverTimeoutException ex)
        {
            Logger.LogWarning($"Timeout na consulta do CPF {cpf.MascararCPF()}", ex);
            return ("NÃO LOCALIZADO", "TEMPO ESGOTADO PARA CONSULTA");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Erro geral na consulta do CPF {cpf.MascararCPF()}", ex);
            return ("NÃO LOCALIZADO", $"ERRO GERAL: {ex.Message.Truncar(100)}");
        }
    }

    private static void VerificarDependenciasSelenium()
    {
        string seleniumPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "selenium-manager.exe");
        FileUtils.VerificarArquivoEssencial(seleniumPath, "Selenium Manager");
        Environment.SetEnvironmentVariable("SE_MANAGER_PATH", seleniumPath);
    }

    private static ChromeOptions ConfigurarChromeOptions()
    {
        var options = new ChromeOptions();
        options.AddArguments(
            "--disable-blink-features=AutomationControlled",
            "--disable-dev-shm-usage",
            "--no-sandbox",
            "--disable-gpu",
            "--window-size=1024,768",
            "user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
        );
        options.AddExcludedArgument("enable-automation");
        options.AddAdditionalOption("useAutomationExtension", false);
        return options;
    }

    private static void NavegarParaPaginaConsulta(ChromeDriver driver)
    {
        driver.Navigate().GoToUrl("https://servicos.receita.fazenda.gov.br/Servicos/CPF/ConsultaSituacao/ConsultaPublica.asp");
    }

    private static void PreencherDadosConsulta(ChromeDriver driver, string cpf, string dataNasc)
    {
        var timeout = TimeSpan.FromSeconds(TempoConsulta);
        var cpfField = AguardarPrimeiroElementoVisivel(driver, timeout,
            By.Id("txtCPF"),
            By.Name("txtCPF"),
            By.CssSelector("input#txtCPF"),
            By.XPath("//input[contains(translate(@id,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'cpf') or contains(translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'cpf')]"));

        cpfField.Clear();
        cpfField.SendKeys(cpf);

        var dataField = AguardarPrimeiroElementoVisivel(driver, timeout,
            By.Id("txtDataNascimento"),
            By.Name("txtDataNascimento"),
            By.CssSelector("input#txtDataNascimento"),
            By.XPath("//input[contains(translate(@id,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'nasc') or contains(translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'nasc')]"));

        dataField.Clear();
        dataField.SendKeys(dataNasc);
    }

    private static void ResolverCaptcha(ChromeDriver driver)
    {
        try
        {
            var captchaFrame = LocalizarFrameCaptcha(driver);
            driver.SwitchTo().Frame(captchaFrame);

            var checkbox = AguardarPrimeiroElementoVisivel(driver, TimeSpan.FromSeconds(TempoCaptcha),
                By.Id("checkbox"),
                By.CssSelector("#checkbox"),
                By.CssSelector("div#checkbox"),
                By.CssSelector("[role='checkbox']"),
                By.CssSelector("iframe + * [type='checkbox']"));
            checkbox.Click();

            driver.SwitchTo().DefaultContent();
            Thread.Sleep(TempoCaptcha * 1000);
        }
        catch (WebDriverTimeoutException)
        {
            AjustarTempoCaptchaInterativamente();
            throw;
        }
    }

    private static IWebElement LocalizarFrameCaptcha(ChromeDriver driver)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TempoCaptcha));
        return wait.Until(d =>
        {
            var iframes = d.FindElements(By.TagName("iframe"));
            return iframes.FirstOrDefault(iframe =>
            {
                var src = iframe.GetAttribute("src") ?? "";
                return src.Length > 0 && (
                    src.Contains("hcaptcha", StringComparison.OrdinalIgnoreCase) ||
                    src.Contains("newassets.hcaptcha", StringComparison.OrdinalIgnoreCase));
            });
        });
    }

    private static void SubmeterConsulta(ChromeDriver driver)
    {
        var timeout = TimeSpan.FromSeconds(TempoConsulta);
        var enviar = AguardarPrimeiroElementoVisivel(driver, timeout,
            By.Id("id_submit"),
            By.Name("id_submit"),
            By.CssSelector("input#id_submit"),
            By.CssSelector("input[type='submit']"),
            By.CssSelector("button[type='submit']"),
            By.XPath("//input[@type='submit' or @type='image']"),
            By.XPath("//button[contains(translate(normalize-space(.),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'consultar')]"));

        enviar.Click();
    }

    private const string MsgLayoutReceitaDesconhecido = "LAYOUT DA RECEITA ALTERADO — ATUALIZE O CIVILIUM";

    private static (string, string) ObterResultadoConsulta(ChromeDriver driver)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TempoConsulta));
        try
        {
            wait.Until(ContemIndiciosPaginaResultado);
        }
        catch (WebDriverTimeoutException)
        {
            Logger.LogWarning("Timeout aguardando conteúdo de resultado após envio da consulta.");
            return ("NÃO LOCALIZADO", "RESULTADO NÃO CARREGOU NO TEMPO");
        }

        if (TryResultadoXPathLegado(driver, out var nome, out var situacao))
        {
            Logger.LogDebug("Resultado da consulta obtido pelo layout legado (span/b).");
            return (nome, situacao);
        }

        if (TryResultadoBoldEmContaineres(driver, out nome, out situacao))
        {
            Logger.LogDebug("Resultado da consulta obtido por agregação de <b> na área principal.");
            return (nome, situacao);
        }

        if (TryResultadoPorHtmlTabelas(driver, out nome, out situacao))
        {
            Logger.LogDebug("Resultado da consulta obtido por padrões de tabela no HTML.");
            return (nome, situacao);
        }

        var caminhoDiag = GravarDiagnosticoLayoutReceitaFalho(driver);
        if (caminhoDiag != null)
        {
            Logger.LogWarning(
                $"Não foi possível interpretar o HTML de resultado (título: {driver.Title}). " +
                $"Fragmento salvo em: {caminhoDiag}");
        }
        else
        {
            Logger.LogWarning(
                $"Não foi possível interpretar o HTML de resultado (título: {driver.Title}). Verifique se a Receita alterou a página.");
        }

        return ("NÃO LOCALIZADO", MsgLayoutReceitaDesconhecido);
    }

    private static IWebElement AguardarPrimeiroElementoVisivel(IWebDriver driver, TimeSpan timeout, params By[] seletores)
    {
        var wait = new WebDriverWait(driver, timeout);
        return wait.Until(d =>
        {
            foreach (var by in seletores)
            {
                var encontrado = TryFindElementoVisivel(d, by);
                if (encontrado != null)
                    return encontrado;
            }
            return null;
        });
    }

    private static IWebElement? TryFindElementoVisivel(ISearchContext contexto, By by)
    {
        try
        {
            var el = contexto.FindElement(by);
            return el.Displayed ? el : null;
        }
        catch (NoSuchElementException)
        {
            return null;
        }
        catch (StaleElementReferenceException)
        {
            return null;
        }
    }

    private static bool ContemIndiciosPaginaResultado(IWebDriver driver)
    {
        try
        {
            var blocosMain = driver.FindElements(By.XPath("//div[@id='mainComp' or contains(@id,'mainComp')]"));
            if (blocosMain.Any(e =>
            {
                try { return e.Displayed; }
                catch { return false; }
            }))
                return true;

            var corpo = driver.FindElement(By.TagName("body")).Text ?? "";
            if (corpo.Length < 40)
                return false;

            return corpo.Contains("Situação", StringComparison.OrdinalIgnoreCase)
                   || corpo.Contains("Cadastral", StringComparison.OrdinalIgnoreCase)
                   || (corpo.Contains("Nome", StringComparison.OrdinalIgnoreCase)
                       && corpo.Contains("CPF", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    private static bool TryResultadoXPathLegado(IWebDriver driver, out string nome, out string situacao)
    {
        nome = "";
        situacao = "";
        try
        {
            var bolds = driver.FindElements(By.XPath("//div[@id='mainComp']//b"));
            if (bolds.Count < 2)
                return false;

            nome = driver.FindElement(By.XPath("//div[@id='mainComp']//p/span[2]/b")).Text.Trim();
            situacao = driver.FindElement(By.XPath("//div[@id='mainComp']//p/span[4]/b")).Text.Trim();
            return nome.Length > 0 && situacao.Length > 0;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
        catch (StaleElementReferenceException)
        {
            return false;
        }
    }

    private static readonly string[] XpathsContainerResultado =
    {
        "//div[@id='mainComp']",
        "//div[contains(@id,'mainComp')]",
    };

    private static bool TryResultadoBoldEmContaineres(IWebDriver driver, out string nome, out string situacao)
    {
        nome = situacao = "";
        foreach (var cx in XpathsContainerResultado)
        {
            try
            {
                var textos = driver.FindElements(By.XPath($"{cx}//b"))
                    .Select(e => e.Text?.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => t!)
                    .Where(t => t.Length >= 2)
                    .Where(t => !EhRotuloCabecalhoCurto(t))
                    .ToList();

                if (textos.Count < 2)
                    continue;

                nome = textos[0];
                situacao = textos.Count > 2 ? textos[^1] : textos[1];
                if (nome.Length > 0 && situacao.Length > 0)
                    return true;
            }
            catch
            {
                // tenta próximo container
            }
        }
        return false;
    }

    private static bool EhRotuloCabecalhoCurto(string texto)
    {
        var u = texto.ToUpperInvariant().Trim();
        return u is "CPF" or "NOME" or "DADOS" or "ATENÇÃO" or "ATENCAO";
    }

    private static readonly Regex RxNomeTd = new(
        @"Nome[^\n<]{0,160}?</t[dh][^>]*>\s*<t[dh][^>]*>\s*([^<]{3,200}?)\s*</",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    private static readonly Regex RxSituacaoTd = new(
        @"(?:Situa[cç][aã]o|Situacao)\s+Cadastral[^\n<]{0,200}?</t[dh][^>]*>\s*<t[dh][^>]*>\s*([^<]{2,120}?)\s*</",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    private static readonly Regex RxNomeBold = new(
        @">Nome[^<]{0,80}</[^>]+>[^<]*<b>([^<]{3,200})</b>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex RxSituacaoBold = new(
        @">(?:Situa[cç][aã]o|Situacao)[^<]{0,80}Cadastral[^<]{0,40}</[^>]+>[^<]*<b>([^<]{2,120})</b>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static bool TryResultadoPorHtmlTabelas(IWebDriver driver, out string nome, out string situacao)
    {
        nome = situacao = "";
        try
        {
            var html = driver.PageSource ?? "";

            var mNome = RxNomeTd.Match(html);
            var mSit = RxSituacaoTd.Match(html);
            if (!mNome.Success || !mSit.Success)
            {
                mNome = RxNomeBold.Match(html);
                mSit = RxSituacaoBold.Match(html);
            }

            if (!mNome.Success || !mSit.Success)
                return false;

            nome = WebUtility.HtmlDecode(NormalizarEspacosHtml(mNome.Groups[1].Value));
            situacao = WebUtility.HtmlDecode(NormalizarEspacosHtml(mSit.Groups[1].Value));
            return nome.Length > 0 && situacao.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizarEspacosHtml(string valor)
    {
        return Regex.Replace(valor.Trim(), @"\s+", " ");
    }

    /// <summary>
    /// Grava trecho do HTML quando nenhuma estratégia de leitura do resultado funcionou (ajuste de seletores).
    /// </summary>
    /// <returns>Caminho do arquivo ou null se não foi possível gravar.</returns>
    private static string? GravarDiagnosticoLayoutReceitaFalho(IWebDriver driver)
    {
        try
        {
            var pastaLogs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(pastaLogs);

            var arquivo = Path.Combine(pastaLogs,
                $"receita-layout-falha-{DateTime.Now:yyyyMMdd-HHmmss-fff}.html");

            string url = "";
            string titulo = "";
            try { url = driver.Url ?? ""; }
            catch { /* ignorado */ }
            try { titulo = driver.Title ?? ""; }
            catch { /* ignorado */ }

            string htmlBruto;
            try { htmlBruto = driver.PageSource ?? ""; }
            catch (Exception ex)
            {
                Logger.LogWarning($"PageSource indisponível para diagnóstico: {ex.Message}");
                htmlBruto = "";
            }

            var fragmento = ExtrairFragmentoParaDiagnostico(htmlBruto);
            var cabecalho = new StringBuilder(512);
            cabecalho.Append("<!-- Civilium diagnóstico | ");
            cabecalho.Append(DateTime.Now.ToString("O"));
            cabecalho.Append(" | Título: ");
            cabecalho.Append(SanitizarTextoComentarioHtml(titulo));
            cabecalho.Append(" | URL: ");
            cabecalho.Append(SanitizarTextoComentarioHtml(url));
            cabecalho.AppendLine(" -->");
            cabecalho.AppendLine("<!-- Revise antes de compartilhar: pode conter dados pessoais da consulta. -->");

            File.WriteAllText(arquivo, cabecalho + fragmento, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            return arquivo;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Falha ao gravar arquivo de diagnóstico da Receita: {ex.Message}");
            return null;
        }
    }

    private static string SanitizarTextoComentarioHtml(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        return texto.Replace("--", "- -", StringComparison.Ordinal).Replace("\r", " ").Replace("\n", " ");
    }

    private static string ExtrairFragmentoParaDiagnostico(string html)
    {
        const int janela = 14000;
        const int maxSemMarcador = 160000;

        if (string.IsNullOrEmpty(html))
            return "<!-- PageSource vazio ou indisponível -->";

        ReadOnlySpan<string> marcadores =
        [
            "id=\"mainComp\"",
            "id='mainComp'",
            "mainComp",
            "Situação Cadastral",
            "Situacao Cadastral",
            "ConsultaPublica",
        ];

        int idx = -1;
        foreach (var m in marcadores)
        {
            idx = html.IndexOf(m, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0) break;
        }

        if (idx >= 0)
        {
            int metade = janela / 2;
            int inicio = Math.Max(0, idx - metade);
            int fim = Math.Min(html.Length, inicio + janela);
            if (fim - inicio < janela && inicio > 0)
                inicio = Math.Max(0, fim - janela);

            var trecho = html.Substring(inicio, fim - inicio);
            var sb = new StringBuilder(trecho.Length + 128);
            if (inicio > 0)
                sb.AppendLine("<!-- … trecho: início da página omitido … -->");
            sb.Append(trecho);
            if (fim < html.Length)
                sb.AppendLine().AppendLine("<!-- … trecho: restante da página omitido … -->");
            return sb.ToString();
        }

        if (html.Length <= maxSemMarcador)
            return html;

        return html.Substring(0, maxSemMarcador)
               + "\r\n<!-- TRUNCADO: HTML original com " + html.Length + " caracteres; nenhum marcador conhecido localizado. -->";
    }

    private static void AjustarTempoCaptchaInterativamente()
    {
        string input = Microsoft.VisualBasic.Interaction.InputBox(
            "Informe o novo tempo de espera (em segundos):",
            "Ajustar Tempo do Captcha",
            TempoCaptcha.ToString());

        if (!int.TryParse(input, out int novoTempo))
            return;

        if (novoTempo >= 5 && novoTempo <= 120)
        {
            TempoCaptcha = novoTempo;
            AppConfig.TimeoutCaptcha = novoTempo;
            CiviliumMessageBox.ShowInfo($"Tempo de captcha ajustado para {TempoCaptcha} segundos", "Configuração Atualizada");
        }
        else
        {
            CiviliumMessageBox.ShowWarning("Use um valor entre 5 e 120 segundos para o captcha.", "Valor inválido");
        }
    }
}

// Classe auxiliar para interação com usuário sobre o captcha
public static class CaptchaUI
{
    public static int? SolicitarNovoTimeout(int tempoAtual)
    {
        if (CiviliumMessageBox.ShowQuestion(
            $"O tempo de resposta do hCaptcha expirou.\nDeseja aumentar o tempo de espera padrão ({tempoAtual}s)?",
             "Tempo esgotado") != DialogResult.Yes)
        {
            return null;
        }

        string input = Microsoft.VisualBasic.Interaction.InputBox(
            "Informe o novo tempo de espera (em segundos):",
            "Ajustar Tempo do Captcha",
            tempoAtual.ToString());

        if (int.TryParse(input, out int novoTempo) && novoTempo > 0)
        {
            CiviliumMessageBox.ShowInfo($"Tempo de espera ajustado para {novoTempo} segundos.", "Ajuste Aplicado");
            return novoTempo;
        }

        return null;
    }
}