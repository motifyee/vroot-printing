
using PrintingLibrary.ExcelUtils;
using PrintingLibrary.InteropUtils;
using PrintingLibrary.SpireUtils;
using PrintingLibrary.Setup;
using TemplatePrinting.Models.Invoice;

namespace TemplatePrinting.Controllers;

public partial class PrintInvoiceController {

  private void PrintInvoiceByExcel(Invoice invoice, bool useSpirePrinter) {

    string templateFile = Path.Combine(
        PrintingSetup.AssemblyPath,
        "printer",
        "templates",
        "excel",
        $"{invoice!.TemplateName ?? ""}.xlsx"
    );

    string outputFile = GetOutputFilePath(invoice.Date, invoice.InvoiceNo, invoice.TemplateName);

    ExcelUtils.CreateOutputExcel(outputFile, templateFile, invoice);

    var printStampImageName = invoice.PrintStampImageName ?? _util.Settings.PrintStampImage;
    var printStampHash = invoice.PrintStampHash ?? _util.Settings.PrintStampHash;
    var printStampSecret = _util.PrintStampSecret;
    ExcelUtils.AddPrintStamp(outputFile, printStampImageName, printStampHash, printStampSecret);

    if (!_hostEnv.IsProduction()) return;

    if (useSpirePrinter)
      SpireUtils.PrintExcelFile(outputFile, invoice.PrinterName);
    else InteropUtils.PrintExcelFile(outputFile, invoice.PrinterName);
  }
}