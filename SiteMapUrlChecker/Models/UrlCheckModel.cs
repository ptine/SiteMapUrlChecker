using SiteMapUrlChecker.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteMapUrlChecker.Models
{
    public class UrlCheckModel : BaseModel
    {
        private string _url;
        private int _statusCode;
        private string _statusDescription;

        public string Url
        {
            get => _url; 
            set
            {
                _url = value;
                OnPropertyChanged("Url");
            }
        }

        public int StatusCode
        {
            get => _statusCode; 
            set
            {
                _statusCode = value;
                OnPropertyChanged("StatusCode");
            }
        }

        public string StatusDescription
        {
            get => _statusDescription; 
            set
            {
                _statusDescription = value;
                OnPropertyChanged("StatusDescription");
            }
        }
    }
}
