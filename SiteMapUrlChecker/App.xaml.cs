using SiteMapUrlChecker.Misc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SiteMapUrlChecker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal const string LOAD_ERROR = "LoadingUrlsError";
        internal const string MSGBOX_CANCEL = "MessageBoxCancel";
        internal const string FILE_DIALOG = "OpenFileDialog";

        static readonly Messenger _messenger = new Messenger();

        static internal Messenger Msn => _messenger;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length == 1)
            {
                var vm = new ViewModels.MainWindowViewModel();
                vm.LoadUrls(e.Args[0]).Wait();
                App.Current.Shutdown();
            }
        }

    }
}
