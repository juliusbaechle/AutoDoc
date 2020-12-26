namespace AutoDoc.Compiler {
  public enum EToken {
    END_OF_SCAN = 128,
    INVALID,
    STRING_LITERAL,     //"..."
    INTEGER_LITERAL,    //[0-9][0-9]*
    REAL_LITERAL,       //[0-9]*(\.[0-9]+)?([eE]-?[0-9]+)?
    CHAR_LITERAL,
    OPERATOR,
    IDENTIFIER,
    KEYWORD,
    KEYWORD_SPECIFIER,
    KEYWORD_ACCESS,
    KEYWORD_SLOT,
    KEYWORD_CV,
    KEYWORD_CLASS,
    COMMENT
  }

  public class Token {
    public Token(string a_text, EToken a_token = EToken.OPERATOR) {
      Type = a_token;
      Text = a_text;
    }

    public Token(EToken a_token) {
      Type = a_token;
      Text = "";
    }

    public override string ToString() {
      return Type.ToString() + ": " + Text;
    }

    public EToken Type { get; private set; }
    public string Text { get; private set; }
  }
}
