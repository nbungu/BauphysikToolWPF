using BauphysikToolWPF.Services.UI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using BauphysikToolWPF.Services.Application;

namespace BauphysikToolWPF.Models.UI
{
    public class NavigationContent : INotifyPropertyChanged // To reflect changes being made in a collection -> OnPropertyChanged(nameof(ParentPages)) not needed
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public NavigationPage Page { get; set; } // parent page of the NavigationMenu ListBoxItem
        public string PageName => NavigationManager.PageNameMapping.TryGetValue(Page, out var name) ? name : Page.ToString();
        public string Tooltip => NavigationManager.PageTooltipMapping.TryGetValue(Page, out var tooltip) ? tooltip : Page.ToString();
        public ImageSource? Icon
        {
            get
            {
                if (NavigationManager.PageIconMapping.TryGetValue(Page, out string? resourceKey))
                {
                    return NavigationManager.GetBitmapImageFromAppResources(resourceKey);
                }
                return null;
            }
        }

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

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public List<NavigationGroupContent>? PageGroups { get; set; }

        public NavigationContent() { } // default constructor for XAML binding
        
        public NavigationContent(NavigationPage page, bool isEnabled = true)
        {
            Page = page;
            IsEnabled = isEnabled;
        }
    }

    public class NavigationGroupContent : INotifyPropertyChanged // To reflect changes being made in a collection -> OnPropertyChanged(nameof(AvailableChildGroups)) not needed
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    
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
    }
}
