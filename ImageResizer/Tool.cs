using ImageResizer.Controls.Tools;
using ImageResizer.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

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
        /// <param name="dirInfo">Should be boxed DirectoryInfo</param>
        public void Resize(object dirInfo)
        {
            DirectoryInfo castedDirInfo = (DirectoryInfo) dirInfo;

            // Escape if no directory is selected or wrong boxing type
            if (castedDirInfo == null)
            {
                return;
            }

            new ResizeTool(castedDirInfo).ShowDialog();
        }

        /// <summary>
        /// On click dbbuild tool
        /// </summary>
        /// <param name="e"></param>
        public void DBBuild(object e)
        {
            new BuildT3DB().ShowDialog();
        }
    }
}
