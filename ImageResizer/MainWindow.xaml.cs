using ImageResizer.Core;
using System.Windows;

namespace ImageResizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal Tool m_Tool;
        internal static MainWindow Instance;

        public MainWindow()
        {
            Instance = this;

            InitializeComponent();
            m_Tool = Tool.Instance;
            m_Tool.Start();
        }

        /// <summary>
        /// Callback from tool bar button
        /// </summary>
        private void OnResizeButton_Click(object sender, RoutedEventArgs e)
        {
            Signal.Dispatch(SignalKey.TOOLS_RESIZE, m_DiskExplorer.m_SelectedDirectory);
        }

        /// <summary>
        /// Callback from tool bar button
        /// </summary>
        private void OnBuildButton_Click(object sender, RoutedEventArgs e)
        {
            Signal.Dispatch(SignalKey.TOOLS_DBBUILD, null);
        }
    }
}
