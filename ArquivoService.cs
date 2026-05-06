using Civilium;
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using System.Linq;

public static class ArquivoService
{
    public static DataTable CarregarCSV(string caminho)
    {
        var dt = new DataTable();
        dt.Columns.Add("NOME");
        dt.Columns.Add("CPF");
        dt.Columns.Add("DATA DE NASCIMENTO");
        dt.Columns.Add("NOME NA RECEITA");
        dt.Columns.Add("SITUAÇÃO NA RECEITA");
        dt.Columns.Add("STATUS");

        foreach (var linha in File.ReadAllLines(caminho).Skip(1))
        {
            var partes = linha.Split(';');
            if (partes.Length >= 3)
                dt.Rows.Add(partes[0], partes[1], partes[2]);
        }

        return dt;
    }

    public static string FormatarCPF(string cpf)
    {
        return new string(cpf.Where(char.IsDigit).ToArray()).PadLeft(11, '0');
    }

    public static string FormatarData(string data)
    {
        if (string.IsNullOrWhiteSpace(data)) return "01011900";

        data = data.Trim()
                   .Replace("-", "/")
                   .Replace(".", "/")
                   .Replace("|", "/");

        // Remove espaços e força 2 dígitos
        var partes = data.Split('/');
        if (partes.Length != 3)
            return "01011900";

        string dia = partes[0].PadLeft(2, '0');
        string mes = partes[1].PadLeft(2, '0');
        string ano = partes[2].Length == 2 ? "20" + partes[2] : partes[2];

        string dataFormatada = $"{dia}/{mes}/{ano}";

        // Usa CultureInfo brasileira
        var culturaBR = new CultureInfo("pt-BR");

        if (DateTime.TryParseExact(dataFormatada, "dd/MM/yyyy", culturaBR, DateTimeStyles.None, out DateTime dtFinal))
        {
            return dtFinal.ToString("ddMMyyyy");
        }

        return "01011900";
    }

    public static void ExportarParaExcel(DataTable tabela, string caminho)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        try
        {
            if (File.Exists(caminho))
                File.Delete(caminho); // Remove arquivo existente antes de salvar o novo

            using var pacote = new ExcelPackage();
            var planilha = pacote.Workbook.Worksheets.Add("Resultado");
            planilha.Cells["A1"].LoadFromDataTable(tabela, true);
            pacote.SaveAs(new FileInfo(caminho));
        }
        catch (IOException)
        {
            CiviliumMessageBox.ShowError(
                "O arquivo está aberto em outro programa (provavelmente o Excel).\n\nFeche o arquivo e tente novamente.",
                "Erro ao Salvar");
        }
        catch (Exception ex)
        {
            CiviliumMessageBox.ShowError($"Erro inesperado ao exportar: {ex.Message}", "Erro");
        }
    }
}
