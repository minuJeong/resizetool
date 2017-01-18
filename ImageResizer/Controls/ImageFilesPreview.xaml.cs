using System;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using ImageResizer.Core;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using ImageResizer.Controls.Tools;
using System.Windows.Input;

namespace ImageResizer.Controls
{
    /// <summary>
    /// Interaction logic for ImageFilesPreview.xaml
    /// 
    /// Usage
    ///   Dispatch UPDATE_SELECTED_DIRECTORY Signal
    ///   Signal.Dispatch(SignalKey.UPDATE_SELECTED_DIRECTORY, {DirectoryInfo});
    /// 
    /// Purpose
    ///   This class is intended to be treated as preview purpose only.
    ///   Don't use any data stored in instance of this class.
    /// </summary>
    public partial class ImageFilesPreview : UserControl
    {
        private const int MAX_FILECOUNT = 30;

        private DirectoryInfo m_RefDir;
        private FileItem m_SelectedItem;

        /// <summary>
        /// Constructor
        /// </summary>
        public ImageFilesPreview()
        {
            InitializeComponent();
            Signal.Add(SignalKey.UPDATE_SELECTED_DIRECTORY, OnUpdateSelectedDirectory);
        }

        /// <summary>
        /// Called by Signal - update selected directory
        /// </summary>
        /// <param name="obj"></param>
        internal void OnUpdateSelectedDirectory(object obj)
        {
            DirectoryInfo dirInfo = obj as DirectoryInfo;

            // Escape if casting failed
            if (dirInfo != null)
            {
                m_RefDir = dirInfo;
                RefreshFileIcons(dirInfo);
            }
            else
            {
                string dirPath = obj as string;
                if (dirPath != null)
                {
                    m_RefDir = new DirectoryInfo(dirPath);
                    RefreshFileIcons(dirInfo);
                }
                else
                {
                    throw new InvalidSignalDataException("INVALID SIGNAL DATA");
                }
            }
        }

        /// <summary>
        /// Clear existing icons and add new icons
        /// </summary>
        private async void RefreshFileIcons(DirectoryInfo dirInfo = null)
        {
            // Clear children
            m_Root.Items.Clear();

            // Assign dirInfo if not set
            if (dirInfo == null)
            {
                dirInfo = m_RefDir;
            }

            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    List<FileInfo> files = dirInfo.GetFiles().ToList();
                    int count = Math.Min(files.Count, MAX_FILECOUNT);
                    files.GetRange(0, count).ForEach((fileInfo) =>
                    {
                        m_Root.Items.Add(new FileItem(fileInfo));
                    });

                    if (files.Count > MAX_FILECOUNT)
                    {
                        m_Root.Items.Add(new IndicatorItem_TooManyFiles());
                    }
                });
            });
        }

        /// <summary>
        /// </summary>
        private void m_Root_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0)
            {
                m_SelectedItem = null;
                return;
            }

            m_SelectedItem = e.AddedItems[0] as FileItem;
            if (m_SelectedItem != null)
            {
                m_StatsBar_FileName.Text = m_SelectedItem.m_FileName;

                if (m_SelectedItem.m_IconRendered != null)
                {
                    m_StatusBar_PreviewImage.Source = m_SelectedItem.m_IconRendered.Source;
                }
                else
                {
                    m_StatusBar_PreviewImage.Source = null;
                }
            }
            else
            {
                m_StatusBar_PreviewImage.Source = null;
            }
        }

        /// <summary>
        /// </summary>
        private void m_Root_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            m_SelectedItem.BrowseInExplorer();
        }
    }
    
    /// <summary>
    /// Represent File Item in file preview
    /// </summary>
    internal class FileItem : Grid
    {
        // Synchronized properties
        private string _m_FileName;
        public string m_FileName
        {
            get
            {
                return _m_FileName;
            }
            set
            {
                _m_FileName = value;

                TextBlock fileName = new TextBlock();
                fileName.Text = _m_FileName;
                fileName.TextWrapping = TextWrapping.Wrap;
                fileName.TextAlignment = TextAlignment.Center;
                fileName.HorizontalAlignment = HorizontalAlignment.Center;
                Children.Add(fileName);

                SetColumn(fileName, 0);
                SetRow(fileName, 1);
            }
        }

        // Synchronized properties
        private FileInfo _m_RefFileInfo;
        public FileInfo m_RefFileInfo
        {
            get
            {
                return _m_RefFileInfo;
            }
            set
            {
                _m_RefFileInfo = value;
                if (_m_RefFileInfo != null)
                {
                    m_FileName = _m_RefFileInfo.Name;
                }
            }
        }

        public Image m_IconRendered;

        /// <summary>
        /// [TODO] Use Designer designed template
        /// ctor
        /// </summary>
        internal FileItem(FileInfo fileInfo)
        {
            m_RefFileInfo = fileInfo;

            RowDefinition rowDef_Icon = new RowDefinition();
            rowDef_Icon.Height = new GridLength(60.0);
            rowDef_Icon.SharedSizeGroup = "IconRow";
            RowDefinitions.Add(rowDef_Icon);

            RowDefinition rowDef_FileName = new RowDefinition();
            rowDef_FileName.Height = new GridLength(20.0);
            rowDef_FileName.SharedSizeGroup = "FileNameRow";
            RowDefinitions.Add(rowDef_FileName);

            ColumnDefinition columnDef = new ColumnDefinition();
            columnDef.Width = new GridLength(80.0);
            columnDef.SharedSizeGroup = "IconColumn";
            ColumnDefinitions.Add(columnDef);

            Init();
        }

        /// <summary>
        /// Wrapped initialize
        /// </summary>
        protected virtual void Init()
        {
            // Enqueue loading image
            if (ResizeTool.VALID_IMAGE_EXTENSIONS.Contains(m_RefFileInfo.Extension))
            {
                ImageFiller.Instance.Enqueue(this);
            }

            m_FileName = m_RefFileInfo.Name;
        }

        /// <summary>
        /// Utility
        /// </summary>
        public void BrowseInExplorer()
        {
            string path = string.Format("\"{0}\"", m_RefFileInfo.FullName);
            Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = string.Format("/select, {0}", path)
            });
        }
    }

    /// <summary>
    /// Prevent blocked by too many files
    /// </summary>
    internal class IndicatorItem_TooManyFiles : FileItem
    {
        public IndicatorItem_TooManyFiles() : base(null) { }

        protected override void Init()
        {
            m_FileName = "...And More";
        }
    }

    /// <summary>
    /// Load bitmap image and add it as Image component for FileItem asynchronously
    /// </summary>
    internal class ImageFiller
    {
        // Simplest Singleton
        private static ImageFiller _Instance;
        public static ImageFiller Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new ImageFiller();
                }
                return _Instance;
            }
        }

        // Simplest State Machine
        enum STATE
        {
            IDLE,
            RUNNING
        }
        private STATE m_RunningState = STATE.IDLE;

        // Thread-safe Queue
        private readonly Queue<FileItem> m_ItemsToFill = new Queue<FileItem>();

        /// <summary>
        /// Run if not running on enqueue
        /// </summary>
        public void Enqueue(FileItem item)
        {
            m_ItemsToFill.Enqueue(item);

            if (m_RunningState == STATE.IDLE)
            {
                Run();
            }
        }

        /// <summary>
        /// Batch fill in images for FileItems
        /// </summary>
        private async void Run()
        {
            m_RunningState = STATE.RUNNING;
            while (m_ItemsToFill.Count > 0)
            {
                await Task.Run(() =>
                {
                    Work(m_ItemsToFill.Dequeue());
                });
            }
            m_RunningState = STATE.IDLE;
        }

        /// <summary>
        /// Actual work that does fill in Image to FileItem
        /// </summary>
        private void Work(FileItem item)
        {
            // Try fetch image,
            try
            {
                // Load and assign image to item
                Application.Current.Dispatcher.Invoke(() =>
                {
                    item.m_IconRendered = new Image();

                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = new Uri(item.m_RefFileInfo.FullName);
                    image.EndInit();

                    item.m_IconRendered.Source = image;
                    item.m_IconRendered.HorizontalAlignment = HorizontalAlignment.Stretch;
                    item.m_IconRendered.VerticalAlignment = VerticalAlignment.Stretch;

                    item.Children.Add(item.m_IconRendered);
                    Grid.SetColumn(item.m_IconRendered, 0);
                    Grid.SetRow(item.m_IconRendered, 0);
                });
            }
            catch (FileFormatException ex)
            {
                // Escape wrong formatted files
                Debug.WriteLine(
                    string.Format("Error Loading {0} Because, {1}", item.m_FileName, ex.Message)
                );
                return;
            }
        }
    }
}
