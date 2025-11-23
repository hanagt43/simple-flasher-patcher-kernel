// File: Program.cs
using SimpleFlasherPatcher;
using System;
using System.Windows.Forms;

namespace SimpleFlasherPatcher
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
