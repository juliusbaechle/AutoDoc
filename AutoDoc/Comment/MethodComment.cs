using System.Collections.Generic;

namespace AutoDoc.Comment {
  public class Pair<T1, T2> {
    public Pair() { }
  
    public Pair(T1 key, T2 value) {
      Key = key;
      Value = value;
    }
  
    public T1 Key { get; set; }
    public T2 Value { get; set; }
  }

  public class CommentParam {
    public CommentParam() { }

    public CommentParam(string strParam) {
      if(strParam.Contains("=")) {
        string typeAndName = strParam.Substring(0, strParam.IndexOf('=')).Trim();
        Type = typeAndName.Substring(0, typeAndName.LastIndexOf(' ')).Trim();
        Name = typeAndName.Substring(typeAndName.LastIndexOf(' ')).Trim();
        Default = strParam.Substring(strParam.IndexOf('=') + 1).Trim();
      } else {
        Type = strParam.Substring(0, strParam.LastIndexOf(' ')).Trim();
        Name = strParam.Substring(strParam.LastIndexOf(' ')).Trim();
      }
    }

    public override string ToString() {
      string str = Type + " " + Name;
      if (Default != "")
        str += " = " + Default;
      return str;
    }

    public string Type = "";
    public string Name = "";
    public string Default = "";
  }
  
  public class MethodComment {
    public string Signature = "";
    public string Summary = "";
    public List<Pair<CommentParam, string>> Params = new List<Pair<CommentParam, string>>();
    public Pair<string, string> Return = new Pair<string, string>();
    public List<Pair<string, string>> Changed = new List<Pair<string, string>>();
  }
}