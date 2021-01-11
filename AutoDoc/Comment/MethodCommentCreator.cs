using System.Collections.Generic;

namespace AutoDoc.Comment {
  public class MethodCommentCreator {
    public MethodCommentCreator(MethodConfig config) {
      m_config = config;
    }

    public string Create(MethodComment comment, int align) {
      Add(CreateLine(m_config.Signature, comment.Signature), m_config.AlignText);
      Add("");
      Add(CreateLine(m_config.Summary, comment.Summary), m_config.AlignText);

      if(comment.Params.Count > 0) {
        Add("");
        CreateParamsSection(comment);
      }

      if(comment.Return.Key != "void" && comment.Return.Key != "") {
        Add("");
        Add(CreateLine(m_config.Return, comment.Return.Key + " - " + comment.Return.Value), m_config.AlignText);
      }

      if(comment.Changed.Count > 0) {
        Add("");
        for(int i = 0; i < comment.Changed.Count; i++)
          Add(CreateLine(m_config.Changed, comment.Changed[i].Key + " - " + comment.Changed[i].Value), m_config.AlignText);
      }

      return Decorate(align);
    }

    private string CreateLine(string tag, string keyAndValue) {
      string line = tag;
      line += new string(' ', m_config.AlignColon - tag.Length);
      line += ": ";
      line += keyAndValue;
      return line;
    }
    
    private void CreateParamsSection(MethodComment comment) {
      for (int i = 0; i < comment.Params.Count; i++) {
        string key = comment.Params[i].Key.ToString();
        string value = comment.Params[i].Value;
        string tag = (i == 0)
          ? m_config.Parameter1
          : m_config.Parameter2;

        string line = CreateLine(tag, key + " - " + value);
        Add(line, m_config.AlignText + 3);
      }
    }

    private string Decorate(int align) {
      string whitespace = new string(' ', align);
      string comment = whitespace + m_config.UpperLimiter + m_config.NewLine;
      foreach(string line in m_block)
        comment += whitespace + m_config.EmptyLine + " " + line + m_config.NewLine;
      comment += whitespace + m_config.LowerLimiter + m_config.NewLine;
      return comment;
    }

    private void Add(string value, int alignNewLines = 0) {
      if (value == null) return;

      value = value.Replace(m_config.NewLine, "\n");
      List<string> strList = new List<string>(value.Split('\n'));
      m_block.Add(strList[0]);
      for (int i = 1; i < strList.Count; i++)
        m_block.Add(new string(' ', alignNewLines) + strList[i]);
    }

    private MethodConfig m_config;
    private List<string> m_block = new List<string>();
  }
}