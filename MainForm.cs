// Copyright Eric Chauvin 2018.
// My blog is at:
// ericsourcecode.blogspot.com


// Microsoft Visual Studio has gotten too _helpful_,
// with the Clippy character lightbulb and all the
// other stuff flashing on the screen.  So I wanted
// to have a basic code editor without all of the
// distractions.


using System;
using System.Collections.Generic;
using System.ComponentModel;
// using System.Data;
using System.Drawing;
using System.Text;
// using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics; // For starting a program/process.
using System.IO;



namespace CodeEditor
{
  public partial class MainForm : Form
  {
  internal const string VersionDate = "1/31/2018";
  internal const int VersionNumber = 09; // 0.9
  // private System.Threading.Mutex SingleInstanceMutex = null;
  // private bool IsSingleInstance = false;
  private bool IsClosing = false;
  private bool Cancelled = false;
  private EditorTabPage[] TabPagesArray;
  private int TabPagesArrayLast = 0;
  internal const string MessageBoxTitle = "Code Editor";
  private string DataDirectory = "";
  private TextBox StatusTextBox;
  private MSBuilder Builder;
  private CSharpCompiler CSCompiler;
  private ConfigureFile ConfigFile;
  private string CurrentProjectText = "";
  private string SearchText = "";
  private Process ProgProcess;
  private float MainTextFontSize = 34.0F;



  public MainForm()
    {
    InitializeComponent();

    // if( !CheckSingleInstance())
      // return;

    // IsSingleInstance = true;

    ///////////
    // Keep this at the top.
    SetupDirectories();
    ConfigFile = new ConfigureFile( DataDirectory + "Config.txt" ); // , this );
    ///////////

    // ConfigFile.SetString( "CurrentProject", "C:\\Eric\\ClimateModel\\ClimateModel.csproj" );
    // ConfigFile.SetString( "ProjectDirectory", "C:\\Eric\\ClimateModel\\" );

    string ShowS = Path.GetFileName( ConfigFile.GetString( "CurrentProject" ));
    ShowS = ShowS.Replace( ".csproj", "" );
    CurrentProjectText = ShowS;

    // this.Font = new System.Drawing.Font("Microsoft Sans Serif", 40F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
    this.Font = new System.Drawing.Font( "Consolas", 28.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
    this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));

    TabPagesArray = new EditorTabPage[2];
    MainTabControl.TabPages.Clear();

    StatusTextBox = new TextBox();
    AddStatusPage();
    OpenRecentFiles();

    ShowStatus( "Version date: " + VersionDate );
    // MessageBox.Show( "Test this.", MessageBoxTitle, MessageBoxButtons.OK);

    Builder = new MSBuilder( this );
    CSCompiler = new CSharpCompiler( this );
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



  /*
  private bool CheckSingleInstance()
    {
    bool InitialOwner = false; // Owner for single instance check.
    string ShowS = "Another instance of the Code Editor is already running." +
      " This instance will close.";

    try
    {
    SingleInstanceMutex = new System.Threading.Mutex( true, "Eric's Code Editor Single Instance", out InitialOwner );
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
    */



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
    /*
    if( IsSingleInstance )
      {
      if( DialogResult.Yes != MessageBox.Show( "Close the program?", MessageBoxTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question ))
        {
        e.Cancel = true;
        return;
        }
      }
      */

    IsClosing = true;
    KeyboardTimer.Stop();
    BuildTimer.Stop();

    // if( IsSingleInstance )
      // {
      // SaveAllFiles();
      DisposeOfEverything();
      // }


    /*
    if( GetURLMgrForm != null )
      {
      if( !GetURLMgrForm.IsDisposed )
        {
        GetURLMgrForm.Hide();
        GetURLMgrForm.FreeEverything();
        GetURLMgrForm.Dispose();
        }

      GetURLMgrForm = null;
      }
      */

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
    CursorLabel.Text = "Line: " + Line.ToString("N0") + "     " + TabTitle + "      Proj: " + CurrentProjectText;

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



  private void DisposeOfEverything()
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

    if( CSCompiler != null )
      CSCompiler.DisposeOfEverything();

    // Dispose of TextBoxes, etc?
    // Does the TabControl own these components?

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
    }
    catch( Exception Except )
      {
      MessageBox.Show( "Exception in MainForm.DisposeOfEverything(). " + Except.Message, MessageBoxTitle, MessageBoxButtons.OK);
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
    TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
    TextBox1.WordWrap = false;
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
      string FileName = ConfigFile.GetString( "RecentFile" + Count.ToString() );
      if( FileName.Length < 1 )
        break;

      string TabTitle = Path.GetFileName( FileName );
      AddNewPage( TabTitle, FileName );
      }
    }



  private void AddNewPage( string TabTitle, string FileName )
    {
    try
    {
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

    ConfigFile.SetString( "RecentFile" + TabPagesArrayLast.ToString(), FileName, true );

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

    string ProjectFileName = "C:\\Eric\\ClimateModel\\BuildProj.bat";
    // string ProjectFileName = ConfigFile.GetString( "CurrentProject" );
    string ProjectDirectory = ConfigFile.GetString( "ProjectDirectory" );
    Builder.StartMSBuild( ProjectFileName, ProjectDirectory );

    BuildTimer.Interval = 500;
    BuildTimer.Start();
    }




  private void BuildTimer_Tick(object sender, EventArgs e)
    {
    /*
    try
    {
    // ShowStatus( "Build Timer." );
    if( Builder == null )
      return;

    if( Cancelled )
      {
      BuildTimer.Stop();
      Builder.DisposeOfEverything();
      return;
      }

    if( Builder.IsMSBuildFinished())
      {
      BuildTimer.Stop();
      ShowStatus( "Build finished." );
      return;
      }
    //////////

    // ShowStatus( "Build is not finished." );
    }
    catch( Exception Except )
      {
      ShowStatus( "Exception in MainForm.BuildTimer_Tick(). " + Except.Message );
      return;
      }
    */
    }



  private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
    if( e.KeyCode == Keys.Escape ) //  && (e.Alt || e.Control || e.Shift))
      {
      ShowStatus( "Cancelled." );
      if( Builder != null )
        {
        CSCompiler.ShowCompileLines();
        // Builder.ShowMSBuildLines();
        Builder.DisposeOfEverything();
        }

      Cancelled = true;
      }
    }




  private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
    OpenFileDialog1.Title = "Code Editor";
    OpenFileDialog1.InitialDirectory = "C:\\Eric\\"; // DataDirectory;

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
        General Punctuation (2000–206F)
        Superscripts and Subscripts (2070–209F)
        Currency Symbols (20A0–20CF)
        Combining Diacritical Marks for Symbols (20D0–20FF)
        Letterlike Symbols (2100–214F)
        Number Forms (2150–218F)
        Arrows (2190–21FF)
        Mathematical Operators (2200–22FF)
        Miscellaneous Technical (2300–23FF)
        Control Pictures (2400–243F)
        Optical Character Recognition (2440–245F)
        Enclosed Alphanumerics (2460–24FF)
        Box Drawing (2500–257F)
        Block Elements (2580–259F)
        Geometric Shapes (25A0–25FF)
        Miscellaneous Symbols (2600–26FF)
        Dingbats (2700–27BF)
        Miscellaneous Mathematical Symbols-A (27C0–27EF)
        Supplemental Arrows-A (27F0–27FF)
        Braille Patterns (2800–28FF)
        Supplemental Arrows-B (2900–297F)
        Miscellaneous Mathematical Symbols-B (2980–29FF)
        Supplemental Mathematical Operators (2A00–2AFF)
        Miscellaneous Symbols and Arrows (2B00–2BFF)

    // See the MarkersDelimiters.cs file.
    // Don't exclude any characters in the Basic
    // Multilingual Plane except these Dingbat characters
    // which are used as markers or delimiters.

    //    Dingbats (2700–27BF)

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
      ConfigFile.SetString( "RecentFile" + Count.ToString(), FileName, false );
      }

    ConfigFile.WriteToTextFile();
    }



  private void CloseAllFiles()
    {
    // Don't save anything automatically.
    // SaveAllFiles();

    // Dispose of text boxes, etc?
    for( int Count = 1; Count <= 20; Count++ )
      {
      ConfigFile.SetString( "RecentFile" + Count.ToString(), "", false );
      }

    ConfigFile.WriteToTextFile();

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
      ConfigFile.SetString( "RecentFile" + Count.ToString(), "", false );

    int Where = 1;
    for( int Count = 1; Count < TabPagesArrayLast; Count++ )
      {
      if( Count == SelectedIndex )
        continue;

      string FileName = TabPagesArray[Count].FileName;
      ConfigFile.SetString( "RecentFile" + Where.ToString(), FileName, false );
      Where++;
      }

    ConfigFile.WriteToTextFile();

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
    SaveFileDialog1.InitialDirectory = DataDirectory;

    if( SaveFileDialog1.ShowDialog() != DialogResult.OK )
      return;

    TabPagesArray[SelectedIndex].TabTitle = Path.GetFileName( SaveFileDialog1.FileName );
    TabPagesArray[SelectedIndex].FileName = SaveFileDialog1.FileName;
    TabPagesArray[SelectedIndex].WriteToTextFile();
    MainTabControl.TabPages[SelectedIndex].Text = TabPagesArray[SelectedIndex].TabTitle;

    ConfigFile.SetString( "RecentFile" + SelectedIndex.ToString(), TabPagesArray[SelectedIndex].FileName, true );
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

    if( OpenFileDialog1.ShowDialog() != DialogResult.OK )
      return;

    string ProjectFile = OpenFileDialog1.FileName;
    string WorkingDir = Path.GetDirectoryName( ProjectFile );
    ConfigFile.SetString( "CurrentProject", ProjectFile, true );
    ConfigFile.SetString( "ProjectDirectory", WorkingDir, true );

    // MessageBox.Show( "Project Directory: " + ConfigFile.GetString( "ProjectDirectory" ), MessageBoxTitle, MessageBoxButtons.OK );

    string ShowS = Path.GetFileName( ConfigFile.GetString( "CurrentProject" ));
    ShowS = ShowS.Replace( ".bat", "" );
    CurrentProjectText = ShowS;
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
      return;

    SearchText = SForm.GetSearchText().Trim().ToLower();
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



  private void CompileTimer_Tick(object sender, EventArgs e)
    {
    try
    {
    // ShowStatus( "Build Timer." );
    if( CSCompiler == null )
      return;

    if( Cancelled )
      {
      CompileTimer.Stop();
      CSCompiler.DisposeOfEverything();
      return;
      }

    if( CSCompiler.IsCompileFinished())
      {
      CompileTimer.Stop();
      ShowStatus( "Compile finished." );
      return;
      }

    }
    catch( Exception Except )
      {
      ShowStatus( "Exception in MainForm.CompileTimer_Tick(). " + Except.Message );
      return;
      }
    }



  private void compileCurrentFileToolStripMenuItem_Click_1(object sender, EventArgs e)
    {
    ClearStatus();
    Cancelled = false;

    int SelectedIndex = MainTabControl.SelectedIndex;
    if( SelectedIndex >= TabPagesArray.Length )
      return;

    if( SelectedIndex < 1 )
      return;

    string FileName = TabPagesArray[SelectedIndex].FileName;

    // Show the StatusTabPage:
    MainTabControl.SelectedIndex = 0;

    // SaveAllFiles();

    // string FileName = ConfigFile.GetString( "CurrentProject" );
    string ProjectDirectory = ConfigFile.GetString( "ProjectDirectory" );
    CSCompiler.StartCSharp( FileName, ProjectDirectory );

    CompileTimer.Interval = 500;
    CompileTimer.Start();
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
    string WorkingDir = ConfigFile.GetString( "ProjectDirectory" );
    string FileName = ConfigFile.GetString( "CurrentProject" );
    FileName = Path.GetFileName( FileName );
    // Path.GetDirectoryName();
    FileName = WorkingDir + "\\bin\\Release\\" + FileName;
    FileName = FileName.Replace( ".csproj", ".exe" );
    // MessageBox.Show( "FileName: " + FileName, MessageBoxTitle, MessageBoxButtons.OK );
    
    StartProgramOrFile( FileName );
    }



  private void showLogToolStripMenuItem_Click(object sender, EventArgs e)
    {
    ClearStatus();

    string FileName = ConfigFile.GetString( "ProjectDirectory" );
    FileName += "\\msbuild.log";
    BuildLog Log = new BuildLog( FileName, this );
    Log.ReadFromTextFile();
    }



  }
}



