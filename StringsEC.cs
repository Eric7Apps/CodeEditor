// Copyright Eric Chauvin 2018.
// My blog is at:
// ericsourcecode.blogspot.com


using System;
using System.Collections.Generic;
using System.Text;
// using System.Threading.Tasks;



namespace CodeEditor2
{
  static class StringsEC
  {

  internal static string CleanAsciiString( string InString, int MaxLength )
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



  internal static string TruncateString( string InString, int HowLong )
    {
    if( InString.Length <= HowLong )
      return InString;

    return InString.Remove( HowLong );
    }




  // You could use Base64 instead.
  internal static string BytesToLetterString( byte[] InBytes )
    {
    StringBuilder SBuilder = new StringBuilder();
    for( int Count = 0; Count < InBytes.Length; Count++ )
      {
      uint ByteHigh = InBytes[Count];
      uint ByteLow = ByteHigh & 0x0F;
      ByteHigh >>= 4;
      SBuilder.Append( (char)('A' + (char)ByteHigh) );
      SBuilder.Append( (char)('A' + (char)ByteLow) );

      // MForm.ShowStatus( SBuilder.ToString() );
      }

    return SBuilder.ToString();
    }




  private static bool IsInLetterRange( uint Letter )
    {
    const uint MaxLetter = (uint)('A') + 15;
    const uint MinLetter = (uint)'A';

    if( Letter > MaxLetter )
      {
      // MForm.ShowStatus( "Letter > MaxLetter" );
      return false;
      }

    if( Letter < MinLetter )
      {
      // MForm.ShowStatus( "Letter < MinLetter" );
      return false;
      }

    return true;
    }




  internal static byte[] LetterStringToBytes( string InString )
    {
    try
    {
    if( InString == null )
      return null;

    if( InString.Length < 2 )
      return null;

    byte[] OutBytes;

    try
    {
    OutBytes = new byte[InString.Length >> 1];
    }
    catch( Exception )
      {
      return null;
      }

    int Where = 0;
    for( int Count = 0; Count < OutBytes.Length; Count++ )
      {
      uint Letter = InString[Where];
      if( !IsInLetterRange( Letter ))
        return null;

      uint ByteHigh = Letter - (uint)'A';
      ByteHigh <<= 4;
      Where++;
      Letter = InString[Where];
      if( !IsInLetterRange( Letter ))
        return null;

      uint ByteLow = Letter - (uint)'A';
      Where++;

      OutBytes[Count] = (byte)(ByteHigh | ByteLow);
      }

    return OutBytes;
    }
    catch( Exception )
      {
      return null;
      }
    }



  internal static string GetCleanUnicodeString( string InString, int HowLong, bool TrimIt )
    {
    if( InString == null )
      return "";

    // This is the maximum length before it's cleaned
    // up.  But that's about the same as the resulting
    // length if it's already clean.  (Minus tabs, CR,
    // LF.)  It is normally just a reasonable maximum
    // limit for user input.

    if( InString.Length > HowLong )
      InString = InString.Remove( HowLong );

    StringBuilder SBuilder = new StringBuilder();
    for( int Count = 0; Count < InString.Length; Count++ )
      {
      char ToCheck = InString[Count];

      // Replace tabs and CR LF with spaces.
      if( ToCheck < ' ' )
        ToCheck = ' ';

      //  Don't go higher than D800 (Surrogates).
      if( ToCheck >= 0xD800 )
        ToCheck = ' ';

      // Don't exclude any characters in the Basic
      // Multilingual Plane except what are called
      // the "Dingbat" characters which are used as
      // markers or delimiters, and they should not
      // be in this text.
      if( (ToCheck >= 0x2700) && (ToCheck <= 0x27BF))
        ToCheck = ' ';

      // Basic Multilingual Plane
      // C0 Controls and Basic Latin (Basic Latin)
      //                 (0000007F)
      // C1 Controls and Latin-1 Supplement (008000FF)
      // Latin Extended-A (0100017F)
      // Latin Extended-B (0180024F)
      // IPA Extensions (025002AF)
      // Spacing Modifier Letters (02B002FF)
      // Combining Diacritical Marks (0300036F)
      // General Punctuation (2000206F)
      // Superscripts and Subscripts (2070209F)
      // Currency Symbols (20A020CF)
      // Combining Diacritical Marks for Symbols (20D020FF)
      // Letterlike Symbols (2100214F)
      // Number Forms (2150218F)
      // Arrows (219021FF)
      // Mathematical Operators (220022FF)
      // Box Drawing (2500257F)
      // Geometric Shapes (25A025FF)
      // Miscellaneous Symbols (260026FF)
      // Dingbats (270027BF)
      // Miscellaneous Symbols and Arrows (2B002BFF)
      // Control characters.

      if( (ToCheck >= 127) && (ToCheck <= 160))
        ToCheck = ' ';

      // Control character?
      if( ToCheck == 173 )
        ToCheck = ' ';

      SBuilder.Append( Char.ToString( ToCheck ));
      }

    string Result = SBuilder.ToString();
    if( TrimIt )
      Result = Result.Trim();

    return Result;
    }




  internal static char AsciiLowerCase( char InChar )
    {
    // int AsciiDif = (int)'A' - (int)'a';
    int AsciiDif = (int)'a' - (int)'A';
    if( AsciiDif < 0 )
      return '?';

      // throw( new Exception( "AsciiDif < 0" ));

    if( InChar < 'A' )
      return InChar;

    if( InChar > 'Z' )
      return InChar;

    return (char)((int)InChar + AsciiDif);
    }




  internal static bool MatchesTestString( int Where, string InString, string MatchS )
    {
    try
    {
    int MatchLength = MatchS.Length;
    int InSLength = InString.Length;

    if( InSLength < MatchLength )
      return false;

    if( Where < 0 )
      return false;

    for( int Count = 0; Count < MatchLength; Count++ )
      {
      if( (Where + Count) >= InSLength )
        return false;

      // ToLower() so it matches something like <ScRipT
      // if( Char.ToLower( InString[Where + Count] ) != MatchS[Count] )
        // return false;

      if( AsciiLowerCase( InString[Where + Count] ) != MatchS[Count] )
        return false;

      }

    return true;
    }
    catch( Exception Except )
      {
      string ShowS = "Exception in Utility.MatchesTestString().\r\n" +
        Except.Message;

      throw( new Exception( ShowS ));
      }
    }




  internal static string RemovePatternFromStartToEnd( string StartS, string EndS, string InString )
    {
    try
    {
    if( InString == null )
      return "";

    if( InString.Length < StartS.Length )
      return InString;

    if( InString.Length < EndS.Length )
      return InString;

    StringBuilder SBuilder = new StringBuilder();
    bool IsInside = false;
    int EndSLength = EndS.Length;
    for( int Count = 0; Count < InString.Length; Count++ )
      {
      // When this is looking for <span at the beginning
      // and > at the end, it can match both at the
      // same time with the ><span pattern.
      // So put if( IsInside ) first.
      if( IsInside )
        {                  //      /script>
        if( MatchesTestString( Count - EndSLength, InString, EndS ))
          IsInside = false;

        }

      if( !IsInside )
        {
        if( MatchesTestString( Count, InString, StartS ))
          IsInside = true;

        }

      if( !IsInside )
        SBuilder.Append( InString[Count] );

      }

    return SBuilder.ToString();
    }
    catch( Exception Except )
      {
      return "Exception in Utility.RemovePatternFromStartToEnd().\r\n" +
        Except.Message;

      }
    }




  internal static SortedDictionary<string, int> GetPatternsFromStartToEnd( string StartS, string EndS, string InString )
    {
    try
    {
    SortedDictionary<string, int> LinesDictionary = new SortedDictionary<string, int>();

    StringBuilder SBuilder = new StringBuilder();
    bool IsInside = false;
    for( int Count = 0; Count < InString.Length; Count++ )
      {
      // Put if( IsInside ) first.

      if( IsInside )
        {                  //      /script>
        if( MatchesTestString( Count - EndS.Length, InString, EndS ))
          {
          IsInside = false;
          string Line = SBuilder.ToString().Trim().ToLower();
          SBuilder.Clear();
          LinesDictionary[Line] = 1;
          }
        }

      if( !IsInside )
        {
        if( MatchesTestString( Count, InString, StartS ))
          IsInside = true;

        }

      if( IsInside )
        SBuilder.Append( InString[Count] );

      }

    return LinesDictionary;
    }
    catch( Exception ) // Except )
      {
      return null;
      }
    }



  internal static bool StringHasALetter( string InString )
    {
    for( int Count = 0; Count < InString.Length; Count++ )
      {
      if( IsALetter( InString[Count] ))
        return true;

      }

    return false;
    }




  internal static bool IsALetter( char Letter )
    {
    // What exactly _is_ a letter?
    // It's anything that's not listed here.
    if( Char.IsDigit( Letter ))
      return false;

    if( Letter == ' ' )
      return false;

    if( Letter == '=' )
      return false;

    // if( Letter == '' )
      // return false;

    if( Letter == '&' )
      return false;

    if( Letter == '@' )
      return false;

    if( Letter == '\r' )
      return false;

    if( Letter == '"' )
      return false;

    if( Letter == ':' )
      return false;

    if( Letter == ';' )
      return false;

    if( Letter == '.' )
      return false;

    if( Letter == ',' )
      return false;

    if( Letter == '-' )
      return false;

    if( Letter == '_' )
      return false;

    if( Letter == '!' )
      return false;

    if( Letter == '?' )
      return false;

    if( Letter == '&' )
      return false;

    if( Letter == '#' )
      return false;

    if( Letter == '*' )
      return false;

    if( Letter == '+' )
      return false;

    if( Letter == '(' )
      return false;

    if( Letter == ')' )
      return false;

    if( Letter == '[' )
      return false;

    if( Letter == ']' )
      return false;

    if( Letter == '{' )
      return false;

    if( Letter == '}' )
      return false;

    if( Letter == '<' )
      return false;

    if( Letter == '>' )
      return false;

    if( Letter == '|' )
      return false;

    if( Letter == '\\' )
      return false;

    if( Letter == '/' )
      return false;

    return true;
    }




  internal static bool ContainsNonASCII( string Word )
    {
    for( int Count = 0; Count < Word.Length; Count++ )
      {
      if( Word[Count] > '~' )
        return true;

      }

    return false;
    }




  internal static int GetFirstNonASCII( string Word )
    {
    for( int Count = 0; Count < Word.Length; Count++ )
      {
      if( Word[Count] > '~' )
        {
        int Result = Word[Count];
        return Result;
        }
      }

    return -1;
    }



  /*
  // This is a Cyclic Redundancy Check (CRC) function.
  // CCITT is the international standards body.
  // This CRC function is translated from a magazine
  // article in Dr. Dobbs Journal.
  // By Bob Felice, June 17, 2007
  // But this is my C# translation of what was in that
  // article.  (It was written in C.)
  internal static uint GetCRC16( string InString )
    {
    // Different Polynomials can be used.
    uint Polynomial = 0x8408;
    uint crc = 0xFFFF;
    if( InString == null )
      return ~crc;

    if( InString.Length == 0 )
      return ~crc;

    uint data = 0;
    for( int Count = 0; Count < InString.Length; Count++ )
      {
      data = (uint)(0xFF & InString[Count] );
      // For each bit in the data byte.
      for( int i = 0; i < 8; i++ )
        {
        if( 0 != ((crc & 0x0001) ^ (data & 0x0001)) )
          crc = (crc >> 1) ^ Polynomial;
        else
          crc >>= 1;

        data >>= 1;
        }
      }

    crc = ~crc;
    data = crc;
    crc = (crc << 8) | ((data >> 8) & 0xFF);

    // Just make sure it's 16 bits.
    return crc & 0xFFFF;
    }
    */



  internal static int CountCharacters( string InString, char CountChar )
    {
    if( InString == null )
      return 0;

    if( InString.Length == 0 )
      return 0;

    int Total = 0;
    for( int Count = 0; Count < InString.Length; Count++ )
      {
      if( CountChar == InString[Count] )
        Total++;

      }

    return Total;
    }




  internal static int FirstDifferentCharacter( string InString1, string InString2 )
    {
    if( InString1 == null )
      return 0;

    if( InString2 == null )
      return 0;

    if( InString1.Length == 0 )
      return 0;

    if( InString2.Length == 0 )
      return 0;

    int ShortestLength = InString1.Length;
    if( ShortestLength > InString2.Length )
      ShortestLength = InString2.Length;

    for( int Count = 0; Count < ShortestLength; Count++ )
      {
      if( InString1[Count] != InString2[Count] )
        return Count;

      }

    return -1;
    }



  }
}





