// Programming by Eric Chauvin.
// Notes on this source code are at:
// ericlibproj.blogspot.com



using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics; // For start program process.
using System.IO;
using System.Threading;



namespace DGOLibrary
{
  public partial class MainForm : Form
  {
  internal const string VersionDate = "4/19/2016";
  internal const int VersionNumber = 09; // 0.9
  internal const string MessageBoxTitle = "Library Project";
  private System.Threading.Mutex SingleInstanceMutex = null;
  private bool IsSingleInstance = false;
  internal GetURLManagerForm GetURLMgrForm;
  internal WebListenerForm WebListenForm;
  internal WebFilesData WebFData;
  private string TempFileDirectory = "";
  // private string PagesDirectory = "";
  private string PageFilesDirectory = "";
  private string PageFilesCompressedDirectory = "";
  private string DataDirectory = "";
  private string WebPagesDirectory = "";
  private bool IsClosing = false;
  private bool Cancelled = false;
  internal GlobalProperties GlobalProps;
  // internal PageList PageList1;
  internal WordsDictionary WordsDictionary1;
  // internal ScriptDictionary ScriptDictionary1;
  // internal CodeCommentDictionary CodeCommentDictionary1;
  internal NetIPStatus NetStats;
  internal WordsData MainWordsData;
  internal URLIndex MainURLIndex;
  internal FrequencyCounter PageFrequencyCtr;
  internal PageCompress PageCompress1;
  private bool ServersAreReady = false;
  internal CharacterIndex CharIndex;
  internal HuffmanCodes HuffmanCd;


  public MainForm()
    {
    InitializeComponent();

    ///////////////////////
    // Keep this at the top:
    SetupDirectories();
    GlobalProps = new GlobalProperties( this );
    ///////////////////////

    NetStats = new NetIPStatus( this );
    NetStats.ReadFromFile();

    HuffmanCd = new HuffmanCodes( this );
    CharIndex = new CharacterIndex( this );
    PageFrequencyCtr = new FrequencyCounter( this, "FrequencyCountPage.txt" );
    PageCompress1 = new PageCompress( this );
    WordsDictionary1 = new WordsDictionary( this );
    MainWordsData = new WordsData( this );
    // ScriptDictionary1 = new ScriptDictionary( this );
    // CodeCommentDictionary1 = new CodeCommentDictionary( this );
    MainURLIndex = new URLIndex( this );
    WebFData = new WebFilesData( this );

    if( !CheckSingleInstance())
      return;

    IsSingleInstance = true;

    StartupTimer.Interval = 200;
    StartupTimer.Start();
    }



  internal void ShowStatus( string Status )
    {
    if( IsClosing )
      return;

    // Commented out for testing but it would be
    // needed on a running server.
    // if( MainTextBox.Text.Length > (80 * 5000))
      // MainTextBox.Text = "";

    MainTextBox.AppendText( Status + "\r\n" ); 
    }


  internal void AddPageFrequencyCount( string Word )
    {
    PageFrequencyCtr.AddString( Word );
    }



  private void testToolStripMenuItem_Click(object sender, EventArgs e)
    {
    /*
    // Herald page:
    // string URL = "http://www.durangoherald.com/";
    string FileName = GetPageFilesDirectory() + "TestFile.txt";
    // PageList1.UpdatePageFromFile( "Main Page", URL, FileName, true, "http://www.durangoherald.com" );

    // Durango Gov:
    // string URL = "http://www.durangogov.org/";
    // PageList1.UpdatePageFromFile( "Durango Gov Main Page", URL, FileName, true, "http://www.durangogov.org" );

    // Colorado Gov:
    string URL = "https://www.colorado.gov/";
    // PageList1.UpdatePageFromFile( "Colorado Gov Main Page", URL, FileName, true, "https://www.colorado.gov" );

    MainURLIndex.ReindexFromFile( "Colorado Gov Main Page", URL, FileName, "https://www.colorado.gov" );
    */
    }



  private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
    string ShowS = "Programming by Eric Chauvin. Version date: " + VersionDate;
    MessageBox.Show( ShowS, MessageBoxTitle, MessageBoxButtons.OK );
    }



  private void SetupDirectories()
    {
    try
    {
    TempFileDirectory = "c:\\DGOLibProject\\TempFiles\\";
    PageFilesDirectory = "c:\\DGOLibProject\\PageFiles\\";
    PageFilesCompressedDirectory = "c:\\DGOLibProject\\PageFilesCompressed\\";
    DataDirectory = Application.StartupPath + "\\Data\\";
    WebPagesDirectory = Application.StartupPath + "\\WebPages\\";

    if( !Directory.Exists( TempFileDirectory ))
      Directory.CreateDirectory( TempFileDirectory );

    if( !Directory.Exists( PageFilesDirectory ))
      Directory.CreateDirectory( PageFilesDirectory );

    if( !Directory.Exists( PageFilesCompressedDirectory ))
      Directory.CreateDirectory( PageFilesCompressedDirectory );

    if( !Directory.Exists( WebPagesDirectory ))
      Directory.CreateDirectory( WebPagesDirectory );

    if( !Directory.Exists( DataDirectory ))
      Directory.CreateDirectory( DataDirectory );

    }
    catch( Exception )
      {
      MessageBox.Show( "Error: The directory could not be created.", MessageBoxTitle, MessageBoxButtons.OK );
      return;
      }
    }


  internal string GetTempFileDirectory()
    {
    return TempFileDirectory;
    }


  internal string GetPageFilesDirectory()
    {
    return PageFilesDirectory;
    }


  internal string GetPageFilesCompressedDirectory()
    {
    return PageFilesCompressedDirectory;
    }


  internal string GetWebPagesDirectory()
    {
    return WebPagesDirectory;
    }


  internal string GetDataDirectory()
    {
    return DataDirectory;
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

    string ShowS = "Another instance of the Durango Library Project Server is already running." +
      " This instance will close.";

    try
    {
    SingleInstanceMutex = new System.Threading.Mutex( true, "Durango Library Project Single Instance", out InitialOwner );
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



  private void SingleInstanceTimer_Tick(object sender, EventArgs e)
    {
    SingleInstanceTimer.Stop();
    Application.Exit();
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
    CheckTimer.Stop();

    NetStats.SaveToFile();

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

    if( WebListenForm != null )
      {
      if( !WebListenForm.IsDisposed )
        {
        WebListenForm.Hide();
        
        // This could take a while since it's closing connections:
        WebListenForm.FreeEverythingAndStopServer();
        WebListenForm.Dispose();
        }

      WebListenForm = null;
      }

    // ShowStatus() won't show it when it's closing.
    MainTextBox.AppendText( "Saving files.\r\n" ); 
    // PageList1.WriteToTextFile();
    MainURLIndex.WriteToTextFile();

    MainTextBox.AppendText( "Saved pages.\r\n" ); 
    MainWordsData.WriteToTextFile();
    MainTextBox.AppendText( "Saved word index file.\r\n" ); 

    // Counts aren't being used here anymore.
    // WordsDictionary1.WriteToTextFile();
    // MainTextBox.AppendText( "Saved words dictionary.\r\n" ); 
    
    // ScriptDictionary1.WriteToTextFile();
    // MainTextBox.AppendText( "Saved script.\r\n" ); 

    // CodeCommentDictionary1.WriteToTextFile();
    // MainTextBox.AppendText( "Saved code comments.\r\n" ); 


    // After getting what those others show.
    SaveStatusToFile();
    }



  private void CloseServers()
    {
    CheckTimer.Stop();

    if( WebListenForm != null )
      {
      if( !WebListenForm.IsDisposed )
        {
        WebListenForm.Hide();
        
        // This could take a while since it's closing connections:
        WebListenForm.FreeEverythingAndStopServer();
        }
      }

    ShowStatus( "Closed servers." );
    }



  internal void ShowWebListenerFormStatus( string Status )
    {
    if( IsClosing )
      return;

    if( WebListenForm == null )
      return;
      
    if( WebListenForm.IsDisposed )
      return;

    WebListenForm.ShowStatus( Status ); 
    }



  internal void SaveStatusToFile()
    {
    try
    {
    string FileName = GetDataDirectory() + "MainStatus.txt";

    using( StreamWriter SWriter = new StreamWriter( FileName, false, Encoding.UTF8 ))
      {
      foreach( string Line in MainTextBox.Lines )
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



  /*
  // Be careful about what you execute from the server.
  internal bool StartProgramOrFile( string FileName )
    {
    if( !File.Exists( FileName ))
      return false;

    Process ProgProcess = new Process();

    try
    {
    ProgProcess.StartInfo.FileName = FileName;
    ProgProcess.StartInfo.Verb = ""; // "Print";
    ProgProcess.StartInfo.CreateNoWindow = false;
    ProgProcess.StartInfo.ErrorDialog = false;
    ProgProcess.Start();
    You would have to Dispose() of this ProgProcess
    when it's done.
    }
    catch( Exception Except )
      {
      MessageBox.Show( "Could not start the file: \r\n" + FileName + "\r\n\r\nThe error was:\r\n" + Except.Message, MessageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Stop );
      return false;
      }

    return true;
    }
    */


  private void showGetURLManagerToolStripMenuItem_Click(object sender, EventArgs e)
    {
    if( GetURLMgrForm == null )
      return;

    if( GetURLMgrForm.IsDisposed )
      return;

    GetURLMgrForm.Show();
    GetURLMgrForm.WindowState = FormWindowState.Normal;
    GetURLMgrForm.BringToFront();
    }


  internal bool GetServersAreReady()
    {
    return ServersAreReady;
    }



  private void StartupTimer_Tick(object sender, EventArgs e)
    {
    StartupTimer.Stop();

    ShowStatus( "Reading URL index data..." );
    // Make sure the URLIndex is loaded up before
    // GetURLMgrForm is started.
    MainURLIndex.ReadFromTextFile();

    // ShowStatus( "Reading script data..." );
    // ScriptDictionary1.ReadFromTextFile();

    // ShowStatus( "Reading code comments data..." );
    // CodeCommentDictionary1.ReadFromTextFile();

    ShowStatus( "Reading words dictionary data..." );
    WordsDictionary1.ReadFromTextFile();
    // Rewrite it so it's sorted and unique.
    WordsDictionary1.WriteToTextFile();

    // ShowStatus( "Reading All Words data..." );
    // AllWords.ReadFromTextFile();

    ShowStatus( "Reading PageCompress data..." );
    PageCompress1.ReadFromTextFile();
    // PageCompress1.ReadFromFrequencyFile();

    // Do these last.
    ShowStatus( "Starting up forms..." );
    GetURLMgrForm = new GetURLManagerForm( this );

    WebListenForm = new WebListenerForm( this );
    ReadWebFileData();
    ShowStatus( "Finished reading web files." );

    WebListenForm.StartServer();
    // TLSListenForm.StartServer();

    // CheckTimer.Interval = 15 * 60 * 1000;
    // CheckTimer.Start();

    // MainURLIndex.ShowCRCDistribution();
    ServersAreReady = true;
    ShowStatus( " " );
    ShowStatus( "After the servers were started." );
    }



  internal void ReadWebFileData()
    {
    ShowStatus( "Starting to read web file data." );
    WebFData.SearchWebPagesDirectory();
    ShowStatus( "Finished reading web file data." );
    }


  private void CheckTimer_Tick(object sender, EventArgs e)
    {
    /*
    Save files periodically on a running server.

    // Alternate which things get done each time.
    AlternateCheckTimer++;
    AlternateCheckTimer = AlternateCheckTimer & 0x7;

    if( AlternateCheckTimer == 0 )
      NetStats.SaveToFile();

    // ShowStatus( "Saving data files." );
    if( AlternateCheckTimer == 1 )
      PageList1.WriteToTextFile();

    // WordsDictionary1.WriteToTextFile();
    if( AlternateCheckTimer == 2 )
      MainWordsData.WriteToTextFile();

    if( AlternateCheckTimer == 3 )
      ScriptDictionary1.WriteToTextFile();

    if( AlternateCheckTimer == 4 )
      CodeCommentDictionary1.WriteToTextFile();

    if( AlternateCheckTimer == 5 )
      And so on...

    // ShowStatus( "Finished saving data files." );
    */
    }



  private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
    if( e.KeyCode == Keys.Escape ) //  && (e.Alt || e.Control || e.Shift))
      {
      ShowStatus( "Cancelled." );
      Cancelled = true;
      // FreeEverything(); // Stop background process.
      }
    }



  private void MakeNonAsciiCharacters()
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
    */

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
    int GetVal = 0x252F; // 0x201c;
    ShowStatus( "Character: " + Char.ToString( (char)GetVal ));
    }



  private void showUnicodeToolStripMenuItem_Click(object sender, EventArgs e)
    {
    MakeNonAsciiCharacters();
    }



  private void indexAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
    if( !GetServersAreReady())
      {
      ShowStatus( "The startup process isn't finished yet." );
      return;
      }

    //////////
    // For profiling and time testing.
    CloseServers();
    //////////

    PageFrequencyCtr.ClearAll();
    
    // Fix this compress problem!
    // ===============
    MainURLIndex.IndexAll( false ); // True: read from compressed.
    // FrequencyCtr.ShowValues( 500 );
    // FrequencyCtr.WriteToTextFile();
    // ShowStatus( "Saved the frequency file." );
    ShowStatus( "Finished indexing everything." );
    }



  private void SaveAllMidnightFiles()
    {
    if( WebListenForm != null )
      {
      if( !WebListenForm.IsDisposed )
        {
        WebListenForm.ClearDailyHackCount();
        }
      }

    NetStats.ClearMidnightValues();
    }



  private void showWebServerToolStripMenuItem_Click(object sender, EventArgs e)
    {
    if( WebListenForm == null )
      return;

    if( WebListenForm.IsDisposed )
      return;

    WebListenForm.Show();
    WebListenForm.WindowState = FormWindowState.Normal;
    WebListenForm.BringToFront();
    }



  private void showDuplicateFileNamesToolStripMenuItem_Click(object sender, EventArgs e)
    {
    MainURLIndex.ShowDuplicateFileNames();
    }



  private void compressAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
    //////////
    // For profiling and time testing.
    CloseServers();
    //////////

    MainURLIndex.CompressAllFiles();
    }



  private void fixDuplicatesToolStripMenuItem_Click(object sender, EventArgs e)
    {
    //////////
    // For profiling and time testing.
    CloseServers();
    //////////

    MainURLIndex.FindAllDuplicatePages();
    }



  }
}

