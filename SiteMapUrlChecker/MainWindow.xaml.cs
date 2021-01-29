using Microsoft.Win32;
using SiteMapUrlChecker.ViewModels;
using System;
using System.Windows;

namespace SiteMapUrlChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            
            

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = new MainWindowViewModel();

            

            DataContext = viewModel;

            App.Msn.Register(App.FILE_DIALOG, new Action(() =>
            {
                //MessageBox.Show("Operazione annullata", "Caricamento file", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "File sitemap (*.xml, *.txt, *.csv) | *.xml;*.txt;*.csv";
                openFileDialog.ShowDialog();
                txtFile.Text = openFileDialog.FileName;

                

            }));

            App.Msn.Register(App.MSGBOX_CANCEL, new Action(() =>
            {

                MessageBox.Show("Operazione annullata", "Caricamento file", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            }));

            App.Msn.Register(App.LOAD_ERROR, new Action<Exception>((Exception ex) =>
            {

                MessageBox.Show(ex.ToString(), "Caricamento file", MessageBoxButton.OK, MessageBoxImage.Error);

            }));

        }
    }
}
