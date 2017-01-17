using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageResizer.Controls.Tools
{
    /// <summary>
    /// Interaction logic for BuildT3DB.xaml
    /// </summary>
    public partial class BuildT3DB : Window
    {
        public BuildT3DB()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            // TODO: initialize here
        }

        /// <summary>
        /// Create command file and execute
        /// </summary>
        private void OnBuildButton_Click(object sender, RoutedEventArgs e)
        {
            string cmdFileName = string.Format("{0}/__buildcommand.bat", System.AppDomain.CurrentDomain.BaseDirectory);
            byte[] content = Encoding.UTF8.GetBytes(
                string.Join("\n",
                    @"Y:",
                    @"cd Y:\Dev",
                    @"attrib -r ""Y:\Dev\DBConverter\Excel2Json\bin\Source\0.Original DB\fifa_ng_db.xls""",
                    @"call p4 edit -t binary -c default ""Y:\Dev\DBConverter\Excel2Json\bin\Source\0.Original DB/fifa_ng_db.xls""",
                    @"PATH %PATH%=%PATH%; Y:\Dev",
                    @"call Y:\Dev\cmdenv_set.bat",
                    @"call Y:\Dev\Art_T3DB.bat",
                    @"echo ""DONE!""",
                    @"pause"
                ));

            // write as file to run
            using (var writer = new FileStream(cmdFileName, FileMode.OpenOrCreate))
            {
                writer.Write(content, 0, content.Length);
            }

            Process proc = Process.Start(new ProcessStartInfo()
            {
                FileName = cmdFileName,
                Verb = "runas",
                UseShellExecute = true,
                ErrorDialog = true
            });
            proc.WaitForExit();
        }

        private void OnExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
