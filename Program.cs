using System;
using System.Collections.Generic;
// using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;


namespace CodeEditor2
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

    Application.ThreadException += new ThreadExceptionEventHandler( MainForm.UIThreadException );
    Application.SetUnhandledExceptionMode( UnhandledExceptionMode.CatchException );

    // AppDomain.CurrentDomain.UnhandledException +=
       //  new UnhandledExceptionEventHandler( CurrentDomain_UnhandledException );

      Application.Run(new MainForm());
    }
  }
}

