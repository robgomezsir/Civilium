using System;
using System.Configuration;
using Civilium.Properties;

namespace Civilium
{
    public static class AppConfig
    {
        private static Settings Settings => Settings.Default;

        public static int TimeoutCaptcha
        {
            get
            {
                try
                {
                    return Settings.TimeoutCaptcha;
                }
                catch (InvalidCastException)
                {
                    Logger.LogWarning("Formato inválido para TimeoutCaptcha, usando valor padrão (8)");
                    return 8;
                }
            }
            set
            {
                if (value < 5 || value > 120)
                    throw new ArgumentOutOfRangeException(nameof(value), "TimeoutCaptcha deve estar entre 5 e 120 segundos");

                Settings.TimeoutCaptcha = value;
                SaveSettings();
                Logger.LogInformation($"TimeoutCaptcha alterado para {value}s");
            }
        }

        public static int TimeoutConsulta
        {
            get
            {
                try
                {
                    return Settings.TimeoutConsulta;
                }
                catch (InvalidCastException)
                {
                    Logger.LogWarning("Formato inválido para TimeoutConsulta, usando valor padrão (30)");
                    return 30;
                }
            }
            set
            {
                if (value < 10 || value > 300)
                    throw new ArgumentOutOfRangeException(nameof(value), "TimeoutConsulta deve estar entre 10 e 300 segundos");

                Settings.TimeoutConsulta = value;
                SaveSettings();
                Logger.LogInformation($"TimeoutConsulta alterado para {value}s");
            }
        }

        public static bool TemaEscuro
        {
            get
            {
                try
                {
                    return Settings.TemaEscuro;
                }
                catch (InvalidCastException)
                {
                    Logger.LogWarning("Formato inválido para TemaEscuro, usando valor padrão (true)");
                    return true;
                }
            }
            set
            {
                Settings.TemaEscuro = value;
                SaveSettings();
                Logger.LogInformation($"Tema alterado para {(value ? "Escuro" : "Claro")}");
            }
        }

        public static int ChromeRemoteDebugPort
        {
            get
            {
                try
                {
                    return Settings.ChromeRemoteDebugPort;
                }
                catch (InvalidCastException)
                {
                    return 0;
                }
            }
            set
            {
                if (value < 0 || value > 65535)
                    throw new ArgumentOutOfRangeException(nameof(value), "Use 0 (desligado) ou uma porta entre 1 e 65535.");

                Settings.ChromeRemoteDebugPort = value;
                SaveSettings();
                Logger.LogInformation($"ChromeRemoteDebugPort alterado para {value}");
            }
        }

        public static void CarregarConfiguracoes()
        {
            try
            {
                Settings.Reload();
                ConfigValidator.ValidarConfiguracoes();
                Logger.LogInformation("Configurações carregadas com sucesso");
            }
            catch (Exception ex)
            {
                Logger.LogError("Erro ao carregar configurações", ex);
                DefinirValoresPadrao();
            }
        }

        internal static void DefinirValoresPadrao()
        {
            try
            {
                Settings.Reset();
                SaveSettings();
                Logger.LogInformation("Configurações redefinidas para valores padrão");
            }
            catch (Exception ex)
            {
                Logger.LogError("Erro ao redefinir configurações", ex);
            }
        }

        private static void SaveSettings()
        {
            try
            {
                Settings.Save();
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.LogError("Erro ao salvar configurações", ex);
                throw new ApplicationException("Falha ao salvar configurações. Verifique as permissões do arquivo de configurações.", ex);
            }
        }
    }

    public static class ConfigValidator
    {
        public static void ValidarConfiguracoes()
        {
            try
            {
                // Validação de TimeoutCaptcha
                if (Settings.Default.TimeoutCaptcha < 5 || Settings.Default.TimeoutCaptcha > 120)
                {
                    Settings.Default.TimeoutCaptcha = 8;
                    Logger.LogWarning("TimeoutCaptcha inválido. Redefinido para valor padrão (8s)");
                }

                // Validação de TimeoutConsulta
                if (Settings.Default.TimeoutConsulta < 10 || Settings.Default.TimeoutConsulta > 300)
                {
                    Settings.Default.TimeoutConsulta = 30;
                    Logger.LogWarning("TimeoutConsulta inválido. Redefinido para valor padrão (30s)");
                }

                if (Settings.Default.ChromeRemoteDebugPort < 0 || Settings.Default.ChromeRemoteDebugPort > 65535)
                {
                    Settings.Default.ChromeRemoteDebugPort = 0;
                    Logger.LogWarning("ChromeRemoteDebugPort inválido. Redefinido para 0 (desligado).");
                }

                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Logger.LogError("Falha na validação das configurações", ex);
                AppConfig.DefinirValoresPadrao();
            }
        }
    }
}