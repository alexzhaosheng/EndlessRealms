using EndlessRealms.Core.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;

namespace EndlessRealms.WebUi.Pages
{
    public class LogModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _log = "";
        public string Log {
            get => _log;
            set
            {
                _log = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Log)));
            }
        }
    }
}
