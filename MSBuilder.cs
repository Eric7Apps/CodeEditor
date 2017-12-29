// Copyright Eric Chauvin 2017 - 2018.
// My blog is at:
// ericsourcecode.blogspot.com


using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics; // For starting a program/process.
using System.IO;
using ECCommon;


namespace CodeEditor
{
  class MSBuilder
  {
  private MainForm MForm;
  private Process MSBuildProcess;
  private ECTime BuildStartTime;


  private MSBuilder()
    {
    }



  internal MSBuilder( MainForm UseForm )
    {
    MForm = UseForm;
    BuildStartTime = new ECTime();
    }



  internal bool StartMSBuild( string ProjectFileName,
                              string WorkingDirectory )
    {
    try
    {
    BuildStartTime.SetToNow();

    if( MSBuildProcess != null )
      MSBuildProcess.Dispose();

    MSBuildProcess = new Process();

    // startInfo.WindowStyle = ProcessWindowStyle.Minimized;

    MSBuildProcess.StartInfo.UseShellExecute = false;
    MSBuildProcess.StartInfo.WorkingDirectory = WorkingDirectory;

    // /p:Configuration=Release
    // Command line options:
    // https://msdn.microsoft.com/en-us/library/ms164311.aspx

    // /toolsversion:14.0 ??

    // Configuration
    // The configuration that you are building, either
    // "Debug" or "Release."
  // DebugSymbols
  // A boolean value that indicates whether symbols
  // are generated by the build.
  // Setting /p:DebugSymbols=false on the command
  // line disables generation of program database
  // (.pdb) symbol files.

  // DefineDebug
  // A boolean value that indicates whether you want
  // the DEBUG constant defined.

  // DefineTrace
  // A boolean value that indicates whether you want
  // the TRACE constant defined.

    string Parameters = "/p:Configuration=Release ";
    MSBuildProcess.StartInfo.Arguments = Parameters + ProjectFileName;

    MSBuildProcess.StartInfo.RedirectStandardOutput = true;
    MSBuildProcess.StartInfo.RedirectStandardInput = true;

    // Use the full path so you don't have to set
    // the Environment variable.
    // So it can find the MSBuild.exe:
    // Set Envirnment Path to 
    // c:\Windows\Microsoft.NET\Framework\v4.0.30319
    // MSBuild.exe MyProj.csproj

    string MSBuildFile = "c:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\MSBuild.exe";
    MSBuildProcess.StartInfo.FileName = MSBuildFile; // "MSBuild.exe";
    MSBuildProcess.StartInfo.Verb = ""; // "Print";
    MSBuildProcess.StartInfo.CreateNoWindow = true;
    MSBuildProcess.StartInfo.ErrorDialog = false;

    MSBuildProcess.Start();
    // WaitForExit() will hold up this main GUI thread.

    MForm.ShowStatus( "MSBuildProcess started." );
    return true;
    }
    catch( Exception Except)
      {
      MForm.ShowStatus( "Could not start the MSBuild program." );
      MForm.ShowStatus( Except.Message );
      if( MSBuildProcess != null )
        {
        MSBuildProcess.Dispose();
        MSBuildProcess = null;
        }

      return false;
      }
    }




  internal bool IsMSBuildFinished()
    {
    try
    {
    if( MSBuildProcess == null )
      return true;

    // MSBuildProcess.Refresh();
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

    if( !MSBuildProcess.HasExited )
      {
      // Don't do this here.  ShowMSBuildLines();
      MForm.ShowStatus( " " );
      MForm.ShowStatus( "MSBuild seconds: " + BuildStartTime.GetSecondsToNow().ToString("N2") );
      // MForm.ShowStatus( " " );
      return false;
      }

    // MForm.ShowStatus( "After HasExited()." );

    // BuildTimer.Stop();

    MForm.ShowStatus( "MSBuild has exited." );
    MForm.ShowStatus( " " );

    ShowMSBuildLines();

    MSBuildProcess.Dispose();
    MSBuildProcess = null;

    MForm.ShowStatus( " " );
    MForm.ShowStatus( "Finished with MSBuild." );
    return true;

    }
    catch( Exception Except )
      {
      MForm.ShowStatus( "Exception in MSBuilder.IsMSBuildFinished(). " + Except.Message );
      return true; // True to finish it.
      }
    }



  internal void DisposeOfEverything()
    {
    if( MSBuildProcess == null )
      return;

    MSBuildProcess.Dispose();
    MSBuildProcess = null;
    }



  internal void ShowMSBuildLines()
    {
    if( MSBuildProcess == null )
      return;

    // "Synchronous read operations introduce a
    // dependency between the caller reading from the
    // StandardOutput stream and the child process
    // writing to that stream. These dependencies can
    // result in deadlock conditions."

    // Refresh();
    StreamReader SReader = MSBuildProcess.StandardOutput;
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
