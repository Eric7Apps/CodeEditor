// Programming by Eric Chauvin.
// Notes on this source code are at:
// ericsourcecode.blogspot.com

// Make a drop down list of files to choose from
// instead of using OpenDialog.

// Use UTF8 to save (but not open/read) the source
// code files.  Test it with a Java source code file.
// Check UTF8 thoroughly.  Is it secure?  Valid?


using System;
using System.Collections.Generic;
using System.ComponentModel;
// using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// using System.Diagnostics; // For start program process.
using System.IO;
using System.Threading;



namespace CodeEditor2
{
  class EditorTabPage
  {
  private MainForm MForm;
  internal string FileName = "";
  internal string TabTitle = "";
  internal TextBox MainTextBox;
  // internal int SearchPosition = -1;



  private EditorTabPage()
    {
    }



  internal EditorTabPage( MainForm UseForm, string SetTabTitle, string SetFileName, TextBox SetTextBox )
    {
    MForm = UseForm;
    FileName = SetFileName;
    TabTitle = SetTabTitle;
    MainTextBox = SetTextBox;
    ReadFromTextFile( FileName, true );
    }


  private bool ReadFromTextFile( string FileName, bool AsciiOnly )
    {
    try
    {
    if( !File.Exists( FileName ))
      {
      // Might be adding a new file that doesn't
      // exist yet.
      MainTextBox.Text = "";
      return false;
      }

    StringBuilder SBuilder = new StringBuilder();
    using( StreamReader SReader = new StreamReader( FileName, Encoding.UTF8 ))
      {
      while( SReader.Peek() >= 0 )
        {
        string Line = SReader.ReadLine();
        if( Line == null )
          continue;

        Line = Line.Replace( "\t", "  " );
        Line = StringsEC.GetCleanUnicodeString( Line, 4000, false );
        Line = Line.TrimEnd(); // TrimStart()

        // if( Line == "" )
          // continue;

        SBuilder.Append( Line + "\r\n" );
        }
      }

    MainTextBox.Text = SBuilder.ToString().TrimEnd();
    return true;
    }
    catch( Exception Except )
      {
      MForm.ShowStatus( "Could not read the file: \r\n" + FileName );
      MForm.ShowStatus( Except.Message );
      return false;
      }
    }



  internal void RemoveEmptyLines()
    {
    try
    {
    StringBuilder SBuilder = new StringBuilder();
    foreach( string Line in MainTextBox.Lines )
      {
      if( Line == null )
        continue;

      string TestS = Line.Trim();
      if( TestS.Length < 1 )
        continue;

      SBuilder.Append( Line + "\r\n" );
      }

    MainTextBox.Text = SBuilder.ToString();
    }
    catch( Exception Except )
      {
      MForm.ShowStatus( "Exception in RemoveEmptyLines():" );
      MForm.ShowStatus( Except.Message );
      }
    }



  internal bool WriteToTextFile()
    {
    try
    {
    MForm.ShowStatus( "Saving: " + FileName );

    Encoding Encode = Encoding.UTF8;
    if( FileName.ToLower().EndsWith( ".bat" ) ||
        FileName.ToLower().EndsWith( ".java" ))
      {
      MForm.ShowStatus( "Using Ascii encoding." );
      Encode = Encoding.ASCII;
      }

    using( StreamWriter SWriter = new StreamWriter( FileName, false, Encode ))
      {
      string[] Lines = MainTextBox.Lines;

      foreach( string Line in Lines )
        {
        // SWriter.WriteLine( Line.TrimEnd() + "\r\n" );
        SWriter.WriteLine( Line.TrimEnd() );
        }

      // SWriter.WriteLine( " " );
      }

    return true;
    }
    catch( Exception Except )
      {
      MForm.ShowStatus( "Could not write to the file:" );
      MForm.ShowStatus( FileName );
      MForm.ShowStatus( Except.Message );
      return false;
      }
    }



  }
}



