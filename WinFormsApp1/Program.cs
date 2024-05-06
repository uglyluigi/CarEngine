using Khronos;
using System.Diagnostics;

namespace ChungusEngine
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            KhronosApi.Log += delegate (object? sender, KhronosLogEventArgs e)
            {
                Debug.WriteLine(e.ToString());
            };
            KhronosApi.LogEnabled = true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SampleForm());
        }
    }
}