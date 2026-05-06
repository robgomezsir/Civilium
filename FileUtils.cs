using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

 namespace Civilium
{
    public static class FileUtils
    {
        public static void VerificarArquivoEssencial(string caminhoRelativo, string descricaoArquivo)
        {
            string caminhoCompleto = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, caminhoRelativo);

            if (!File.Exists(caminhoCompleto))
            {
                string mensagem = $"O arquivo {descricaoArquivo} não foi encontrado em:\n{caminhoCompleto}";
                Logger.LogError(mensagem);
                throw new FileNotFoundException(mensagem, caminhoCompleto);
            }

            Logger.LogInformation($"Arquivo verificado: {caminhoRelativo}");
        }
    }
}
