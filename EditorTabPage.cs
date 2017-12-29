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
    ReadFromTextFile( FileName );
    }


  private bool ReadFromTextFile( string FileName )
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

        Line = Line.TrimEnd(); // TrimStart()
        // if( Line == "" )
          // continue;

        Line = Line.Replace( "\t", "  " );

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
