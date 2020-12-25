using System.Collections.Generic;

namespace AutoDoc.Compiler {
  public class Token {
    public Token(int a_token, string a_text = "") {
      Type = a_token;
      Text = a_text;
    }

    public Token(EToken a_token, string a_text = "") {
      Type = (int)a_token;
      Text = a_text;
    }

    public override string ToString() {
      string text;

      if (Type < 128) {
        text = ((char)Type).ToString();
      } else {
        text = ((EToken)Type).ToString();
        if (Text != "")
          text += ": " + Text;
      }

      return text;
    }

    public int Type { get; private set; }
    public string Text { get; private set; }
  }

  public interface IScanner {
    int getToken();

    string Text  { get; }
    int Position { get; }
    string Code  { get; }
  };

  public enum EToken {
    END_OF_SCAN = 128,
    STRING_LITERAL,     //"..."
    INTEGER_LITERAL,    //[0-9][0-9]*
    REAL_LITERAL,       //[0-9]*(\.[0-9]+)?([eE]-?[0-9]+)?
    CHAR_LITERAL,
    QUALIFIER,          //::
    RVALUE_REF,         //&&
    IDENTIFIER,
    KEYWORD,
    KEYWORD_SPECIFIER,
    KEYWORD_ACCESS,
    KEYWORD_CV,
    KEYWORD_CLASS,
    COMMENT
  }

  public class Keywords {
    public static EToken getType(string text) {
      if (m_dictionary.ContainsKey(text))
        return m_dictionary[text];
      return EToken.IDENTIFIER;
    }

    private static Dictionary<string, EToken> m_dictionary = new Dictionary<string, EToken>() {
      { "inline"      , EToken.KEYWORD_SPECIFIER },
      { "virtual"     , EToken.KEYWORD_SPECIFIER },
      { "override"    , EToken.KEYWORD_SPECIFIER },
      { "explicit"    , EToken.KEYWORD_SPECIFIER },
      { "constexpr"   , EToken.KEYWORD_SPECIFIER },
      { "friend"      , EToken.KEYWORD_SPECIFIER },
      { "register"    , EToken.KEYWORD_SPECIFIER },
      { "static"      , EToken.KEYWORD_SPECIFIER },
      { "thread_local", EToken.KEYWORD_SPECIFIER },
      { "extern"      , EToken.KEYWORD_SPECIFIER },
      { "mutable"     , EToken.KEYWORD_SPECIFIER },
      { "const"       , EToken.KEYWORD_CV        },
      { "volatile"    , EToken.KEYWORD_CV        },
      { "public"      , EToken.KEYWORD_ACCESS    },
      { "protected"   , EToken.KEYWORD_ACCESS    },
      { "private"     , EToken.KEYWORD_ACCESS    },
      { "slots"       , EToken.KEYWORD_ACCESS    },
      { "signals"     , EToken.KEYWORD_ACCESS    },
      { "enum"        , EToken.KEYWORD           },
      { "class"       , EToken.KEYWORD_CLASS     },
      { "struct"      , EToken.KEYWORD_CLASS     },
      { "union"       , EToken.KEYWORD_CLASS     },
      { "new"         , EToken.KEYWORD           }
    };
  }
}
