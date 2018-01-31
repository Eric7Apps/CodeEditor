// Copyright Eric Chauvin 2018.
// My blog is at:
// ericsourcecode.blogspot.com


using System;
// using System.Collections.Generic;
using System.Text;
// using System.Threading.Tasks;
using System.IO;


namespace CodeEditor
{
  class BuildLog
  {
  private MainForm MForm;
  // private Dictionary<string, string> CDictionary;
  private string FileName;

  private BuildLog()
    {
    }



  internal BuildLog( string FileToUseName, MainForm UseForm )
    {
    MForm = UseForm;
    FileName = FileToUseName;
    }



  internal bool ReadFromTextFile()
    {
    // Maybe do more with this in the future, like
    // parse it.

    // CDictionary.Clear();
    if( !File.Exists( FileName ))
      {
      MForm.ShowStatus( "The file doesn't exist." );
      MForm.ShowStatus( FileName );
      return false;
      }

    try
    {
    using( StreamReader SReader = new StreamReader( FileName, Encoding.UTF8 ))
      {
      while( SReader.Peek() >= 0 ) 
        {
        string Line = SReader.ReadLine();
        if( Line == null )
          continue;

        MForm.ShowStatus( Line );

        /*
        Line = Line.Trim();
        if( Line == "" )
          continue;

        // if( !Line.Contains( "\t" ))
          // Line = AESEncrypt.DecryptString( Line );

        if( !Line.Contains( "\t" ))
          continue;

        string[] SplitString = Line.Split( new Char[] { '\t' } );

        if( SplitString.Length < 2 )
          continue;

        string KeyWord = SplitString[0].Trim();
        string Value = SplitString[1].Trim();
        KeyWord = KeyWord.Replace( "\"", "" );
        Value = Value.Replace( "\"", "" );

        if( KeyWord == "" )
          continue;

        CDictionary[KeyWord] = Value;
        // try
        // CDictionary.Add( KeyWord, Value );
        */
        }
      }

    return true;
    }
    catch( Exception Except )
      {
      MForm.ShowStatus( "Could not read the file: \r\n" + FileName );
      MForm.ShowStatus( Except.Message );
      return false;
      }
    }



  }
}
