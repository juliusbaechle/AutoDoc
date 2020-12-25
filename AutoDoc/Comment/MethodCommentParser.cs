using System;
using System.Collections.Generic;

namespace AutoDoc.Comment {
  public class MethodCommentParser {
    public MethodCommentParser(MethodConfig config) {
      m_config = config;
    }

    public MethodComment Parse(List<string> commentBlock) {
      try {
        m_comment.Whitespace = commentBlock[0].IndexOf('/');
        List<string> block = TrimLines(commentBlock);

        foreach (string line in block)
          ParseLine(line);
        AssignLast();

        return m_comment;
      } catch (Exception e) {
        return new MethodComment();
      }
    }

    // Entfernt Limiter ("///" und "///---------...")
    private List<string> TrimLines(List<string> comment) {
      List<string> block = new List<string>();

      foreach (string rawLine in comment) {
        string line = rawLine.Trim();
        if (line == m_config.UpperLimiter) continue;
        if (line == m_config.LowerLimiter) continue;
        if (line == m_config.EmptyLine) continue;

        line = line.Replace(m_config.EmptyLine, "").Trim();
        if(line != "") block.Add(line);
      }

      return block;
    }

    // Zerlegt Zeilen in Sektionsangaben, Key und Value
    // Line -> [Section ":"] [key "-"] value "\n"
    private void ParseLine(string line) {
      if (line.Contains(":")) {
        string delimiter = line.Remove(line.IndexOf(':')).Trim(' ');

        if (m_sectionDelimiters.ContainsKey(delimiter)) {
          AssignLast();
          m_section = m_sectionDelimiters[delimiter];
          line = line.Remove(0, line.IndexOf(':') + 1).Trim(' ');
        }
      }

      if (line.Contains("-")) {
        AssignLast();
        m_key = line.Remove(line.IndexOf('-')).Trim(' ');
        line = line.Remove(0, line.IndexOf('-') + 1).Trim(' ');
      }

      if (m_strComment != "")
        m_strComment += m_config.NewLine;
      m_strComment += line;
    }

    // Weißt in Abhängigkeit der aktuellen Sektion Key und Comment zu
    private void AssignLast() {
      if (m_key == "" && m_section != ESection.Summary) {
        m_strComment = "";
        return;
      }

      if (m_section == ESection.Signature)
        m_comment.Signature = m_strComment;
      if (m_section == ESection.Summary)
        m_comment.Summary = m_strComment;
      if (m_section == ESection.Params)
        m_comment.Params.Add(new Pair<string, string>(m_key, m_strComment));
      if (m_section == ESection.Return)
        m_comment.Return = new Pair<string, string>(m_key, m_strComment);

      m_key = "";
      m_strComment = "";
    }

    private MethodComment m_comment = new MethodComment();
    private MethodConfig m_config = new MethodConfig();

    private enum ESection { Invalid, Signature, Summary, Params, Return, Changed }
    private ESection m_section = ESection.Invalid;
    private Dictionary<string, ESection> m_sectionDelimiters = new Dictionary<string, ESection>() {
      { "Function", ESection.Signature },
      { "Summary", ESection.Summary },
      { "Params", ESection.Params },
      { "Return", ESection.Return },
      { "CHANGED", ESection.Changed }
    };

    private string m_strComment = "";
    private string m_key = "";
  }


  // public class MethodCommentParser {
  //   public MethodCommentParser(MethodConfig a_config) {
  //     m_config = a_config;
  //   }
  // 
  //   public MethodComment Parse(List<string> a_commentBlock) {
  //     try {
  //       m_comment.Whitespace = a_commentBlock[0].IndexOf('/');
  //       m_strComment = TrimLines(a_commentBlock);
  //       return ParseBlock();
  //     } catch(Exception e) {
  //       return null;
  //     }
  //   }
  // 
  //   private string TrimLines(List<string> rawLines) {
  //     List<string> trimmedLines = new List<string>();
  // 
  //     foreach (string rawLine in rawLines) {
  //       string line = rawLine.Trim();
  //       if (line == m_config.UpperLimiter) continue;
  //       if (line == m_config.LowerLimiter) continue;
  //       if (line == m_config.EmptyLine) continue;
  // 
  //       line = line.Replace(m_config.EmptyLine, "");
  //       trimmedLines.Add(line.Trim());
  //     }
  // 
  //     string comment = "";
  //     foreach (string line in trimmedLines)
  //       comment += line + m_config.NewLine;
  //     return comment;
  //   }
  // 
  //   // block -> {section}
  //   private MethodComment ParseBlock() {
  //     nextWord();
  //     while (m_token != EToken.END)
  //       ParseSection();  
  //     return m_comment;
  //   }
  // 
  //   // section -> section_identifier ":" { pair }
  //   private void ParseSection() {
  //     ESection section = section_identifiers[m_word];
  //     nextWord();
  // 
  //     while (section_identifiers.ContainsKey(m_word)) {
  //       Pair<string, string> pair = ParsePair();
  //       Assign(section, pair);
  //     }
  //   }
  // 
  //   // pair -> [key "-"] value {"\n" value} (1...*)
  //   private Pair<string, string> ParsePair() {
  //     Pair<string, string> comment = new Pair<string, string>();
  //     if (m_token == EToken.Key) {
  //       comment.Key = m_word;
  //       nextWord();
  //     }
  // 
  //     comment.Value = m_word;
  //     while(m_token == EToken.Value)
  //       comment.Value += m_config.NewLine + m_word;
  //     return comment;
  //   }
  // 
  //   // word -> .* [":", ",", "\n"]
  //   private void nextWord() {
  //     char c = m_strComment[m_position];
  //     
  //     m_word = "";
  //     while (!delimiters.ContainsKey(c) && m_position < m_strComment.Length) {
  //       m_position++;
  //       m_word += c;
  //       c = m_strComment[m_position];
  //     }
  //     m_word.Trim();
  //           
  //     m_token = delimiters.ContainsKey(c)
  //       ? delimiters[c]
  //       : EToken.END;
  // 
  //     // Skip delimiter
  //     m_position++; 
  //   }
  // 
  //   private void Assign(ESection section, Pair<string, string> pair) {
  //     if (section == ESection.Summary)
  //       m_comment.Summary = pair.Value;
  //     if (section == ESection.Params)
  //       m_comment.Params.Add(pair.Key, pair.Value);
  //     if (section == ESection.Return)
  //       m_comment.Return = pair;
  //   }
  // 
  //   private MethodConfig m_config;
  //   private MethodComment m_comment = new MethodComment();
  // 
  //   private enum ESection { Invalid, Signature, Summary, Params, Return };
  //   private Dictionary<string, ESection> section_identifiers = new Dictionary<string, ESection>() {
  //     { "Function", ESection.Signature },
  //     { "Summary", ESection.Summary },
  //     { "Params", ESection.Params },
  //     { "Return", ESection.Return }
  //   };
  // 
  //   private enum EToken { Section_Identifier, Key, Value, END }    
  //   private Dictionary<char, EToken> delimiters = new Dictionary<char, EToken>() {
  //     { ':', EToken.Section_Identifier },
  //     { '-', EToken.Key },
  //     { '\n', EToken.Value }
  //   };
  // 
  //   private string m_strComment = "";
  //   private EToken m_token = EToken.END;
  //   private string m_word = "";
  //   private int m_position = 0;
  // }
}
