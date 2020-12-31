using AutoDoc.Compiler;
using System.Collections.Generic;

namespace AutoDoc.Comment {
  ///---------------------------------------------------
  /// Function: %Signature%
  ///
  /// Summary :
  ///
  /// Params  : %Parameter% -                (optional)
  ///
  /// Return  : %Return% -                   (optional)
  ///---------------------------------------------------

  public class MethodConfig {
    public string UpperLimiter   = "///-----------------------------------------------------------------------------------";
    public string EmptyLine      = "///";
    public string LowerLimiter   = "///-----------------------------------------------------------------------------------";
    public string NewLine        = "\r\n";

    public string SignatureLine  = "Function: ";
    public string SummaryLine    = "Summary : ";
    public string ParameterLine1 = "Params  : ";
    public string ParameterLine2 = "          ";
    public string ReturnLine     = "Return  : ";
    public string ChangedLine    = "CHANGED : ";

    public int Align = 10;
  }

  public class MethodCommentCreator {
    public MethodCommentCreator(MethodConfig config) {
      m_config = config;
    }

    public string Create(MethodComment comment, int align) {
      Add(m_config.SignatureLine + comment.Signature, m_config.Align);
      Add("");
      Add(m_config.SummaryLine + comment.Summary, m_config.Align);

      if(comment.Params.Count > 0) {
        Add("");
        CreateParamsSection(comment);
      }

      if(comment.Return.Key != "void" && comment.Return.Key != "") {
        Add("");
        Add(m_config.ReturnLine + comment.Return.Key + " - " + comment.Return.Value, m_config.Align);
      }

      if(comment.Changed.Count > 0) {
        Add("");
        for(int i = 0; i < comment.Changed.Count; i++)
          Add(m_config.ChangedLine + comment.Changed[i].Key + " - " + comment.Changed[i].Value);
      }

      return Decorate(align);
    }

    private void CreateParamsSection(MethodComment comment) {
      for (int i = 0; i < comment.Params.Count; i++) {
        string line = (i == 0)
          ? m_config.ParameterLine1
          : m_config.ParameterLine2;
        
        line += comment.Params[i].Key.ToString();
        line += " - " + comment.Params[i].Value;
        Add(line, m_config.Align + 3);
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

    private void Add(string value, int align = 0) {
      if (value == null) return;

      value = value.Replace(m_config.NewLine, "\n");
      List<string> strList = new List<string>(value.Split('\n'));
      m_block.Add(strList[0]);
      for (int i = 1; i < strList.Count; i++)
        m_block.Add(new string(' ', align) + strList[i]);
    }

    private MethodConfig m_config;
    private List<string> m_block = new List<string>();
  }
}