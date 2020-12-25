using System;
using System.Linq;

namespace AutoDoc.Compiler {
  class EndOfScanException : Exception { };

  public class Scanner : IScanner {
    public Scanner(string sourcecode) {
      Code = sourcecode + ' ';
    }

    public EToken TokenType  { get; private set; }
    public int Position { get; private set; } = 0;
    public string Code  { get; private set; }

    public string getToken() {
      try {
        TokenType = EToken.INVALID;
        m_text = "";

        // Skip til next non whitespace char
        while (isWhitespace(m_cc))
          nextChar();

        // Colon ':'
        if (m_cc == ':') {
          TokenType = EToken.OPERATOR;
          nextChar();
          if (m_cc == ':')
            nextChar();
          return m_text;
        }

        // And '&'
        if (m_cc == '&') {
          TokenType = EToken.OPERATOR;
          nextChar();
          if (m_cc == '&')
            nextChar();
          return m_text;
        }

        // Number Literal
        // TODO: Properly scan real literals
        if (isNumber(m_cc)) {
          do {
            nextChar();
          } while (isNumber(m_cc));

          if (m_cc != '.') {
            TokenType = EToken.INTEGER_LITERAL;
            return m_text;
          } else {
            do {              
              nextChar();
            } while (isNumber(m_cc) || "efl+-".Contains(m_cc));

            TokenType = EToken.REAL_LITERAL;
            return m_text;
          }
        }

        // String Literal
        if (m_cc == '"') {
          bool escaped = false;

          nextRichChar(out escaped);
          while (m_cc != '"' || escaped)
            nextRichChar(out escaped);
          nextChar();

          TokenType = EToken.STRING_LITERAL;
          m_text = m_text.Remove(0, 1);
          m_text = m_text.Remove(m_text.Length - 1, 1);
          return m_text;
        }

        // Char Literal
        if (m_cc == '\'') {
          bool escaped = false;

          nextRichChar(out escaped);
          while (m_cc != '\'' || escaped)
            nextRichChar(out escaped);
          nextChar();

          TokenType = EToken.CHAR_LITERAL;
          m_text = m_text.Remove(0, 1);
          m_text = m_text.Remove(m_text.Length - 1, 1);
          return m_text;
        }

        // Comments
        if (m_cc == '/') {
          nextChar();

          // Line comment
          if (m_cc == '/') {
            while (m_cc != '\n')
              nextChar();
            nextChar();

            TokenType = EToken.COMMENT;
            return m_text; 
          }

          // Block comment
          if (m_cc == '*') {
            char lastC = ' ';
            while(!(lastC == '*' && m_cc == '/')) {
              lastC = m_cc;
              nextChar();
            }
            nextChar();

            TokenType = EToken.COMMENT;
            return m_text;
          }

          TokenType = EToken.OPERATOR;
          return "/";
        }

        // Identifier / Keywords
        if (isLetter(m_cc) || m_cc == '_') {
          while (isLetter(m_cc) || isNumber(m_cc) || m_cc == '_')
            nextChar();

          TokenType = Keywords.getType(m_text);
          return m_text;
        }

        // Default
        char token = m_cc;
        TokenType = EToken.OPERATOR;
        nextChar();
        return m_text;
      } catch (EndOfScanException ex) {
        TokenType = EToken.END_OF_SCAN;
        return m_text;
      }      
    }

    // Called when not in String / Char-Literal
    private void nextChar() {
      if (Position >= Code.Length)
        throw new EndOfScanException();

      if (!isWhitespace(m_cc))
        m_text += m_cc;
      m_cc = Code[Position++];
    }

    // Called in String / Char-Literal; accepting escape chars
    private void nextRichChar(out bool escaped) {
      if (Position >= Code.Length)
        throw new EndOfScanException();
      
      char c = Code[Position++];
      escaped = false;

      if (c == '\\') {
        escaped = true;
        c = Code[Position++];

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
    
    private char m_cc = ' '; // Current Character
    private string m_text = "";

    bool isWhitespace(char c) { return c < '!' || c > '~'; }
    bool isNumber(char c) { return c >= '0' && c <= '9'; }
    bool isLetter(char c) { return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z'; }
  }
}
