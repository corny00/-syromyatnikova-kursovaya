using GBDD.Models;
using System.Globalization;
using System.Windows;
using Xceed.Words.NET;

namespace GBDD
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }
        public App()
        {
            Licenser.LicenseKey = "WDN52-KWNUK-548M5-34SA";


            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("ru-RU");
        }
    }
}
