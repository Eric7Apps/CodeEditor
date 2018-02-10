// Copyright Eric Chauvin 2018.
// My blog is at:
// ericsourcecode.blogspot.com

// For examples and notes about project files, see
// any of the project files in any of the projects
// I have in the repositories on GitHub.


using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
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

    MForm.ShowStatus( "Working Directory: " + WorkingDirectory );
    MForm.ShowStatus( "Starting: " + ProjectFileName );

    MSBuildProcess.StartInfo.FileName = ProjectFileName;
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
      MForm.ShowStatus( "Could not start the Build batch file." );
      MForm.ShowStatus( Except.Message );
      if( MSBuildProcess != null )
        {
        MSBuildProcess.Dispose();
        MSBuildProcess = null;
        }

      return false;
      }
    }



  internal void DisposeOfEverything()
    {
    if( MSBuildProcess == null )
      return;

    MSBuildProcess.Dispose();
    MSBuildProcess = null;
    }



//////////////////////////////////////////
// Example batch file to call MSBuild.exe:
/*

@echo off

rem echo Test this.

rem This should already be running in the
rem working directory this batch file is in.
rem cd c:\Eric\ClimateModel

rem The properties can be in the project file instead
rem of on the command line.
c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe  /nologo /p:Configuration=Release /p:DebugSymbols=false /p:DefineDebug=false /p:DefineTrace=false /fileLogger /verbosity:normal ComputationLinks.csproj

rem Help:
rem c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe  /help

pause
*/

  }
}


