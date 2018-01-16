// Programming by Eric Chauvin.
// Notes on this source code are at:
// ericsourcecode.blogspot.com

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



namespace CodeEditor
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

        if( AsciiOnly )
          Line = CleanAsciiString( Line, 4096 );

        Line = Line.Replace( "\t", "  " );
        Line = Line.TrimEnd(); // TrimStart()
        // if( Line == "" )
          // continue;

        SBuilder.Append( Line + "\r\n" );
        }
      }

    MainTextBox.Text = SBuilder.ToString();
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



  internal string CleanAsciiString( string InString, int MaxLength )
    {
    if( InString == null )
      return "";

    StringBuilder SBuilder = new StringBuilder();

    for( int Count = 0; Count < InString.Length; Count++ )
      {
      if( Count >= MaxLength )
        break;

      if( InString[Count] > 127 )
        continue; // Don't want this character.

      if( InString[Count] < ' ' )
        continue; // Space is lowest ASCII character.

      SBuilder.Append( Char.ToString( InString[Count] ) );
      }

    string Result = SBuilder.ToString();
    // Result = Result.Replace( "\"", "" );
    return Result;
    }



  internal bool WriteToTextFile()
    {
    try
    {
    MForm.ShowStatus( "Saving: " + FileName );

    using( StreamWriter SWriter = new StreamWriter( FileName, false, Encoding.UTF8 ))
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
