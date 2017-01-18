using ImageResizer.Controls.Tools;
using ImageResizer.Core;
using System.IO;

namespace ImageResizer
{
    /// <summary>
    /// Tool bar interactions
    /// </summary>
    internal class Tool
    {
        // Simplest Singleton
        private static Tool _Instance;
        public static Tool Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new Tool();
                }
                return _Instance;
            }
        }

        private BuildT3DB _buildTool;

        /// <summary>
        /// Private constructor
        /// </summary>
        private Tool() { }

        /// <summary>
        /// Initialize here
        /// </summary>
        public void Start()
        {
            AddSignalListeners();
        }

        /// <summary>
        /// Add signal listeners here
        /// </summary>
        private void AddSignalListeners()
        {
            Signal.Add(SignalKey.TOOLS_RESIZE, Resize);
            Signal.Add(SignalKey.TOOLS_DBBUILD, DBBuild);
        }

        /// <summary>
        /// On click resize tool
        /// </summary>
        public void Resize(object dirInfo)
        {
            DirectoryInfo castedDirInfo = (DirectoryInfo) dirInfo;

            // Escape if no directory is selected or wrong boxing type
            if (castedDirInfo == null)
            {
                return;
            }

            new ResizeTool(castedDirInfo).Show();
        }

        /// <summary>
        /// On click dbbuild tool
        /// </summary>
        public void DBBuild(object e)
        {
            // Prevent multiple build tool instances
            if (_buildTool != null)
            {
                _buildTool.Close();
            }

            _buildTool = new BuildT3DB();
            _buildTool.Show();
        }
    }
}
