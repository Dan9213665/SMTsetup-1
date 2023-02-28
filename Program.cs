using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Web;
using Microsoft.Office.Interop.Excel;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
namespace SMTsetup
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
            ApplicationConfiguration.Initialize();
            System.Windows.Forms.Application.Run(new SMTSetupMain());
        }
    }
}
