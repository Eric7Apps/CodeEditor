// Copyright Eric Chauvin 2018.
// My blog is at:
// ericsourcecode.blogspot.com


using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics; // For starting a program/process.
using System.IO;


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

    MSBuildProcess.StartInfo.UseShellExecute = true;
    MSBuildProcess.StartInfo.WorkingDirectory = WorkingDirectory;

    MSBuildProcess.StartInfo.Arguments = "";
    MSBuildProcess.StartInfo.RedirectStandardOutput = false;
    MSBuildProcess.StartInfo.RedirectStandardInput = false;

    // Use the full path so you don't have to set
    // the Environment variable.
    // So it can find the MSBuild.exe:
    // Set Envirnment Path to
    // c:\Windows\Microsoft.NET\Framework\v4.0.30319
    // MSBuild.exe MyProj.csproj

    MForm.ShowStatus( "Starting: " + ProjectFileName );

    MSBuildProcess.StartInfo.FileName = ProjectFileName;
    // MSBuildProcess.StartInfo.FileName = "c:\\Eric\\ClimateModel\\BuildProj.bat";
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



  /*
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
    */


  internal void DisposeOfEverything()
    {
    if( MSBuildProcess == null )
      return;

    MSBuildProcess.Dispose();
    MSBuildProcess = null;
    }



  /*
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
    */


  }
}

