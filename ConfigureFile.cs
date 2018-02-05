// Copyright Eric Chauvin 2018.
// My blog is at:
// ericsourcecode.blogspot.com



using System;
using System.Text;
// using System.Windows.Forms; // Application.
using System.Collections.Generic;
// using System.Threading.Tasks;
using System.IO;


namespace CodeEditor
{
  class ConfigureFile
  {
  private MainForm MForm;
  private Dictionary<string, string> CDictionary;
  private string FileName;
  // private AESEncryption AESEncrypt;


  private ConfigureFile()
    {
    }



  internal ConfigureFile( string FileToUseName, MainForm UseForm )
    {
    MForm = UseForm;

    FileName = FileToUseName;
    // AESEncrypt = new AESEncryption();
    // string ExampleKey = "Where does this key come from? 
    // AESEncrypt.SetKey( ExampleKey );

    CDictionary = new Dictionary<string, string>();
    ReadFromTextFile();
    }



  internal string GetString( string KeyWord )
    {
    KeyWord = KeyWord.ToLower().Trim();

    string Value;
    if( CDictionary.TryGetValue( KeyWord, out Value ))
      return Value;
    else
      return "";

    }



  internal void SetString( string KeyWord, string Value, bool WriteFile )
    {
    KeyWord = KeyWord.ToLower().Trim();

    if( KeyWord == "" )
      {
      MForm.ShowStatus( "Can't add an empty keyword to the dictionary in ConfigureFile.cs." );
      return;
      }

    CDictionary[KeyWord] = Value;

    if( WriteFile )
      WriteToTextFile();

    }



  private bool ReadFromTextFile()
    {
    // AESEncryption AESEncrypt = null;

    CDictionary.Clear();
    if( !File.Exists( FileName ))
      return false;

    try
    {
    using( StreamReader SReader = new StreamReader( FileName, Encoding.UTF8 ))
      {
      while( SReader.Peek() >= 0 ) 
        {
        string Line = SReader.ReadLine();
        if( Line == null )
          continue;

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



  internal bool WriteToTextFile()
    {
    try
    {
    using( StreamWriter SWriter = new StreamWriter( FileName, false, Encoding.UTF8 ))
      {
      foreach( KeyValuePair<string, string> Kvp in CDictionary )
        {
        string Line = Kvp.Key + "\t" + Kvp.Value;
        // Line = AESEncrypt.EncryptString( Line );
        SWriter.WriteLine( Line );
        }

      SWriter.WriteLine( " " );
      }

    return true;
    }
    catch( Exception Except )
      {
      MForm.ShowStatus( "Could not write the configuration data to the file." );
      MForm.ShowStatus( Except.Message );
      return false;
      }
    }



  /*
  private int GetIntegerValue( string Key )
    {
    try
    {
    return Int32.Parse( Config.GetString( Key ));
    }
    catch( Exception ) // Except )
      {
      // MForm.ShowStatus( "Exception in GetIntegerValue():" );
      // MForm.ShowStatus( Except.Message );
      return -1;
      }
    }
    */


  /*
  private void SetIntegerValue( string Key, int ToSet )
    {
    SetString( Key, ToSet.ToString() );
    WriteToTextFile();
    }
    */



  }
}
