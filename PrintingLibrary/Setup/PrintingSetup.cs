using Newtonsoft.Json;

namespace PrintingLibrary.Setup;

public partial class PrintingSetup() : IPrintingSetup, IDisposable {

  // public string AssemblyPath = System.Reflection.Assembly.GetEntryAssembly()?.Location ?? Environment.CurrentDirectory;
  public static readonly string AssemblyPath = Environment.CurrentDirectory;

  public void Setup() {
    SpireUtils.SpireUtils.LoadSpireLicenseKey();
    if (!Settings?.UseHtmlTemplate ?? false) return;
    _ = PuppeteerUtils.PuppeteerUtils.Browser;
  }


  public async void Dispose() {
    await PuppeteerUtils.PuppeteerUtils.Browser.CloseAsync();

    GC.Collect();
    GC.SuppressFinalize(this);
  }


  public PrintingSettings Settings {
    get {
      if (!File.Exists("printing-settings.json"))
        return new PrintingSettings();

      try {
        PrintingSettings? settings;
        using (StreamReader r = new("printing-settings.json"))
          settings = JsonConvert.DeserializeObject<PrintingSettings>(r.ReadToEnd());

        return settings ?? new PrintingSettings();
      } catch (System.Exception) {
        return new PrintingSettings();
      }
    }
  }

  public string? PrintStampSecret {
    get { return null; }
  }
  public string? TemplateEncOutputSecret {
    get { return null; }
  }

}
