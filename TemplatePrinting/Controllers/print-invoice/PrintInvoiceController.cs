using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TemplatePrinting.Models.Invoice;
using PrintingLibrary.Setup;
using PrintingLibrary.EmbeddedResourcesUtils;
using TemplatePrinting.Models;

namespace TemplatePrinting.Controllers;

[ApiController]
[Route("")]
public partial class PrintInvoiceController(
    ILogger<PrintInvoiceController> logger,
    IWebHostEnvironment hostEnvironment,
    IPrintingSetup util,
    Resources<Assets> resources
) : ControllerBase {
  private readonly ILogger<PrintInvoiceController> _logger = logger;
  private readonly IWebHostEnvironment _hostEnv = hostEnvironment;
  private readonly IPrintingSetup _util = util;
  private readonly Resources<Assets> _resources = resources;

  [HttpGet("", Name = "Index")]
  public IActionResult Index() {
    var filePath = Path.Combine(_hostEnv.WebRootPath, "printers/index.html");
    if (!System.IO.File.Exists(filePath)) return NotFound("Dashboard not found");
    return PhysicalFile(filePath, "text/html");
  }

  [HttpPost("", Name = "PrintInvoice")]
  [HttpPost("PrintingData", Name = "PostPrintingData")]
  public async Task<ActionResult> PrintInvoice([FromBody] Invoice invoice) {
    var settings = _util.Settings;

    if (!CanPrintInvoice(invoice, settings))
      return Ok("Receipt for pending invoice not printed");

    invoice = ProcessInvoicePrintingSettings(invoice, settings);

    try {
      // TODO: check if required printer exists otherwise it sends to default
      // TODO: clean up files after sending to printer
      // TODO: check if template && lib files exists

      if (settings.UseHtmlTemplate) await PrintInvoiceByHtml(invoice);
      else PrintInvoiceByExcel(invoice, settings.UseSpireExcelPrinter);

      // SaveAsJson(invoice);

      return Ok();

    } catch (Exception e) {
      _logger.LogInformation("exception: {Message}", e.Message);
      var err = $"message = {e.Message}, stack = {e.StackTrace}";
      return StatusCode(StatusCodes.Status500InternalServerError, err);
    }
  }

  private static void SaveAsJson(Invoice invoice) {
    var outputFile = GetOutputFilePath(invoice.Date, invoice.InvoiceNo, invoice.TemplateName, "json");
    var json = JsonConvert.SerializeObject(invoice, Formatting.Indented);

    System.IO.File.WriteAllText(outputFile, json);
  }

  private Assets GetPrintStampAsset(string? printerName) {
    var stampAsset = Assets.PrintStamp;
    if (string.IsNullOrEmpty(printerName)) return stampAsset;

    try {
      var printerSettings = new System.Drawing.Printing.PrinterSettings { PrinterName = printerName };
      if (!printerSettings.IsValid) return stampAsset;

      // width unit is in 1/1000 of an inch = 0.254mm
      // 1mm = 3.7795275591px
      var width = printerSettings.DefaultPageSettings.PaperSize.Width;
      // 80mm is ~315 units, 72mm is ~283 units. Using 290 as threshold.
      if (width > 0 && width <= 290) {
        stampAsset = Assets.PrintStamp72;
        _logger.LogInformation("Selected 72mm stamp for printer {PrinterName} (Width: {Width})", printerName, width);
      }
    } catch (Exception ex) {
      _logger.LogWarning(ex, "Failed to determine printer width for {PrinterName}, defaulting to 80mm stamp", printerName);
    }

    return stampAsset;
  }
}
