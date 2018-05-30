// Copyright Eric Chauvin 2018.
// My blog is at:
// https://scientificmodels.blogspot.com/



using System;
// using System.Collections.Generic;
using System.ComponentModel;
// using System.Data;
using System.Drawing;
using System.Text;
// using System.Threading.Tasks;
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



/*
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
*/



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

    // This opens the file, reads or writes all the
    // bytes, then closes the file.
    byte[] FileBytes = File.ReadAllBytes( FileName );
    if( FileBytes == null )
      {
      MainTextBox.Text = "FileBytes was null.";
      return false;
      }

    string FileS = UTF8Strings.BytesToString( FileBytes, 1000000000 );

    StringBuilder SBuilder = new StringBuilder();
    StringBuilder FileSBuilder = new StringBuilder();

    int Last = FileS.Length;
    for( int Count = 0; Count < Last; Count++ )
      {
      char OneChar = FileS[Count];
      if( OneChar == '\r' )
        continue; // Ignore it.

      if( OneChar == '\n' )
        {
        string Line = SBuilder.ToString();
        SBuilder.Clear();
        Line = Line.Replace( "\t", "  " );
        Line = StringsEC.GetCleanUnicodeString( Line, 4000, false );
        Line = Line.TrimEnd();

        // if( Line == "" )
          // continue;

        FileSBuilder.Append( Line + "\r\n" );
        continue;
        }

      SBuilder.Append( OneChar );
      }

    MainTextBox.Text = FileSBuilder.ToString().TrimEnd();
    return true;
    }
    catch( Exception Except )
      {
      MForm.ShowStatus( "Could not read the file: \r\n" + FileName );
      MForm.ShowStatus( Except.Message );
      return false;
      }
    }





/*
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
*/




  internal bool WriteToTextFile()
    {
    try
    {
    MForm.ShowStatus( "Saving: " + FileName );

    // if( FileName.ToLower().EndsWith( ".bat" ) ||
    //     FileName.ToLower().EndsWith( ".java" ))

    string[] Lines = MainTextBox.Lines;
    StringBuilder FileSBuilder = new StringBuilder();

    foreach( string Line in Lines )
      {
      FileSBuilder.Append( Line.TrimEnd() + "\n" );
      }

    byte[] FileBytes = UTF8Strings.StringToBytes( FileSBuilder.ToString() );
    if( FileBytes == null )
      {
      MForm.ShowStatus( "FileBytes was null for:" );
      MForm.ShowStatus( FileName );
      return false;
      }

    File.WriteAllBytes( FileName, FileBytes );
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



  }
}







