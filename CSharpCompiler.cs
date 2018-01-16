// Copyright Eric Chauvin 2018.
// My blog is at:
// ericsourcecode.blogspot.com


// Csc.exe is in the same directory as the
// MSBuild.exe file.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics; // For starting a program/process.
using System.IO;
using ECCommon;


namespace CodeEditor
{
  class CSharpCompiler
  {
  private MainForm MForm;
  private Process CSharpCompProcess;
  private ECTime CSharpStartTime;


  private CSharpCompiler()
    {
    }



  internal CSharpCompiler( MainForm UseForm )
    {
    MForm = UseForm;
    CSharpStartTime = new ECTime();
    }



  internal bool StartCSharp( string FileName,
                             string WorkingDirectory )
    {
    try
    {
    CSharpStartTime.SetToNow();

    if( CSharpCompProcess != null )
      CSharpCompProcess.Dispose();

    CSharpCompProcess = new Process();

    // startInfo.WindowStyle = ProcessWindowStyle.Minimized;

    CSharpCompProcess.StartInfo.UseShellExecute = false;
    CSharpCompProcess.StartInfo.WorkingDirectory = WorkingDirectory;

    // This compiler only supportes up to version 5
    // of the C# language, the next version is the
    // open source Roslyn compiler.
    // http://go.microsoft.com/fwlink/?LinkID=533240

    // Visual C# Compiler Options

    //                        - OUTPUT FILES -
    // /out:<file>    Specify output file name (default: base name of file with main class or first file)
    // /target:exe                    Build a console executable (default) (Short form: /t:exe)
    // /target:winexe                 Build a Windows executable (Short form: /t:winexe)
    // /platform:<string>             Limit which platforms this code can run on: x86, Itanium, x64, arm, anycpu32bitpreferred, or anycpu. The default is anycpu.

    //                         - INPUT FILES -
    // /recurse:<wildcard>            Include all files in the current directory and subdirectories according to the wildcard specifications

    // /optimize[+|-]                 Enable optimizations (Short form: /o)

    //                         - ERRORS AND WARNINGS -
    // /warnaserror[+|-]              Report all warnings as errors
    // /warn:<n>                      Set warning level (0-4) (Short form: /w)
    // /nowarn:<warn list>            Disable specific warning messages

    // @<file>                        Read response file for more options
    // /help                          Display this usage message (Short form: /?)

    // /errorreport:<string>          Specify how to handle internal compiler errors: prompt, send, queue, or none. The default is queue.
    // /appconfig:<file>              Specify an application configuration file containing assembly binding settings
    // /moduleassemblyname:<string>   Name of the assembly which this module will be a part of


    // csc /warnaserror+
    // As you would hope, csc.exe supports wildcard
    // notation. Thus, to compile all files within a
    // single directory, simply specify *.cs as the
    // input option:
    // csc /t:library /out:MyCodeLibrary.dll *.cs


     // Recursively in subdirectories:
     // csc /t:library /out:MyCodeLibrary.dll /recurse:AsmInfo /doc:myDoc.xml *.cs

     // could compile all C# files within all
     // subdirectories with the following command
     //  set:
     // csc /t:library /out:MyCodeLibrary.dll /recurse:*.cs

     // To compile this application as a Windows Forms
     // executable, make sure to specify winexe as
     // the decoration for the /target flag.

     // When enabled (/optimize+) you instruct the
    //  compiler to generate the smallest and fastest
    // assembly as possible.



    // Configuration
    // string Parameters = "/help ";
    string Parameters = "/target:winexe ";

    CSharpCompProcess.StartInfo.Arguments = Parameters + FileName;

    CSharpCompProcess.StartInfo.RedirectStandardOutput = true;
    CSharpCompProcess.StartInfo.RedirectStandardInput = true;

    // Use the full path so you don't have to set
    // the Environment variable.
    // So it can find the MSBuild.exe:
    // Set Envirnment Path to
    // c:\Windows\Microsoft.NET\Framework\v4.0.30319
    // csc.exe MyFile.cs

    string CSharpCompFile = "c:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\Csc.exe";
    CSharpCompProcess.StartInfo.FileName = CSharpCompFile;
    CSharpCompProcess.StartInfo.Verb = ""; // "Print";
    CSharpCompProcess.StartInfo.CreateNoWindow = true;
    CSharpCompProcess.StartInfo.ErrorDialog = false;

    CSharpCompProcess.Start();
    // WaitForExit() will hold up this main GUI thread.

    MForm.ShowStatus( "CSharpCompProcess started." );
    return true;
    }
    catch( Exception Except)
      {
      MForm.ShowStatus( "Could not start the Csc.exe program." );
      MForm.ShowStatus( Except.Message );
      if( CSharpCompProcess != null )
        {
        CSharpCompProcess.Dispose();
        CSharpCompProcess = null;
        }

      return false;
      }
    }




  internal bool IsCompileFinished()
    {
    try
    {
    if( CSharpCompProcess == null )
      return true;

    // CSharpCompProcess.Refresh();
    // "After Refresh is called, the first request
    // for information about each property causes the
    // process component to obtain a new value from
    // the associated process."

    // "The Process component is a snapshot of the
    // process resource at the time they are
    // associated. To view the current values for
    // the associated process, call the Refresh method."

    // Console.WriteLine("Physical Memory Usage: "
       //   + myProcess.WorkingSet.ToString());

    // MForm.ShowStatus( "Before HasExited()." );

    if( !CSharpCompProcess.HasExited )
      {
      // Don't do this here.  ShowMSBuildLines();
      MForm.ShowStatus( " " );
      MForm.ShowStatus( "CSharpCompProcess seconds: " + CSharpStartTime.GetSecondsToNow().ToString("N2") );
      // MForm.ShowStatus( " " );
      return false;
      }

    // MForm.ShowStatus( "After HasExited()." );

    MForm.ShowStatus( "CSharp Process has exited." );
    MForm.ShowStatus( " " );

    ShowCompileLines();

    CSharpCompProcess.Dispose();
    CSharpCompProcess = null;

    MForm.ShowStatus( " " );
    MForm.ShowStatus( "Finished with CSharp Compile." );
    return true;

    }
    catch( Exception Except )
      {
      MForm.ShowStatus( "Exception in IsCompileFinished(). " + Except.Message );
      return true; // True to finish it.
      }
    }



  internal void DisposeOfEverything()
    {
    if( CSharpCompProcess == null )
      return;

    CSharpCompProcess.Dispose();
    CSharpCompProcess = null;
    }



  internal void ShowCompileLines()
    {
    if( CSharpCompProcess == null )
      return;

    // "Synchronous read operations introduce a
    // dependency between the caller reading from the
    // StandardOutput stream and the child process
    // writing to that stream. These dependencies can
    // result in deadlock conditions."

    // Refresh();
    StreamReader SReader = CSharpCompProcess.StandardOutput;
    while( SReader.Peek() >= 0 )
      {
      string Line = SReader.ReadLine();
      // string Line = SReader.ReadToEnd();
      if( Line == null )
        continue;

      MForm.ShowStatus( Line );
      }
    }



  }
}



