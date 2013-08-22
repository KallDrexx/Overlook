using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace Overlook.Gui.ViewModels
{
    public class ServerConfigurationViewModel : ViewModelBase
    {
        private string _serverUrl;

        public string ServerUrl
        {
            get { return _serverUrl; }
            set { Set(() => ServerUrl, ref _serverUrl, value); }
        }
    }
}
