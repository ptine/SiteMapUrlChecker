using SiteMapUrlChecker.BaseClasses;
using SiteMapUrlChecker.Misc;
using SiteMapUrlChecker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;

namespace SiteMapUrlChecker.ViewModels
{
    public class MainWindowViewModel : BaseModel
    {
        #region private fields
        private CancellationTokenSource _cancellationToken;

        private string _sitemapFilePath;
        private string _siteRoot;
        private string _checkProgress;

        private ObservableCollection<UrlCheckModel> _urlCollection;

        protected ICommand _cmdLoadFile;
        protected ICommand _cmdCancel;
        #endregion

        #region public properties
        public string SitemapFilePath
        {
            get => _sitemapFilePath;
            set
            {
                _sitemapFilePath = value;
                OnPropertyChanged("SitemapFilePath");

            }
        }

        public string SiteRoot
        {
            get => _siteRoot;
            set
            {
                _siteRoot = value;
                OnPropertyChanged("SiteRoot");

            }
        }

        public string CheckProgress
        {
            get => _checkProgress;
            set
            {
                _checkProgress = value;
                OnPropertyChanged("CheckProgress");
            }
        }

        public ObservableCollection<UrlCheckModel> UrlCollection
        {
            get => _urlCollection;
            set
            {
                _urlCollection = value;
                OnPropertyChanged("UrlCollection");

            }
        }
        #endregion

        public MainWindowViewModel()
        {
            _cancellationToken = new CancellationTokenSource();
        }

        #region commands        
        public ICommand LoadFileCommand
        {
            get
            {
                if (_cmdLoadFile == null)
                {
                    _cmdLoadFile = new RelayCommand(LoadFileExecute, CanLoadFileExecute);
                }
                return _cmdLoadFile;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cmdCancel == null)
                {
                    _cmdCancel = new RelayCommand(CancelExecute, CancelFileExecute);
                }
                return _cmdCancel;
            }
        }
        #endregion

        #region private methods
        private bool CancelFileExecute(object obj)
        {
            return true;
        }

        private void CancelExecute(object obj)
        {
            if (_cancellationToken != null)
                _cancellationToken.Cancel();
        }

        private bool CanLoadFileExecute(object obj)
        {
            return true;
        }

        private async Task<UrlCheckModel> GetResponse(string url)
        {
            using (var client = new HttpClient())
            {
                
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                httpClientHandler.AllowAutoRedirect = false;
                httpClientHandler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url),  _cancellationToken.Token);

                var urlCheckItem = new UrlCheckModel()
                {
                    Url = url,
                    StatusCode = (int)response.StatusCode,
                    StatusDescription = response.StatusCode.ToString()
                };                

                return urlCheckItem;
            }
        }

        private async Task ProcessUrls(string[] urls)
        {
            try
            {
                var fileName = $"report_{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv";
                File.Create(fileName).Close();
                File.AppendAllText(fileName, $"Url,StatusCode,StatusDescription{Environment.NewLine}");

                UrlCollection = new ObservableCollection<UrlCheckModel>();

                CheckProgress = $"Url 0 di {urls.Length}";

                foreach (var line in urls)
                {
                    var url = "";
                    var ln = "";

                    if (line.Contains(","))
                        ln = line.Split(',')[0];
                    else if (line.Contains(";"))
                        ln = line.Split(';')[0];
                    else
                        ln = line;

                    if (!string.IsNullOrEmpty(_siteRoot))
                    {
                        url = $"{_siteRoot.TrimEnd('/').TrimEnd('\\')}/{ln.TrimStart('/').TrimEnd('\\')}";
                    }
                    else
                    {
                        url = line;
                    }


                    var urlCheckItem = await GetResponse(url);
                    UrlCollection.Add(urlCheckItem);

                    File.AppendAllText(fileName,
                        $"{urlCheckItem.Url},{urlCheckItem.StatusCode},{urlCheckItem.StatusDescription}{Environment.NewLine}");

                    CheckProgress = $"Url {UrlCollection.Count()} di {urls.Length}";

                    Thread.Sleep(200);
                }

            }
            catch (OperationCanceledException)
            {
                App.Msn.NotifyColleagues(App.MSGBOX_CANCEL);
            }
            catch (Exception ex)
            {

                App.Msn.NotifyColleagues(App.LOAD_ERROR, ex);
            }
        }

        private async Task LoadUrls()
        {

            string[] urls;

            if (System.IO.Path.GetExtension(_sitemapFilePath) == ".xml")
            {
                XmlDocument urldoc = new XmlDocument();
                urldoc.Load(_sitemapFilePath);

                XmlNodeList xnList = urldoc.GetElementsByTagName("url");

                var nodeList = new List<XmlNode>(urldoc.DocumentElement.GetElementsByTagName("url").OfType<XmlNode>());
                urls = nodeList.Select(x => x["loc"].InnerText).ToArray();
            }
            else
            {
                urls = System.IO.File.ReadAllLines(_sitemapFilePath);
            }

            await ProcessUrls(urls);


        }

        private async void LoadFileExecute(object obj)
        {

            App.Msn.NotifyColleagues(App.FILE_DIALOG);

            if (!string.IsNullOrEmpty(_sitemapFilePath))
            {


                await LoadUrls();
            }
        }
        #endregion

        #region public methods
        public async Task LoadUrls(string fileName)
        {
            _sitemapFilePath = fileName;
            await LoadUrls();
        }
        #endregion
    }
}
