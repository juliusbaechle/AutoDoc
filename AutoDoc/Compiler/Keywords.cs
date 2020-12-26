using System.Collections.Generic;

namespace AutoDoc.Compiler {
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
      { "signals"     , EToken.KEYWORD_ACCESS    },
      { "slots"       , EToken.KEYWORD_SLOT      },
      { "enum"        , EToken.KEYWORD           },
      { "class"       , EToken.KEYWORD_CLASS     },
      { "struct"      , EToken.KEYWORD_CLASS     },
      { "union"       , EToken.KEYWORD_CLASS     },
      { "new"         , EToken.KEYWORD           }
    };
  }
}
