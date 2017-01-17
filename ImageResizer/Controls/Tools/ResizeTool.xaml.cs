using ImageProcessor;
using ImageProcessor.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;

namespace ImageResizer.Controls.Tools
{
    /// <summary>
    /// Interaction logic for ResizeTool.xaml
    /// </summary>
    public partial class ResizeTool : Window
    {
        public static readonly List<string> VALID_IMAGE_EXTENSIONS = new List<string>
        {
            ".png",
            ".jpg",
            // ".psd",
            // ".tga"
        };

        /// <summary>
        /// Keep m_RefDir and m_DirPath synced
        /// </summary>
        DirectoryInfo _m_RefDir;
        DirectoryInfo m_RefDir
        {
            get
            {
                return _m_RefDir;
            }
            set
            {
                _m_RefDir = value;
                m_PathName.Text = _m_RefDir.FullName;
            }
        }

        /// <summary>
        /// Keep m_RefDir and m_DirPath synced
        /// </summary>
        string m_DirPath
        {
            get
            {
                return m_RefDir.FullName;
            }

            set
            {
                Debug.Assert(Directory.Exists(value));
                m_RefDir = new DirectoryInfo(value);
            }
        }

        System.Drawing.Size m_TargetSize
        {
            get
            {
                int w = int.Parse(m_SizeInput_Width.Text);
                int h = int.Parse(m_SizeInput_Height.Text);
                return new System.Drawing.Size(w, h);
            }
        }

        /// <summary>
        /// Initialize here
        /// </summary>
        /// <param name="dirInfo"></param>
        public ResizeTool(DirectoryInfo dirInfo)
        {
            InitializeComponent();
            m_RefDir = dirInfo;
            SetStatusMessage("Initialized!");
        }

        /// <summary>
        /// Validate if text is numeric, by regex
        /// </summary>
        private bool IsNumeric(string text)
        {
            return !new Regex("[^0-9.-]+").IsMatch(text);
        }

        /// <summary>
        /// Disallow non-numeric text input
        ///  - Spacebar is not filtered here
        /// </summary>
        private void m_SizeInput_Any_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsNumeric(e.Text);
        }

        /// <summary>
        /// On copy-paste value to textbox
        /// </summary>
        private void PreventPaste(object sender, DataObjectPastingEventArgs e)
        {
            // Disallow non-string paste value
            // [should not happen]
            string inputText = (string)e.DataObject.GetData(typeof(string));
            if (inputText == null)
            {
                e.CancelCommand();
                return;
            }

            // Disallow non-numeric paste value
            if (!IsNumeric(inputText))
            {
                e.CancelCommand();
                return;
            }
        }

        /// <summary>
        /// Update message in status bar
        /// </summary>
        private void SetStatusMessage(string message)
        {
            m_StatusBar_StatusText.Text = message;
        }

        /// <summary>
        /// Initialize files list
        /// </summary>
        private void m_FilesView_Init(object sender, RoutedEventArgs e)
        {
            ListView castedSender = (ListView)sender;

            m_RefDir.GetFiles().ToList().ForEach((FileInfo fileInfo) =>
            {
                if (!VALID_IMAGE_EXTENSIONS.Contains(fileInfo.Extension))
                {
                    return;
                }

                castedSender.Items.Add(
                    new ListViewItem()
                    {
                        Content = fileInfo.Name
                    }
                );
            });
        }

        /// <summary>
        /// Callback from UI interaction
        /// </summary>
        private void m_ExitButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Callback from UI interaction
        /// </summary>
        private void m_ResizeAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            m_RefDir.GetFiles().ToList().ForEach((FileInfo fileInfo) =>
            {
                if (!File.Exists(fileInfo.FullName))
                {
                    return;
                }

                using (MemoryStream inStream = new MemoryStream(File.ReadAllBytes(fileInfo.FullName)))
                {
                    using (ImageFactory factory = new ImageFactory(preserveExifData: false))
                    {
                        try
                        {
                            factory.Load(inStream)
                                   .Resize(new ResizeLayer(m_TargetSize)
                                   {
                                       ResizeMode = ImageProcessor.Imaging.ResizeMode.Stretch,
                                       AnchorPosition = AnchorPosition.Center,
                                       Upscale = true
                                   })
                                   .Save(fileInfo.FullName);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(
                                string.Format("Resizing {0} Failed, Because: {1}", fileInfo.FullName, ex.Message));
                        }
                    }
                }
            });
        }
    }
}
