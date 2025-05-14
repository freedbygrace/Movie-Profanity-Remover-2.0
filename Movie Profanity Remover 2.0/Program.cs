using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Movie_Profanity_Remover_2._0
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Initialize FFMPEG
            Tool.CreateFFMPEG();

            // Check if running in CLI mode
            if (args.Length > 0)
            {
                // Run in CLI mode
                CliProgram.Run(args);
            }
            else
            {
                // Run in GUI mode
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new CfrmMain());
            }
        }
    }
}
