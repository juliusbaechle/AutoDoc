using System;
using System.Diagnostics;
using System.Linq;

namespace AutoDoc.Compiler {
  class EndOfScanException : Exception { };

  public class Scanner : IScanner {
    public Scanner(string sourcecode) {
      Code = sourcecode + ' ';
    }

    public string Text  { get; private set; }
    public int Position { get; private set; } = 0;
    public string Code  { get; private set; }

    public int getToken() {
      try {
        Text = "";

        // Skip til next non whitespace char
        while (isWhitespace(c))
          c = nextChar();

        // Colon ':'
        if (c == ':') {
          if ((c = nextChar()) == ':') {
            c = nextChar();
            return (int)EToken.QUALIFIER;
          } else {
            return ':';
          }
        }

        // And '&'
        if (c == '&') {
          if ((c = nextChar()) == '&') {
            c = nextChar();
            return (int)EToken.RVALUE_REF;
          } else {
            return '&';
          }
        }

        // Number Literal
        // TODO: Properly scan real literals
        if (isNumber(c)) {
          do {
            Text += c;
            c = nextChar();
          } while (isNumber(c));

          if (c != '.') {
            return (int)EToken.INTEGER_LITERAL;
          } else {
            do {
              Text += c;
              c = nextChar();
            } while (isNumber(c) || "efl+-".Contains(c));

            return (int)EToken.REAL_LITERAL;
          }
        }

        // String Literal
        if (c == '"') {
          bool escaped = false;
          c = nextRichChar(out escaped);
          while (c != '"' || escaped) {
            Text += c;
            c = nextRichChar(out escaped);
          }
          c = nextChar();
          return (int)EToken.STRING_LITERAL;
        }

        // Char Literal
        if (c == '\'') {
          bool escaped = false;
          c = nextRichChar(out escaped);
          while (c != '\'' || escaped) {
            Text += c;
            c = nextRichChar(out escaped);
          }
          c = nextChar();
          return (int)EToken.CHAR_LITERAL;
        }

        // Comments
        if (c == '/') {
          c = nextChar();

          // Line comment
          if (c == '/') {
            while (c != '\n')
              c = nextChar();

            c = nextChar();
            return (int)EToken.COMMENT;
          }

          // Block comment
          if (c == '*') {
            char lastC = ' ';
            while(!(lastC == '*' && c == '/')) {
              lastC = c;
              c = nextChar();
            }

            c = nextChar();
            return (int)EToken.COMMENT;
          }

          return '/';
        }

        // Identifier / Keywords
        if (isLetter(c) || c == '_') {
          while (isLetter(c) || isNumber(c) || c == '_') {
            Text += c;
            c = nextChar();
          }

          return (int)Keywords.getType(Text);
        }

        // Default
        char token = c;
        c = nextChar();
        return token;
      } catch (EndOfScanException ex) {
        return (int)EToken.END_OF_SCAN;
      }      
    }

    // Called when not in String / Char-Literal
    private char nextChar() {
      if (Position >= Code.Length)
        throw new EndOfScanException();

      char c = Code[Position++];
      if (c == '\\')
        Debug.WriteLine("unexpected token: " + c);
      return c;
    }

    // Called in String / Char-Literal; accepting escape chars
    private char nextRichChar(out bool escaped) {
      if (Position >= Code.Length)
        throw new EndOfScanException();

      char c = Code[Position++];
      escaped = false;

      if (c == '\\') {
        escaped = true;
        c = Code[Position++];

        if (c == 'a') return '\x07';  // audible bell
        if (c == 'b') return '\x08';  // backspace
        if (c == 'f') return '\x0c';  // form feed - new page
        if (c == 'n') return '\x0a';  // line feed - new line
        if (c == 'r') return '\x0d';  // carraige return
        if (c == 't') return '\x09';  // horizontal tab
        if (c == 'v') return '\x0b';  // vertical tab
        
        // TODO: Accept numeric escape sequences
        if (isNumber(c) || c == 'x' || c == 'u' || c == 'U')
          throw new Exception("numeric escape sequences are not supported yet");

        return c;
      }

      return c;
    }
    
    private char c = ' ';

    bool isWhitespace(char c) { return c < '!' || c > '~'; }
    bool isNumber(char c) { return c >= '0' && c <= '9'; }
    bool isLetter(char c) { return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z'; }
  }
}
