using BauphysikToolWPF.Services.Application;
using System.Collections.Generic;
using System.ComponentModel;

namespace BauphysikToolWPF.Models.Application
{
    public class NavigationGroupContent : INotifyPropertyChanged // To reflect changes being made in a collection -> OnPropertyChanged(nameof(AvailableChildGroups)) not needed
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string GroupHeader { get; set; } = string.Empty; // defines the header name of a NavigationMenu ListBoxItem
        public List<NavigationContent> ChildPages { get; set; } = new();

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        #region ctors

        public NavigationGroupContent() { } // default constructor for XAML binding

        public NavigationGroupContent(string groupHeader, List<NavigationPage> pages, bool isGroupEnabled = true)
        {
            GroupHeader = groupHeader;
            IsEnabled = isGroupEnabled;
            foreach (var page in pages)
            {
                ChildPages.Add(new NavigationContent(page, IsEnabled));
            }
        }

        #endregion
    }
}
