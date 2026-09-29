using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace CineTrack
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += OnUnhandledException;
            base.OnStartup(e);
        }

        /// <summary>Writes unexpected errors to %LocalAppData%\CineTrack\error.log so they can be reported.</summary>
        static void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CineTrack");
                Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "error.log"), $"[{DateTime.Now:u}] {e.Exception}\n\n");
            }
            catch (IOException)
            {
            }
        }
    }
}
