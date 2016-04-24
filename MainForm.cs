// Programming by Eric Chauvin.
// My blogs are at:
// ericlibproj.blogspot.com
// and
// ericbreakingrsa.blogspot.com



// I'm just getting started on this code editor.
// Microsoft Visual Studio has gotten too _helpful_,
// with the Clippy character lightbulb and all the other
// stuff flashing on the screen.  So I wanted to have
// a basic code editor without all of the distractions.
// I also wanted to have the menu font sizes big
// enough so I can see them easily.

// This calls MSBuild to build programs written in C#,
// but this is also going to be used for Android/Gradle
// and any other code I write.


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics; // For starting a program/process.
using System.IO;
using System.Threading;


namespace CodeEditor
{
  public partial class MainForm : Form
  {
  internal const string VersionDate = "4/24/2016";
  internal const int VersionNumber = 09; // 0.9
  internal const string MessageBoxTitle = "Code Editor";
  private Process MSBuildProcess;
  private bool IsClosing = false;
  private ECTime BuildStartTime;
  private EditorTabPage[] TabPagesArray;
  private int TabPagesArrayLast = 0;
  private string DataDirectory = "";
  private EditorTabPage StatusPage;
  private GlobalProperties GlobalProps;
  private TextBox SelectedTextBox;



  public MainForm()
    {
    InitializeComponent();

    ///////////////////////
    // Keep this at the top:
    SetupDirectories();
    GlobalProps = new GlobalProperties( this );
    ///////////////////////

    BuildStartTime = new ECTime();

    TabPagesArray = new EditorTabPage[2];

    MainTabControl.TabPages.Clear();

    StatusPage = new EditorTabPage( this, "BuildStatus.txt", DataDirectory );
    TabPagesArray[TabPagesArrayLast] = StatusPage;
    TabPagesArrayLast++;
    MainTabControl.TabPages.Add( StatusPage );
    StatusPage.ClearTextBox();

    AddTabPage( "HelloWorld.cs", "C:\\Temp\\" );
    }



  private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
    string ShowS = "Programming by Eric Chauvin." +
      " Version date: " + VersionDate;

    MessageBox.Show( ShowS, MessageBoxTitle, MessageBoxButtons.OK );
    }




  internal bool StartMSBuild( string ProjectFileName,
                              string WorkingDirectory )
    {
    try
    {
    // Set Envirnment Path to 
    // c:\Program Files (x86)\MSBuild\14.0\Bin\
    // MSBuild.exe MyProj.proj /property:Configuration=Debug

    MSBuildProcess = new Process();

    // startInfo.WindowStyle = ProcessWindowStyle.Minimized;

    MSBuildProcess.StartInfo.UseShellExecute = false;
    MSBuildProcess.StartInfo.WorkingDirectory = WorkingDirectory;

    // /p:Configuration=Release
    MSBuildProcess.StartInfo.Arguments = "HelloWorld.csproj"; // /?";
    MSBuildProcess.StartInfo.RedirectStandardOutput = true;
    MSBuildProcess.StartInfo.RedirectStandardInput = true;
    MSBuildProcess.StartInfo.FileName = "MSBuild.exe";
    MSBuildProcess.StartInfo.Verb = ""; // "Print";
    MSBuildProcess.StartInfo.CreateNoWindow = true;
    MSBuildProcess.StartInfo.ErrorDialog = false;

    BuildStartTime.SetToNow();
    MSBuildProcess.Start();

    BuildTimer.Interval = 200;
    BuildTimer.Start();
    // WaitForExit() will hold up this main GUI thread.

    return true;

    }
    catch( Exception Except )
      {
      MessageBox.Show( "Could not start the MSBuild program.\r\n\r\n" + Except.Message, MessageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Stop );
      return false;
      }
    }




  private void FinishMSBuild()
    {
    if( MSBuildProcess == null )
      {
      BuildTimer.Stop();
      return;
      }

    if( !MSBuildProcess.HasExited )
      {
      ShowStatus( "MSBuild seconds: " + BuildStartTime.GetSecondsToNow().ToString( "N2" ) );
      return;
      }

    BuildTimer.Stop();

    ShowStatus( "MSBuild has exited." );
    ShowStatus( " " );

    StreamReader SReader = MSBuildProcess.StandardOutput;
    while( SReader.Peek() >= 0 ) 
      {
      string Line = SReader.ReadLine();
      if( Line == null )
        break;

      ShowStatus( Line );
      }

    MSBuildProcess.Dispose();
    MSBuildProcess = null;

    ShowStatus( " " );
    ShowStatus( "Finished with MSBuild." );
    }



  private void BuildTimer_Tick(object sender, EventArgs e)
    {
    FinishMSBuild();
    }


  internal void ShowStatus( string Status )
    {
    if( IsClosing )
      return;

    StatusPage.AppendToTextBox( Status ); 
    }



  private void buildToolStripMenuItem1_Click(object sender, EventArgs e)
    {
    // Show the StatusTabPage:
    MainTabControl.SelectedIndex = 0;

    SaveAllFiles();

    StartMSBuild( "C:\\Temp\\HelloWorld.csproj",
                  "C:\\Temp" );

    }



  private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
    IsClosing = true;
    KeyboardTimer.Stop();
    BuildTimer.Stop();
    SaveAllFiles();

    DisposeOfEverything();
    }




  internal void StartKeyboardTimer( TextBox UseTextBox )
    {
    SelectedTextBox = UseTextBox;
    KeyboardTimer.Interval = 25;
    KeyboardTimer.Start();
    }



  private void KeyboardTimer_Tick(object sender, EventArgs e)
    {
    KeyboardTimer.Stop();
    if( IsClosing )
      return;

    if( SelectedTextBox == null )
      return;

    if( SelectedTextBox.IsDisposed )
      return;

    // int Start = StatusTextBox.SelectionStart;
    int Start = SelectedTextBox.GetFirstCharIndexOfCurrentLine();
    int Line = SelectedTextBox.GetLineFromCharIndex( Start );
    BottomLabel.Text = "Line: " + Line.ToString( "N0" );
    }



  private void AddTabPage( string FileName, string Path )
    {
    try
    {
    EditorTabPage NewTabPage = new EditorTabPage( this, FileName, Path );
    TabPagesArray[TabPagesArrayLast] = NewTabPage;
    TabPagesArrayLast++;

    if( TabPagesArrayLast >= TabPagesArray.Length )
      {
      Array.Resize( ref TabPagesArray, TabPagesArray.Length + 16 );
      }

    MainTabControl.TabPages.Add( NewTabPage );

    // MainTabControl.TabPages.Count

    /*

    MainTabControl.TabPages.Insert( Where, tabPage1 );
    Remove( tabPage1 );
    RemoveAt()
    "To change the order of tabs in the control, you must
     change their positions in the collection by removing
      them and inserting them at new indexes."

    */

    }
    catch( Exception Except )
      {
      ShowStatus( "Exception in AddTabPage()." );
      ShowStatus( Except.Message );
      }
    }



  internal void SaveAllFiles()
    {
    for( int Count = 0; Count < TabPagesArrayLast; Count++ )
      {
      TabPagesArray[Count].WriteToTextFile();
      }
    }



  private void DisposeOfEverything()
    {
    try
    {
    MainTabControl.TabPages.Clear();

    for( int Count = 0; Count < TabPagesArrayLast; Count++ )
      {
      TabPagesArray[Count].DisposeOfEverything();
      TabPagesArray[Count] = null;
      }

    TabPagesArrayLast = 0;

    }
    catch( Exception )
      {
      MessageBox.Show( "Exception in MainForm.DisposeOfEverything().", MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }
    }



  private void SetupDirectories()
    {
    try
    {
    DataDirectory = Application.StartupPath + "\\Data\\";

    if( !Directory.Exists( DataDirectory ))
      Directory.CreateDirectory( DataDirectory );

    }
    catch( Exception )
      {
      MessageBox.Show( "Error: The directory could not be created.", MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }
    }



  private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
    SaveAllFiles();
    }



  }
}

