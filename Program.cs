using System;
using System.Windows.Forms;
namespace Win7Clipboard { static class Program { [STAThread] static void Main(){ Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false); Application.Run(new MainForm()); } } }