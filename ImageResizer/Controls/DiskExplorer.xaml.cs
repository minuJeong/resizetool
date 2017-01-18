using ImageResizer.Core;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImageResizer.Controls
{
    /// <summary>
    /// Interaction logic for DiskExplorer.xaml
    /// </summary>
    public partial class DiskExplorer : UserControl
    {
        public DirectoryInfo m_SelectedDirectory;

        public DiskExplorer()
        {
            InitializeComponent();
            Start();
        }

        void Start()
        {
            // For each drives,
            DriveInfo.GetDrives().ToList().ForEach((driveInfo) =>
            {
                // For each fixed drives, (excluding dvd, network, etc drives)
                if (driveInfo.DriveType != DriveType.Fixed)
                {
                    return;
                }

                m_Root.Items.Add(
                    new DirectoryItemModel(driveInfo.RootDirectory)
                );
            });

            Signal.Add(SignalKey.UPDATE_SELECTED_DIRECTORY, (object dirInfo) =>
            {
                m_SelectedDirectory = (DirectoryInfo) dirInfo;
            });
        }
    }

    internal class DirectoryItemModel : TreeViewItem
    {
        public DirectoryInfo m_RefDir;

        /// <summary>
        /// Constructor
        /// </summary>
        internal DirectoryItemModel(DirectoryInfo info)
        {
            m_RefDir = info;
            Header = info.Name;
            Selected += OnItem_Selected;

            Foreground = new SolidColorBrush(
                Color.FromScRgb(a: 1.0F, r: 0.82F, g: 0.82F, b: 0.82F)
            );
        }

        /// <summary>
        /// Lazy recursive call to prevent crash
        /// </summary>
        private void OnItem_Selected(object sender, RoutedEventArgs e)
        {
            // Dispatch signal
            Signal.Dispatch(SignalKey.UPDATE_SELECTED_DIRECTORY, m_RefDir);

            // Stop bubbling
            e.Handled = true;

            // Foreach directories under m_RefDir
            m_RefDir.GetDirectories().ToList().ForEach(dirInfo =>
            {
                // Escape duplicate item appension
                for (int i = Items.Count - 1; i >= 0; --i)
                {
                    if ((string) ((TreeViewItem) Items.GetItemAt(i)).Header == dirInfo.Name)
                    {
                        return;
                    }
                }

                // Append items
                Items.Add(new DirectoryItemModel(dirInfo));
            });
        }
    }
}
