using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BauphysikToolWPF.UI.CustomControls
{
    /// <summary>
    /// Interaktionslogik für FileDropArea.xaml
    /// </summary>
    public partial class FileDropArea : UserControl
    {
        public ObservableCollection<string> FilePaths
        {
            get => (ObservableCollection<string>)GetValue(FilePathsProperty);
            set => SetValue(FilePathsProperty, value);
        }

        public static readonly DependencyProperty FilePathsProperty =
            DependencyProperty.Register(nameof(FilePaths), typeof(ObservableCollection<string>), typeof(FileDropArea), new FrameworkPropertyMetadata(new ObservableCollection<string>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public FileDropArea()
        {
            InitializeComponent();
        }

        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var file in files)
                {
                    FilePaths.Add(file);
                }

                // Manually trigger the property changed event
                var bindingExpression = BindingOperations.GetBindingExpression(this, FilePathsProperty);
                if (bindingExpression != null)
                {
                    SetValue(FilePathsProperty, FilePaths); // This will trigger the OnFilePathsChanged callback
                }
            }
        }
    }
}
