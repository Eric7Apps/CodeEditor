// Copyright Eric Chauvin 2018.
// My blog is at:
// ericsourcecode.blogspot.com



// Code Editor version 2.


// Microsoft Visual Studio has gotten too _helpful_,
// with the Clippy character lightbulb and all the
// other stuff flashing on the screen.  So I wanted
// to have a basic code editor without all of the
// distractions.


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
// using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics; // For starting a program/process.
using System.IO;



namespace CodeEditor2
{
  // public partial class MainForm : Form
  public class MainForm : Form
  {
  internal const string VersionDate = "4/17/2018";
  internal const int VersionNumber = 20; // 2.0
  private System.Threading.Mutex SingleInstanceMutex = null;
  private bool IsSingleInstance = false;
  private bool IsClosing = false;
  private bool Cancelled = false;
  private EditorTabPage[] TabPagesArray;
  private int TabPagesArrayLast = 0;
  internal const string MessageBoxTitle = "Code Editor";
  private string DataDirectory = "";
  private TextBox StatusTextBox;
  private MSBuilder Builder;
  private ConfigureFile MainConfigFile;
  private ConfigureFile ProjectConfigFile;
  private string ShowProjectText = "";
  private string SearchText = "";
  private Process ProgProcess;
  private float MainTextFontSize = 34.0F;

  // System.Windows.Forms.
  private MenuStrip menuStrip1;
  private ToolStripMenuItem fileToolStripMenuItem;
  private ToolStripMenuItem saveToolStripMenuItem;
  private ToolStripMenuItem exitToolStripMenuItem;
  private ToolStripMenuItem helpToolStripMenuItem;
  private ToolStripMenuItem aboutToolStripMenuItem;
  private Panel BottomPanel;
  private Panel MainPanel;
  private TabControl MainTabControl;
  // private TabPage tabPage1;
  // private TabPage tabPage2;
  private System.Windows.Forms.Timer KeyboardTimer;
  private System.Windows.Forms.Timer SingleInstanceTimer;
  // private TextBox TextBox1;
  private ToolStripMenuItem buildToolStripMenuItem;
  private ToolStripMenuItem buildToolStripMenuItem1;
  private Label CursorLabel;
  private OpenFileDialog OpenFileDialog1;
  private ToolStripMenuItem openToolStripMenuItem;
  private ToolStripMenuItem showNonAsciiToolStripMenuItem;
  private ToolStripMenuItem closeAllToolStripMenuItem;
  private SaveFileDialog SaveFileDialog1;
  private ToolStripMenuItem saveFileAsToolStripMenuItem;
  private ToolStripMenuItem editToolStripMenuItem;
  private ToolStripMenuItem copyToolStripMenuItem;
  private ToolStripMenuItem cutToolStripMenuItem;
  private ToolStripMenuItem selectAllToolStripMenuItem;
  private ToolStripMenuItem projectToolStripMenuItem;
  private ToolStripMenuItem setCurrentProjectToolStripMenuItem;
  private ToolStripMenuItem findToolStripMenuItem;
  private ToolStripMenuItem findNextToolStripMenuItem;
  private ToolStripMenuItem closeCurrentToolStripMenuItem;
  private ToolStripMenuItem removeEmptyLinesToolStripMenuItem;
  private ToolStripMenuItem debugToolStripMenuItem;
  private ToolStripMenuItem runWithoutDebuggingToolStripMenuItem;
  private ToolStripMenuItem showLogToolStripMenuItem;
  private ToolStripMenuItem setExecutableToolStripMenuItem;
  private ToolStripMenuItem codeAnalysisToolStripMenuItem;
  private ToolStripMenuItem runToolStripMenuItem;
  private ToolStripMenuItem newFileToolStripMenuItem;




  public MainForm()
    {
    // InitializeComponent();
    InitializeGuiComponents();

    // this.Font = new System.Drawing.Font("Microsoft Sans Serif", 40F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
    this.Font = new System.Drawing.Font( "Consolas", 28.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
    this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));

    if( !CheckSingleInstance())
      return;

    IsSingleInstance = true;

    ///////////
    // Keep this at the top.
    SetupDirectories();
    MainConfigFile = new ConfigureFile( DataDirectory + "MainConfig.txt", this );

    string CurrentProjectFileName = MainConfigFile.GetString( "CurrentProjectFile" );
    if( CurrentProjectFileName.Length < 4 )
      CurrentProjectFileName = DataDirectory + "ProjectOptions.txt";

    ProjectConfigFile = new ConfigureFile( CurrentProjectFileName, this );
    ///////////

    SetShowProjectText();

    TabPagesArray = new EditorTabPage[2];
    MainTabControl.TabPages.Clear();

    StatusTextBox = new TextBox();
    AddStatusPage();
    OpenRecentFiles();

    ShowStatus( "Version date: " + VersionDate );
    // MessageBox.Show( "Test this.", MessageBoxTitle, MessageBoxButtons.OK);

    Builder = new MSBuilder( this );
    KeyboardTimer.Interval = 100;
    KeyboardTimer.Start();
    }



  internal string GetDataDirectory()
    {
    return DataDirectory;
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
      MessageBox.Show( "Error: The directory could not be created.", MessageBoxTitle, MessageBoxButtons.OK);
      return;
      }
    }



  internal bool GetIsClosing()
    {
    return IsClosing;
    }



  internal void SetCancelled()
    {
    Cancelled = true;
    }



  internal bool GetCancelled()
    {
    return Cancelled;
    }



  internal bool CheckEvents()
    {
    if( IsClosing )
      return false;

    Application.DoEvents();

    if( Cancelled )
      return false;

    return true;
    }



  // This has to be added in the Program.cs file.
  //   Application.ThreadException += new ThreadExceptionEventHandler( MainForm.UIThreadException );
  //   Application.SetUnhandledExceptionMode( UnhandledExceptionMode.CatchException );
    // What about this part?
    // AppDomain.CurrentDomain.UnhandledException +=
       //  new UnhandledExceptionEventHandler( CurrentDomain_UnhandledException );
  internal static void UIThreadException( object sender, ThreadExceptionEventArgs t )
    {
    string ErrorString = t.Exception.Message;

    try
      {
      string ShowString = "There was an unexpected error:\r\n\r\n" +
             "The program will close now.\r\n\r\n" +
             ErrorString;

      MessageBox.Show( ShowString, "Program Error", MessageBoxButtons.OK, MessageBoxIcon.Stop );
      }

    finally
      {
      Application.Exit();
      }
    }



  private bool CheckSingleInstance()
    {
    bool InitialOwner = false; // Owner for single instance check.
    string ShowS = "Another instance of the Code Editor is already running." +
      " This instance will close.";

    try
    {
    SingleInstanceMutex = new System.Threading.Mutex( true, "Eric's Code Editor Version 2 Single Instance", out InitialOwner );
    }
    catch
      {
      MessageBox.Show( ShowS, MessageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Stop );
      // mutex.Close();
      // mutex = null;

      // Can't do this here:
      // Application.Exit();

      SingleInstanceTimer.Interval = 50;
      SingleInstanceTimer.Start();
      return false;
      }

    if( !InitialOwner )
      {
      MessageBox.Show( ShowS, MessageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Stop );
      // Application.Exit();
      SingleInstanceTimer.Interval = 50;
      SingleInstanceTimer.Start();
      return false;
      }

    return true;
    }



  internal void SaveStatusToFile()
    {
    try
    {
    string FileName = DataDirectory + "MainStatus.txt";

    using( StreamWriter SWriter = new StreamWriter( FileName, false, Encoding.UTF8 ))
      {
      foreach( string Line in StatusTextBox.Lines )
        {
        SWriter.WriteLine( Line );
        }
      }

    // MForm.StartProgramOrFile( FileName );
    }
    catch( Exception Except )
      {
      ShowStatus( "Error: Could not write the status to the file." );
      ShowStatus( Except.Message );
      return;
      }
    }



  private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
    string ShowS = "Programming by Eric Chauvin." +
            " Version date: " + VersionDate;

    MessageBox.Show(ShowS, MessageBoxTitle, MessageBoxButtons.OK);
    }




  private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
    if( IsSingleInstance )
      {
      if( DialogResult.Yes != MessageBox.Show( "Close the program?", MessageBoxTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question ))
        {
        e.Cancel = true;
        return;
        }
      }

    IsClosing = true;
    KeyboardTimer.Stop();

    // if( IsSingleInstance )
      // {
      // SaveAllFiles();
      FreeEverything();
      // }

    // ShowStatus() won't show it when it's closing.
    // MainTextBox.AppendText( "Saving files.\r\n" );
    SaveStatusToFile();
    }



  private TextBox GetSelectedTextBox()
    {
    int SelectedIndex = MainTabControl.SelectedIndex;
    if( SelectedIndex >= TabPagesArray.Length )
      return null;

    if( SelectedIndex < 0 )
      return null;

    TextBox SelectedBox = TabPagesArray[SelectedIndex].MainTextBox;
    return SelectedBox;
    }



  private void KeyboardTimer_Tick(object sender, EventArgs e)
    {
    try
    {
    // KeyboardTimer.Stop();
    if (IsClosing)
      return;

    int SelectedIndex = MainTabControl.SelectedIndex;
    if( SelectedIndex >= TabPagesArray.Length )
      {
      CursorLabel.Text = "No textbox selected.";
      return;
      }

    if( SelectedIndex < 0 )
      {
      CursorLabel.Text = "No textbox selected.";
      // MessageBox.Show( "There is no tab page selected, or the status page is selected. (Top.)", MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }

    // The status page is at zero.
    if( SelectedIndex == 0 )
      {
      CursorLabel.Text = "Status page.";
      return;
      }

    string TabTitle = TabPagesArray[SelectedIndex].TabTitle;
    // TabPagesArray[SelectedIndex].FileName;

    TextBox SelectedTextBox = TabPagesArray[SelectedIndex].MainTextBox;
    if (SelectedTextBox == null)
      return;

    if (SelectedTextBox.IsDisposed)
      return;

    int Start = SelectedTextBox.SelectionStart;
    // int Start2 = SelectedTextBox.GetFirstCharIndexOfCurrentLine();

    // The +1 is for display and matching with
    // the compiler error line number.
    int Line = 1 + SelectedTextBox.GetLineFromCharIndex( Start );
    CursorLabel.Text = "Line: " + Line.ToString("N0") + "     " + TabTitle + "      Proj: " + ShowProjectText;

    // KeyboardTimer.Start();
    }
    catch( Exception Except )
      {
      MessageBox.Show( "Exception in MainForm.KeyboardTimer_Tick(). " + Except.Message, MessageBoxTitle, MessageBoxButtons.OK);
      return;
      }
    }




  internal void ShowStatus(string Status)
    {
    if (IsClosing)
      return;

    StatusTextBox.AppendText( Status + "\r\n" );
    }



  internal void ClearStatus()
    {
    if (IsClosing)
      return;

    StatusTextBox.Text = "";
    }



  private void FreeEverything()
    {
    try
    {
    if( ProgProcess != null )
      {
      ProgProcess.Dispose();
      ProgProcess = null;
      }

    if( Builder != null )
      Builder.DisposeOfEverything();


    for (int Count = 0; Count < TabPagesArrayLast; Count++)
      {
      // And the tab pages?
      TabPagesArray[Count].MainTextBox.Dispose();
      // TabPagesArray[Count] = null;
      }

    StatusTextBox.Dispose();

    // Does this dispose of its owned objects?
    MainTabControl.TabPages.Clear();

    TabPagesArrayLast = 0;

    menuStrip1.Dispose();
    fileToolStripMenuItem.Dispose();
    saveToolStripMenuItem.Dispose();
    exitToolStripMenuItem.Dispose();
    helpToolStripMenuItem.Dispose();
    aboutToolStripMenuItem.Dispose();
    BottomPanel.Dispose();
    MainPanel.Dispose();
    MainTabControl.Dispose();
    // tabPage1.Dispose();
    // tabPage2.Dispose();
    KeyboardTimer.Dispose();
    SingleInstanceTimer.Dispose();
    // TextBox1;
    buildToolStripMenuItem.Dispose();
    buildToolStripMenuItem1.Dispose();
    CursorLabel.Dispose();
    OpenFileDialog1.Dispose();
    openToolStripMenuItem.Dispose();
    showNonAsciiToolStripMenuItem.Dispose();
    closeAllToolStripMenuItem.Dispose();
    SaveFileDialog1.Dispose();
    saveFileAsToolStripMenuItem.Dispose();
    editToolStripMenuItem.Dispose();
    copyToolStripMenuItem.Dispose();
    cutToolStripMenuItem.Dispose();
    selectAllToolStripMenuItem.Dispose();
    projectToolStripMenuItem.Dispose();
    setCurrentProjectToolStripMenuItem.Dispose();
    findToolStripMenuItem.Dispose();
    findNextToolStripMenuItem.Dispose();
    closeCurrentToolStripMenuItem.Dispose();
    removeEmptyLinesToolStripMenuItem.Dispose();
    debugToolStripMenuItem.Dispose();
    runWithoutDebuggingToolStripMenuItem.Dispose();
    showLogToolStripMenuItem.Dispose();
    setExecutableToolStripMenuItem.Dispose();
    codeAnalysisToolStripMenuItem.Dispose();
    runToolStripMenuItem.Dispose();
    newFileToolStripMenuItem.Dispose();
    }
    catch( Exception Except )
      {
      MessageBox.Show( "Exception in MainForm.FreeEverything(). " + Except.Message, MessageBoxTitle, MessageBoxButtons.OK);
      return;
      }
    }



  private void SingleInstanceTimer_Tick(object sender, EventArgs e)
    {
    SingleInstanceTimer.Stop();
    Application.Exit();
    }




  private void AddStatusPage()
    {
    try
    {
    TabPage TabPageS = new TabPage();

    StatusTextBox.AcceptsReturn = true;
    StatusTextBox.BackColor = System.Drawing.Color.Black;
    StatusTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
    StatusTextBox.Font = new System.Drawing.Font( "Consolas", MainTextFontSize, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
    StatusTextBox.ForeColor = System.Drawing.Color.White;
    StatusTextBox.Location = new System.Drawing.Point(3, 3);
    StatusTextBox.Multiline = true;

/*
    TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
    TextBox1.WordWrap = false;
*/

    StatusTextBox.Name = "TextBox1";
    StatusTextBox.Size = new System.Drawing.Size(781, 219);
    StatusTextBox.TabIndex = 0;

    TabPageS.Controls.Add( StatusTextBox );
    // TabPage1.Location = new System.Drawing.Point(4, 63);
    TabPageS.Name = "StatusTabPage";
    TabPageS.Padding = new System.Windows.Forms.Padding(3);
    TabPageS.Size = new System.Drawing.Size(787, 225);
    TabPageS.TabIndex = 0;
    TabPageS.Text = "Status";
    TabPageS.UseVisualStyleBackColor = true;
    TabPageS.Enter += new System.EventHandler( this.tabPage_Enter );

    MainTabControl.TabPages.Add( TabPageS );
    EditorTabPage NewPage = new EditorTabPage( this, "Status", DataDirectory + "Status.txt", StatusTextBox );
    TabPagesArray[TabPagesArrayLast] = NewPage;
    TabPagesArrayLast++;

    }
    catch( Exception Except )
      {
      MessageBox.Show( "Exception in MainForm.AddStatusPage(). " + Except.Message, MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }
    }



  private void OpenRecentFiles()
    {
    for( int Count = 1; Count <= 20; Count++ )
      {
      string FileName = ProjectConfigFile.GetString( "RecentFile" + Count.ToString() );
      if( FileName.Length < 1 )
        break;

      string TabTitle = Path.GetFileName( FileName );
      AddNewPage( TabTitle, FileName );
      }
    }



  private bool FileIsInTabPages( string FileName )
    {
    try
    {
    for( int Count = 0; Count < TabPagesArrayLast; Count++ )
      {
      if( FileName.ToLower() == TabPagesArray[Count].FileName.ToLower())
        return true;

      }

    return false;

    }
    catch( Exception Except )
      {
      MessageBox.Show( "Exception in MainForm.FileIsInTabPages(). " + Except.Message, MessageBoxTitle, MessageBoxButtons.OK );
      return false;
      }
    }



  private void AddNewPage( string TabTitle, string FileName )
    {
    try
    {
    if( FileIsInTabPages( FileName ))
      {
      MessageBox.Show( "That file is already in the tab pages:\r\n" + FileName, MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }

    TabPage TabPage1 = new TabPage();
    TextBox TextBox1 = new TextBox();

    TextBox1.AcceptsReturn = true;
    TextBox1.BackColor = System.Drawing.Color.Black;
    TextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
    TextBox1.Font = new System.Drawing.Font( "Consolas", MainTextFontSize, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
    TextBox1.ForeColor = System.Drawing.Color.White;
    TextBox1.Location = new System.Drawing.Point(3, 3);
    TextBox1.Multiline = true;
    TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
    TextBox1.WordWrap = false;
    TextBox1.MaxLength = 1000000000;
    TextBox1.Name = "TextBox1";
    TextBox1.Size = new System.Drawing.Size(781, 219);
    TextBox1.TabIndex = 0;

    TabPage1.Controls.Add( TextBox1 );
    // TabPage1.Location = new System.Drawing.Point(4, 63);
    TabPage1.Name = "tabPage1";
    TabPage1.Padding = new System.Windows.Forms.Padding(3);
    TabPage1.Size = new System.Drawing.Size(787, 225);
    TabPage1.TabIndex = 0;
    TabPage1.Text = TabTitle;
    TabPage1.UseVisualStyleBackColor = true;
    TabPage1.Enter += new System.EventHandler( this.tabPage_Enter );

    MainTabControl.TabPages.Add( TabPage1 );

    EditorTabPage NewPage = new EditorTabPage( this, TabTitle, FileName, TextBox1 );
    TabPagesArray[TabPagesArrayLast] = NewPage;

    ProjectConfigFile.SetString( "RecentFile" + TabPagesArrayLast.ToString(), FileName, true );

    TabPagesArrayLast++;

    if( TabPagesArrayLast >= TabPagesArray.Length )
      {
      Array.Resize( ref TabPagesArray, TabPagesArray.Length + 16 );
      }

    // TabPages.Insert( Where, tabPage1 );

    MainTabControl.SelectedIndex = MainTabControl.TabPages.Count - 1;
    TextBox1.Select();
    TextBox1.SelectionLength = 0;
    TextBox1.SelectionStart = 0;

    }
    catch( Exception Except )
      {
      MessageBox.Show( "Exception in MainForm.AddNewPage(). " + Except.Message, MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }
    }



  private void buildToolStripMenuItem1_Click(object sender, EventArgs e)
    {
    ClearStatus();

    Cancelled = false;
    // Show the StatusTabPage:
    MainTabControl.SelectedIndex = 0;

    // SaveAllFiles();

    // BuildProj.bat
    string ProjectDirectory = ProjectConfigFile.GetString( "ProjectDirectory" );
    if( !ProjectDirectory.EndsWith( "\\" ))
      ProjectDirectory += "\\";

    string BuildBatchFileName = ProjectDirectory + "BuildProj.bat";
    Builder.StartMSBuild( BuildBatchFileName, ProjectDirectory );
    }



  private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
    if( e.KeyCode == Keys.Escape ) //  && (e.Alt || e.Control || e.Shift))
      {
      ShowStatus( "Cancelled." );
      if( Builder != null )
        {
        Builder.DisposeOfEverything();
        }

      Cancelled = true;
      }
    }




  private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
    OpenFileDialog1.Title = "Code Editor";
    OpenFileDialog1.InitialDirectory = "C:\\Eric\\"; // DataDirectory;
    OpenFileDialog1.Filter = "All files (*.*)|*.*";

    if( OpenFileDialog1.ShowDialog() != DialogResult.OK )
      return;

    string TabTitle = Path.GetFileName( OpenFileDialog1.FileName );
    string FileName = OpenFileDialog1.FileName;
    AddNewPage( TabTitle, FileName );
    }




  private void showNonAsciiToolStripMenuItem_Click(object sender, EventArgs e)
    {
    /*
    Symbols:
        General Punctuation (2000206F)
        Superscripts and Subscripts (2070209F)
        Currency Symbols (20A020CF)
        Combining Diacritical Marks for Symbols (20D020FF)
        Letterlike Symbols (2100214F)
        Number Forms (2150218F)
        Arrows (219021FF)
        Mathematical Operators (220022FF)
        Miscellaneous Technical (230023FF)
        Control Pictures (2400243F)
        Optical Character Recognition (2440245F)
        Enclosed Alphanumerics (246024FF)
        Box Drawing (2500257F)
        Block Elements (2580259F)
        Geometric Shapes (25A025FF)
        Miscellaneous Symbols (260026FF)
        Dingbats (270027BF)
        Miscellaneous Mathematical Symbols-A (27C027EF)
        Supplemental Arrows-A (27F027FF)
        Braille Patterns (280028FF)
        Supplemental Arrows-B (2900297F)
        Miscellaneous Mathematical Symbols-B (298029FF)
        Supplemental Mathematical Operators (2A002AFF)
        Miscellaneous Symbols and Arrows (2B002BFF)

    // See the MarkersDelimiters.cs file.
    // Don't exclude any characters in the Basic
    // Multilingual Plane except these Dingbat characters
    // which are used as markers or delimiters.

    //    Dingbats (270027BF)

    // for( int Count = 0x2700; Count < 0x27BF; Count++ )
      // ShowStatus( Count.ToString( "X2" ) + ") " + Char.ToString( (char)Count ));

    // for( int Count = 128; Count < 256; Count++ )
      // ShowStatus( "      case (int)'" + Char.ToString( (char)Count ) + "': return " + Count.ToString( "X4" ) + ";" );


    // for( int Count = 32; Count < 256; Count++ )
      // ShowStatus( "    CharacterArray[" + Count.ToString() + "] = '" + Char.ToString( (char)Count ) + "';  //  0x" + Count.ToString( "X2" ) );

     // &#147;

    // ShowStatus( " " );
    */

    int GetVal = 0x252F; // 0x201c;
    ShowStatus( "Character: " + Char.ToString( (char)GetVal ));
    }



  private void saveToolStripMenuItem_Click(object sender, EventArgs e)
    {
    SaveAllFiles();
    }



  private void SaveAllFiles()
    {
    if( TabPagesArrayLast < 1 )
      return;

    for( int Count = 1; Count < TabPagesArrayLast; Count++ )
      {
      TabPagesArray[Count].WriteToTextFile();
      string FileName = TabPagesArray[Count].FileName;
      ProjectConfigFile.SetString( "RecentFile" + Count.ToString(), FileName, false );
      }

    ProjectConfigFile.WriteToTextFile();
    }



  private void CloseAllFiles()
    {
    // Don't save anything automatically.
    // SaveAllFiles();

    // Dispose of text boxes, etc?
    for( int Count = 1; Count <= 20; Count++ )
      {
      ProjectConfigFile.SetString( "RecentFile" + Count.ToString(), "", false );
      }

    ProjectConfigFile.WriteToTextFile();

    for (int Count = 0; Count < TabPagesArrayLast; Count++)
      {
      // TabPagesArray[Count].DisposeOfEverything();
      // TabPagesArray[Count] = null;
      }

    MainTabControl.TabPages.Clear();
    TabPagesArrayLast = 0;

    AddStatusPage();
    }



  private void CloseCurrentFile()
    {
    try
    {
    // Don't save anything automatically.
    // SaveAllFiles();

    if( TabPagesArrayLast < 2 )
      return;

    int SelectedIndex = MainTabControl.SelectedIndex;
    if( SelectedIndex < 1 )
      return;

    if( SelectedIndex >= TabPagesArrayLast )
      return;

    // Clear all RecentFile entries.
    for( int Count = 1; Count <= 20; Count++ )
      ProjectConfigFile.SetString( "RecentFile" + Count.ToString(), "", false );

    int Where = 1;
    for( int Count = 1; Count < TabPagesArrayLast; Count++ )
      {
      if( Count == SelectedIndex )
        continue;

      string FileName = TabPagesArray[Count].FileName;
      ProjectConfigFile.SetString( "RecentFile" + Where.ToString(), FileName, false );
      Where++;
      }

    ProjectConfigFile.WriteToTextFile();

    MainTabControl.TabPages.Clear();
    TabPagesArrayLast = 0;
    AddStatusPage();
    OpenRecentFiles();
    }
    catch( Exception Except )
      {
      MessageBox.Show( "Exception in MainForm.CloseCurrentFile(): " + Except.Message, MessageBoxTitle, MessageBoxButtons.OK );
      }
    }



  private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
    Close();
    }



  private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
    CloseAllFiles();
    }



  private void saveFileAsToolStripMenuItem_Click(object sender, EventArgs e)
    {
    int SelectedIndex = MainTabControl.SelectedIndex;
    if( SelectedIndex >= TabPagesArray.Length )
      {
      MessageBox.Show( "The selected index is past the TabPagesArray length.", MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }

    // The status page is at zero.
    if( SelectedIndex <= 0 )
      {
      MessageBox.Show( "There is no tab page selected, or the status page is selected. (Top.)", MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }

    SaveFileDialog1.Title = TabPagesArray[SelectedIndex].TabTitle;
    SaveFileDialog1.InitialDirectory = "C:\\Eric\\"; // DataDirectory;
    SaveFileDialog1.Filter = "All files (*.*)|*.*";
    SaveFileDialog1.FileName = "";
    if( SaveFileDialog1.ShowDialog() != DialogResult.OK )
      return;

    TabPagesArray[SelectedIndex].TabTitle = Path.GetFileName( SaveFileDialog1.FileName );
    TabPagesArray[SelectedIndex].FileName = SaveFileDialog1.FileName;
    TabPagesArray[SelectedIndex].WriteToTextFile();
    MainTabControl.TabPages[SelectedIndex].Text = TabPagesArray[SelectedIndex].TabTitle;

    ProjectConfigFile.SetString( "RecentFile" + SelectedIndex.ToString(), TabPagesArray[SelectedIndex].FileName, true );
    }



  private void copyToolStripMenuItem_Click(object sender, EventArgs e)
    {
    TextBox SelectedBox = GetSelectedTextBox();
    if( SelectedBox == null )
      return;

    if( SelectedBox.SelectionLength < 1 )
      return;

    SelectedBox.Copy();

    // .Paste();
    // If SelectionLength is not zero this will paste
    // over (replace) the current selection.
    }



  private void cutToolStripMenuItem_Click(object sender, EventArgs e)
    {
    TextBox SelectedBox = GetSelectedTextBox();
    if( SelectedBox == null )
      return;

    if( SelectedBox.SelectionLength < 1 )
      return;

    SelectedBox.Cut();
    }



  private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
    TextBox SelectedBox = GetSelectedTextBox();
    if( SelectedBox == null )
      return;

    // if( SelectedBox.SelectionLength < 1 )
      // return;

    SelectedBox.SelectAll();
    }



  private void setCurrentProjectToolStripMenuItem_Click(object sender, EventArgs e)
    {
    OpenFileDialog1.Title = "Code Editor";
    OpenFileDialog1.InitialDirectory = "C:\\Eric\\"; // DataDirectory;
    // openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*" ;
    // openFileDialog1.FilterIndex = 2 ;
    OpenFileDialog1.Filter = "txt files (*.txt)|*.txt";

    OpenFileDialog1.CheckFileExists = false;
    OpenFileDialog1.FileName = "ProjectName.txt";

    if( OpenFileDialog1.ShowDialog() != DialogResult.OK )
      return;

    // CloseAllFiles();
    MainTabControl.TabPages.Clear();
    TabPagesArrayLast = 0;
    AddStatusPage();

    string ProjectFileName = OpenFileDialog1.FileName;

    if( ProjectFileName.Length < 4 )
      {
      MessageBox.Show( "Pick a file in the Project directory.", MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }

    MainConfigFile.SetString( "CurrentProjectFile", ProjectFileName, true );

    // Just make a new one instead of the old one.
    ProjectConfigFile = new ConfigureFile( ProjectFileName, this );

    string WorkingDir = ProjectFileName;
    WorkingDir = Path.GetDirectoryName( WorkingDir );
    ProjectConfigFile.SetString( "ProjectDirectory", WorkingDir, true );

    OpenRecentFiles();

    // string BuildBatchFileName = ProjectConfigFile.GetString( "BuildBatchFile" );

    SetShowProjectText();
    }



  private void SetShowProjectText()
    {
    string ShowS = ProjectConfigFile.GetString( "ProjectDirectory" );
    // GetFileName()
    // ChangeExtension()

    ShowS = ShowS.Replace( "C:\\Eric\\", "" );
    ShowProjectText = ShowS;
    }



  private void findToolStripMenuItem_Click(object sender, EventArgs e)
    {
    int SelectedIndex = MainTabControl.SelectedIndex;
    if( SelectedIndex >= TabPagesArray.Length )
      {
      MessageBox.Show( "No text box selected.", MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }

    if( SelectedIndex < 0 )
      {
      MessageBox.Show( "No text box selected.", MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }

    SearchForm SForm = new SearchForm();
    // try
    SForm.ShowDialog();
    if( SForm.DialogResult == DialogResult.Cancel )
      {
      SForm.FreeEverything();
      return;
      }

    SearchText = SForm.GetSearchText().Trim().ToLower();
    SForm.FreeEverything();

    if( SearchText.Length < 1 )
      {
      MessageBox.Show( "No search text entered.", MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }

    TextBox SelectedBox = TabPagesArray[SelectedIndex].MainTextBox;
    if( SelectedBox == null )
      return;

    // It has to have the focus in order to set
    // SelectionStart.
    SelectedBox.Select();

    SelectedBox.SelectionLength = 0;
    int Start = SelectedBox.SelectionStart;
    if( Start < 0 )
      Start = 0;

    string TextS = SelectedBox.Text.ToLower();
    int TextLength = TextS.Length;
    for( int Count = Start; Count < TextLength; Count++ )
      {
      if( TextS[Count] == SearchText[0] )
        {
        int Where = SearchTextMatches( Count, TextS, SearchText );
        if( Where >= 0 )
          {
          // MessageBox.Show( "Found at: " + Where.ToString(), MessageBoxTitle, MessageBoxButtons.OK );
          SelectedBox.Select();
          SelectedBox.SelectionStart = Where;
          SelectedBox.ScrollToCaret();
          return;
          }
        }
      }

    MessageBox.Show( "Nothing found.", MessageBoxTitle, MessageBoxButtons.OK );
    }




  private int SearchTextMatches( int Position, string TextToSearch, string SearchText )
    {
    int SLength = SearchText.Length;
    if( SLength < 1 )
      return -1;

    if( (Position + SLength - 1) >= TextToSearch.Length )
      return -1;

    for( int Count = 0; Count < SLength; Count++ )
      {
      if( SearchText[Count] != TextToSearch[Position + Count] )
        return -1;

      }

    return Position;
    }



  private void findNextToolStripMenuItem_Click(object sender, EventArgs e)
    {
    int SelectedIndex = MainTabControl.SelectedIndex;
    if( SelectedIndex >= TabPagesArray.Length )
      {
      MessageBox.Show( "No text box selected.", MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }

    if( SelectedIndex < 0 )
      {
      MessageBox.Show( "No text box selected.", MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }

    // SearchText = SForm.GetSearchText().Trim().ToLower();
    if( SearchText.Length < 1 )
      {
      MessageBox.Show( "No search text entered.", MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }

    TextBox SelectedBox = TabPagesArray[SelectedIndex].MainTextBox;
    if( SelectedBox == null )
      return;

    // It has to have the focus in order to set
    // SelectionStart.
    SelectedBox.Select();

    SelectedBox.SelectionLength = 0;
    int Start = SelectedBox.SelectionStart;
    if( Start < 0 )
      Start = 0;

    Start = Start + SearchText.Length;

    string TextS = SelectedBox.Text.ToLower();
    int TextLength = TextS.Length;
    for( int Count = Start; Count < TextLength; Count++ )
      {
      if( TextS[Count] == SearchText[0] )
        {
        int Where = SearchTextMatches( Count, TextS, SearchText );
        if( Where >= 0 )
          {
          // MessageBox.Show( "Found at: " + Where.ToString(), MessageBoxTitle, MessageBoxButtons.OK );
          SelectedBox.Select();
          SelectedBox.SelectionStart = Where;
          SelectedBox.ScrollToCaret();
          return;
          }
        }
      }

    MessageBox.Show( "Nothing found.", MessageBoxTitle, MessageBoxButtons.OK );
    }



  private void tabPage_Enter(object sender, EventArgs e)
    {
    int SelectedIndex = MainTabControl.SelectedIndex;
    if( SelectedIndex >= TabPagesArray.Length )
      return;

    if( SelectedIndex < 0 )
      return;

    // Get the index and then set the focus.
    TextBox SelectedBox = TabPagesArray[SelectedIndex].MainTextBox;
    SelectedBox.Select();
    }



  private void closeCurrentToolStripMenuItem_Click(object sender, EventArgs e)
    {
    CloseCurrentFile();
    }



  private void removeEmptyLinesToolStripMenuItem_Click(object sender, EventArgs e)
    {
    int SelectedIndex = MainTabControl.SelectedIndex;
    if( SelectedIndex >= TabPagesArray.Length )
      {
      MessageBox.Show( "No text box selected.", MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }

    if( SelectedIndex < 0 )
      {
      MessageBox.Show( "No text box selected.", MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }

    if( DialogResult.Yes != MessageBox.Show( "Remove blank lines?", MessageBoxTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question ))
      return;

    TabPagesArray[SelectedIndex].RemoveEmptyLines();
    }




    /*
      internal void SearchWebPagesDirectory()

      try
        {
        WebFilesDictionary.Clear();

        string[] FileEntries = Directory.GetFiles( MForm.GetWebPagesDirectory(), "*.*" );

        foreach( string FileName in FileEntries )
          {
          string ShortName = FileName.Replace( MForm.GetWebPagesDirectory(), "" );
          ShortName = ShortName.ToLower();

          if( !MForm.CheckEvents())
            return;

          }

        string [] SubDirEntries = Directory.GetDirectories( Name );
        foreach( string SubDir in SubDirEntries )
          {
          Do a recursive search through sub directories.
          ProcessOneDirectory( SubDir );
          }
        }
        catch( Exception Except )
          {
          MForm.ShowWebListenerFormStatus( "Exception in SearchWebPagesDirectory():" );
          MForm.ShowWebListenerFormStatus( Except.Message );
          }
        }
        */



  internal bool StartProgramOrFile( string FileName )
    {
    if( !File.Exists( FileName ))
      return false;

    if( ProgProcess != null )
      ProgProcess.Dispose();

    ProgProcess = new Process();
    try
    {
    ProgProcess.StartInfo.FileName = FileName;
    ProgProcess.StartInfo.Verb = ""; // "Print";
    ProgProcess.StartInfo.CreateNoWindow = false;
    ProgProcess.StartInfo.ErrorDialog = false;
    ProgProcess.Start();
    return true;
    }
    catch( Exception Except )
      {
      MessageBox.Show( "Could not start the file: \r\n" + FileName + "\r\n\r\nThe error was:\r\n" + Except.Message, MessageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Stop );
      return false;
      }
    }



  private void runWithoutDebuggingToolStripMenuItem_Click(object sender, EventArgs e)
    {
    // FileName = Path.GetFileName( FileName );
    // Path.GetDirectoryName();

    string FileName = ProjectConfigFile.GetString( "ExecutableFile" );
    // MessageBox.Show( "FileName: " + FileName, MessageBoxTitle, MessageBoxButtons.OK );

    StartProgramOrFile( FileName );
    }



  private void showLogToolStripMenuItem_Click(object sender, EventArgs e)
    {
    ClearStatus();

    string FileName = ProjectConfigFile.GetString( "ProjectDirectory" );
    if( File.Exists( FileName + "\\JavaBuild.log" ))
      FileName += "\\JavaBuild.log";
    else
      FileName += "\\msbuild.log";

    ShowStatus( "Log file: " + FileName );
    BuildLog Log = new BuildLog( FileName, this );
    Log.ReadFromTextFile();
    }



  private void setExecutableToolStripMenuItem_Click(object sender, EventArgs e)
    {
    OpenFileDialog1.Title = "Code Editor";
    OpenFileDialog1.InitialDirectory = "C:\\Eric\\"; // DataDirectory;
    // OpenFileDialog1.Filter = "exe files (*.exe)|*.exe";
    OpenFileDialog1.Filter = "All Files (*.*)|*.*";

    if( OpenFileDialog1.ShowDialog() != DialogResult.OK )
      return;

    string ExecFile = OpenFileDialog1.FileName;
    ProjectConfigFile.SetString( "ExecutableFile", ExecFile, true );
    MessageBox.Show( "Exec File: " + ProjectConfigFile.GetString( "ExecutableFile" ), MessageBoxTitle, MessageBoxButtons.OK );
    }




  private void runToolStripMenuItem_Click(object sender, EventArgs e)
    {
    // Nake a file that contains a list of the source
    // code files used in the project.
    // ProjectSource.txt
    // Use the full path.  No searching for source
    // files allowed.  They have to be explicitely
    // listed in the file.
    string FileName = "c:\\Eric\\CodeAnalysis\\bin\\Release\\CodeAnalysis.exe";
    StartProgramOrFile( FileName );
    }



  private void newFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
    string TabTitle = "No Name"; // Path.GetFileName( OpenFileDialog1.FileName );
    string FileName = "NoName.txt"; // OpenFileDialog1.FileName;
    AddNewPage( TabTitle, FileName );
    }




  private void InitializeGuiComponents()
    {
    menuStrip1 = new System.Windows.Forms.MenuStrip();
    fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    saveFileAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    closeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    closeCurrentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    buildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    buildToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
    showLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    findNextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    removeEmptyLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    runWithoutDebuggingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    projectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    setCurrentProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    setExecutableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    codeAnalysisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    runToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    showNonAsciiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
    BottomPanel = new System.Windows.Forms.Panel();
    CursorLabel = new System.Windows.Forms.Label();
    MainPanel = new System.Windows.Forms.Panel();
    MainTabControl = new System.Windows.Forms.TabControl();
    // tabPage1 = new System.Windows.Forms.TabPage();
    // TextBox1 = new System.Windows.Forms.TextBox();
    // tabPage2 = new System.Windows.Forms.TabPage();

    KeyboardTimer = new System.Windows.Forms.Timer();
    SingleInstanceTimer = new System.Windows.Forms.Timer();

    OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
    SaveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
    newFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();



      /*
      this.menuStrip1.SuspendLayout();
      this.BottomPanel.SuspendLayout();
      this.MainPanel.SuspendLayout();
      this.MainTabControl.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.SuspendLayout();
      */


      // menuStrip1.Font = new System.Drawing.Font("Segoe UI", 19.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
      menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            fileToolStripMenuItem,
            buildToolStripMenuItem,
            editToolStripMenuItem,
            debugToolStripMenuItem,
            projectToolStripMenuItem,
            codeAnalysisToolStripMenuItem,
            helpToolStripMenuItem});
      menuStrip1.Location = new System.Drawing.Point(0, 0);
      menuStrip1.Name = "menuStrip1";
      menuStrip1.Size = new System.Drawing.Size(995, 53);
      menuStrip1.TabIndex = 0;
      menuStrip1.Text = "menuStrip1";

      fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            openToolStripMenuItem,
            saveToolStripMenuItem,
            saveFileAsToolStripMenuItem,
            newFileToolStripMenuItem,
            closeAllToolStripMenuItem,
            closeCurrentToolStripMenuItem,
            exitToolStripMenuItem});
      fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      fileToolStripMenuItem.Size = new System.Drawing.Size(81, 49);
      fileToolStripMenuItem.Text = "&File";

      openToolStripMenuItem.Name = "openToolStripMenuItem";
      openToolStripMenuItem.Size = new System.Drawing.Size(301, 50);
      openToolStripMenuItem.Text = "&Open";
      openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);

      saveToolStripMenuItem.Name = "saveToolStripMenuItem";
      saveToolStripMenuItem.Size = new System.Drawing.Size(301, 50);
      saveToolStripMenuItem.Text = "Save A&ll";
      saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);

      saveFileAsToolStripMenuItem.Name = "saveFileAsToolStripMenuItem";
      saveFileAsToolStripMenuItem.Size = new System.Drawing.Size(301, 50);
      saveFileAsToolStripMenuItem.Text = "&Save File As";
      saveFileAsToolStripMenuItem.Click += new System.EventHandler(this.saveFileAsToolStripMenuItem_Click);

      closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
      closeAllToolStripMenuItem.Size = new System.Drawing.Size(301, 50);
      closeAllToolStripMenuItem.Text = "&Close All";
      closeAllToolStripMenuItem.Click += new System.EventHandler(this.closeAllToolStripMenuItem_Click);

      closeCurrentToolStripMenuItem.Name = "closeCurrentToolStripMenuItem";
      closeCurrentToolStripMenuItem.Size = new System.Drawing.Size(301, 50);
      closeCurrentToolStripMenuItem.Text = "Close C&urrent";
      closeCurrentToolStripMenuItem.Click += new System.EventHandler(this.closeCurrentToolStripMenuItem_Click);

      exitToolStripMenuItem.Name = "exitToolStripMenuItem";
      exitToolStripMenuItem.Size = new System.Drawing.Size(301, 50);
      exitToolStripMenuItem.Text = "E&xit";
      exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);

      buildToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            buildToolStripMenuItem1,
            showLogToolStripMenuItem});
      buildToolStripMenuItem.Name = "buildToolStripMenuItem";
      buildToolStripMenuItem.Size = new System.Drawing.Size(105, 49);
      buildToolStripMenuItem.Text = "&Build";

      buildToolStripMenuItem1.Name = "buildToolStripMenuItem1";
      buildToolStripMenuItem1.Size = new System.Drawing.Size(249, 50);
      buildToolStripMenuItem1.Text = "B&uild";
      buildToolStripMenuItem1.Click += new System.EventHandler(this.buildToolStripMenuItem1_Click);

      showLogToolStripMenuItem.Name = "showLogToolStripMenuItem";
      showLogToolStripMenuItem.Size = new System.Drawing.Size(249, 50);
      showLogToolStripMenuItem.Text = "Show &Log";
      showLogToolStripMenuItem.Click += new System.EventHandler(this.showLogToolStripMenuItem_Click);

      editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            copyToolStripMenuItem,
            cutToolStripMenuItem,
            selectAllToolStripMenuItem,
            findToolStripMenuItem,
            findNextToolStripMenuItem,
            removeEmptyLinesToolStripMenuItem});
      editToolStripMenuItem.Name = "editToolStripMenuItem";
      editToolStripMenuItem.Size = new System.Drawing.Size(87, 49);
      editToolStripMenuItem.Text = "&Edit";

      copyToolStripMenuItem.Name = "copyToolStripMenuItem";
      copyToolStripMenuItem.Size = new System.Drawing.Size(405, 50);
      copyToolStripMenuItem.Text = "&Copy";
      copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);

      cutToolStripMenuItem.Name = "cutToolStripMenuItem";
      cutToolStripMenuItem.Size = new System.Drawing.Size(405, 50);
      cutToolStripMenuItem.Text = "Cu&t";
      cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);

      selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
      selectAllToolStripMenuItem.Size = new System.Drawing.Size(405, 50);
      selectAllToolStripMenuItem.Text = "Select &All";
      selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);

      findToolStripMenuItem.Name = "findToolStripMenuItem";
      findToolStripMenuItem.Size = new System.Drawing.Size(405, 50);
      findToolStripMenuItem.Text = "&Find";
      findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);

      findNextToolStripMenuItem.Name = "findNextToolStripMenuItem";
      findNextToolStripMenuItem.Size = new System.Drawing.Size(405, 50);
      findNextToolStripMenuItem.Text = "Find &Next";
      findNextToolStripMenuItem.Click += new System.EventHandler(this.findNextToolStripMenuItem_Click);

      removeEmptyLinesToolStripMenuItem.Name = "removeEmptyLinesToolStripMenuItem";
      removeEmptyLinesToolStripMenuItem.Size = new System.Drawing.Size(405, 50);
      removeEmptyLinesToolStripMenuItem.Text = "Remove &Empty Lines";
      removeEmptyLinesToolStripMenuItem.Click += new System.EventHandler(this.removeEmptyLinesToolStripMenuItem_Click);

      debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runWithoutDebuggingToolStripMenuItem});
      debugToolStripMenuItem.Name = "debugToolStripMenuItem";
      debugToolStripMenuItem.Size = new System.Drawing.Size(129, 49);
      debugToolStripMenuItem.Text = "&Debug";

      runWithoutDebuggingToolStripMenuItem.Name = "runWithoutDebuggingToolStripMenuItem";
      runWithoutDebuggingToolStripMenuItem.Size = new System.Drawing.Size(462, 50);
      runWithoutDebuggingToolStripMenuItem.Text = "Run Wit&hout Debugging";
      runWithoutDebuggingToolStripMenuItem.Click += new System.EventHandler(this.runWithoutDebuggingToolStripMenuItem_Click);

      projectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            setCurrentProjectToolStripMenuItem,
            setExecutableToolStripMenuItem});
      projectToolStripMenuItem.Name = "projectToolStripMenuItem";
      projectToolStripMenuItem.Size = new System.Drawing.Size(131, 49);
      projectToolStripMenuItem.Text = "&Project";

      setCurrentProjectToolStripMenuItem.Name = "setCurrentProjectToolStripMenuItem";
      setCurrentProjectToolStripMenuItem.Size = new System.Drawing.Size(377, 50);
      setCurrentProjectToolStripMenuItem.Text = "Set &Current Project";
      setCurrentProjectToolStripMenuItem.Click += new System.EventHandler(this.setCurrentProjectToolStripMenuItem_Click);

      setExecutableToolStripMenuItem.Name = "setExecutableToolStripMenuItem";
      setExecutableToolStripMenuItem.Size = new System.Drawing.Size(377, 50);
      setExecutableToolStripMenuItem.Text = "Set &Executable";
      setExecutableToolStripMenuItem.Click += new System.EventHandler(this.setExecutableToolStripMenuItem_Click);

      codeAnalysisToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            runToolStripMenuItem});
      codeAnalysisToolStripMenuItem.Name = "codeAnalysisToolStripMenuItem";
      codeAnalysisToolStripMenuItem.Size = new System.Drawing.Size(233, 49);
      codeAnalysisToolStripMenuItem.Text = "Code &Analysis";

      runToolStripMenuItem.Name = "runToolStripMenuItem";
      runToolStripMenuItem.Size = new System.Drawing.Size(164, 50);
      runToolStripMenuItem.Text = "&Run";
      runToolStripMenuItem.Click += new System.EventHandler(this.runToolStripMenuItem_Click);

      helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            showNonAsciiToolStripMenuItem,
            aboutToolStripMenuItem});
      helpToolStripMenuItem.Name = "helpToolStripMenuItem";
      helpToolStripMenuItem.Size = new System.Drawing.Size(99, 49);
      helpToolStripMenuItem.Text = "&Help";

      showNonAsciiToolStripMenuItem.Name = "showNonAsciiToolStripMenuItem";
      showNonAsciiToolStripMenuItem.Size = new System.Drawing.Size(337, 50);
      showNonAsciiToolStripMenuItem.Text = "&Show Non-Ascii";
      showNonAsciiToolStripMenuItem.Click += new System.EventHandler(this.showNonAsciiToolStripMenuItem_Click);

      aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
      aboutToolStripMenuItem.Size = new System.Drawing.Size(337, 50);
      aboutToolStripMenuItem.Text = "&About";
      aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);

      BottomPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      BottomPanel.Controls.Add(this.CursorLabel);
      BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
      BottomPanel.Location = new System.Drawing.Point(0, 391);
      BottomPanel.Name = "BottomPanel";
      BottomPanel.Size = new System.Drawing.Size(995, 44);
      BottomPanel.TabIndex = 1;

      CursorLabel.AutoSize = true;
      CursorLabel.Location = new System.Drawing.Point(10, 2);
      CursorLabel.Name = "CursorLabel";
      CursorLabel.Size = new System.Drawing.Size(180, 56);
      CursorLabel.TabIndex = 0;
      CursorLabel.Text = "label1";

      MainPanel.Controls.Add(this.MainTabControl);
      MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      MainPanel.Location = new System.Drawing.Point(0, 53);
      MainPanel.Name = "MainPanel";
      MainPanel.Size = new System.Drawing.Size(995, 338);
      MainPanel.TabIndex = 2;

      // MainTabControl.Controls.Add(this.tabPage1);
      // MainTabControl.Controls.Add(this.tabPage2);

      MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      MainTabControl.Location = new System.Drawing.Point(0, 0);
      MainTabControl.Multiline = true;
      MainTabControl.Name = "MainTabControl";
      MainTabControl.SelectedIndex = 0;
      MainTabControl.Size = new System.Drawing.Size(995, 338);
      MainTabControl.TabIndex = 0;

     /*
      this.TextBox1.AcceptsReturn = true;
      this.TextBox1.BackColor = System.Drawing.Color.Black;
      this.TextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 28F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
      this.TextBox1.ForeColor = System.Drawing.Color.White;
      this.TextBox1.Location = new System.Drawing.Point(3, 3);
      this.TextBox1.MaxLength = 32001;
      this.TextBox1.Multiline = true;
      this.TextBox1.Name = "TextBox1";
      this.TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.TextBox1.Size = new System.Drawing.Size(981, 264);
      this.TextBox1.TabIndex = 0;
      this.TextBox1.WordWrap = false;

      this.tabPage2.Location = new System.Drawing.Point(4, 25);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(987, 309);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "tabPage2";
      this.tabPage2.UseVisualStyleBackColor = true;
*/

      KeyboardTimer.Tick += new System.EventHandler(this.KeyboardTimer_Tick);
      SingleInstanceTimer.Tick += new System.EventHandler(this.SingleInstanceTimer_Tick);

      newFileToolStripMenuItem.Name = "newFileToolStripMenuItem";
      newFileToolStripMenuItem.Size = new System.Drawing.Size(301, 50);
      newFileToolStripMenuItem.Text = "&New File";
      newFileToolStripMenuItem.Click += new System.EventHandler(this.newFileToolStripMenuItem_Click);

      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackColor = System.Drawing.Color.Black;
      this.ClientSize = new System.Drawing.Size(995, 445);
      this.Controls.Add(this.MainPanel);
      this.Controls.Add(this.BottomPanel);
      this.Controls.Add(this.menuStrip1);
      this.Font = new System.Drawing.Font("Consolas", 28.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.ForeColor = System.Drawing.Color.White;
      this.KeyPreview = true;
      this.MainMenuStrip = this.menuStrip1;
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Code Editor V2";
      this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);


      /*
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.BottomPanel.ResumeLayout(false);
      this.BottomPanel.PerformLayout();
      this.MainPanel.ResumeLayout(false);
      this.MainTabControl.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
      */
    }



  }
}



