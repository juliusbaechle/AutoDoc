using System;
using System.Linq;

namespace AutoDoc.Compiler {
  class EndOfScanException : Exception { };

  public class Scanner : IScanner {
    public Scanner(string code) {
      m_code = code + '\0';
    }

    public string ScannedText { get; set; }
    public EToken TokenType  { get; private set; }

    public string getToken() {
      try {
        TokenType = EToken.INVALID;
        m_text = "";

        // Skip til next non whitespace char
        while (isWhitespace(m_cc))
          readChar();

        // Colon ':'
        if (m_cc == ':') {
          TokenType = EToken.OPERATOR;
          readChar();
          if (m_cc == ':')
            readChar();
          return m_text;
        }

        // And '&'
        if (m_cc == '&') {
          TokenType = EToken.OPERATOR;
          readChar();
          if (m_cc == '&')
            readChar();
          return m_text;
        }

        // Number Literal
        // TODO: Properly scan real literals
        if (isNumber(m_cc)) {
          do {
            readChar();
          } while (isNumber(m_cc));

          if (m_cc != '.') {
            TokenType = EToken.INTEGER_LITERAL;
            return m_text;
          } else {
            do {              
              readChar();
            } while (isNumber(m_cc) || "efl+-".Contains(m_cc));

            TokenType = EToken.REAL_LITERAL;
            return m_text;
          }
        }

        // String Literal
        if (m_cc == '"') {
          bool escaped = false;

          readRichChar(out escaped);
          while (m_cc != '"' || escaped)
            readRichChar(out escaped);
          readChar();

          TokenType = EToken.STRING_LITERAL;
          m_text = m_text.Remove(0, 1);
          m_text = m_text.Remove(m_text.Length - 1, 1);
          return m_text;
        }

        // Char Literal
        if (m_cc == '\'') {
          bool escaped = false;

          readRichChar(out escaped);
          while (m_cc != '\'' || escaped)
            readRichChar(out escaped);
          readChar();

          TokenType = EToken.CHAR_LITERAL;
          m_text = m_text.Remove(0, 1);
          m_text = m_text.Remove(m_text.Length - 1, 1);
          return m_text;
        }

        // Comments
        if (m_cc == '/') {
          readChar();

          // Line comment
          if (m_cc == '/') {
            while (m_cc != '\n' && m_cc != '\0')
              readChar();
            readChar();

            TokenType = EToken.COMMENT;
            return m_text; 
          }

          // Block comment
          if (m_cc == '*') {
            char lastC = ' ';
            while((m_cc != '/' || lastC != '*') && m_cc != '\0') {
              lastC = m_cc;
              readChar();
            }
            readChar();

            TokenType = EToken.COMMENT;
            return m_text;
          }

          TokenType = EToken.OPERATOR;
          return "/";
        }

        // Identifier / Keywords
        if (isLetter(m_cc) || m_cc == '_') {
          while ((isLetter(m_cc) || isNumber(m_cc) || m_cc == '_') && m_cc != '\0')
            readChar();

          TokenType = Keywords.getType(m_text);
          return m_text;
        }

        // Default
        char token = m_cc;
        TokenType = EToken.OPERATOR;
        readChar();
        return m_text;
      } catch (EndOfScanException) {
        TokenType = EToken.END_OF_SCAN;
        return m_text;
      }   
    }

    // Called when not in String / Char-Literal
    private void readChar() {
      ScannedText += m_cc;
      if (!isWhitespace(m_cc))
        m_text += m_cc;
      m_cc = nextChar();
    }

    // Called in String / Char-Literal; accepting escape chars
    private void readRichChar(out bool escaped) {
      ScannedText += m_cc;

      char c = nextChar();      
      escaped = false;

      if (c == '\\') {
        escaped = true;

        ScannedText += c;
        c = nextChar();

        if (c == 'a') c = '\x07';  // audible bell
        if (c == 'b') c = '\x08';  // backspace
        if (c == 'f') c = '\x0c';  // form feed - new page
        if (c == 'n') c = '\x0a';  // line feed - new line
        if (c == 'r') c = '\x0d';  // carraige return
        if (c == 't') c = '\x09';  // horizontal tab
        if (c == 'v') c = '\x0b';  // vertical tab
        
        // TODO: Accept numeric escape sequences
        if (isNumber(c) || c == 'x' || c == 'u' || c == 'U')
          throw new Exception("numeric escape sequences are not supported yet");
      }
      
      m_text += m_cc;
      m_cc = c;
    }

    private char nextChar() {
      if (Position >= m_code.Length)
        throw new EndOfScanException();
      return m_code[Position++];
    }

    bool isWhitespace(char c) { return c < '!' || c > '~'; }
    bool isNumber(char c) { return c >= '0' && c <= '9'; }
    bool isLetter(char c) { return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z'; }

    private char m_cc = '\0'; // Current Character
    private string m_text = "";

    public int Position { get; private set; }
    string m_code = "";
  }
}