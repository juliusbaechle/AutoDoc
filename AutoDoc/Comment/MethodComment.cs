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
  
  public class MethodComment {
    public int Whitespace = 2;
    public string Signature = "";
    public string Summary = "";
    public List<Pair<string, string>> Params = new List<Pair<string, string>>();
    public Pair<string, string> Return = new Pair<string, string>();
    public List<Pair<string, string>> Changed = new List<Pair<string, string>>();
  }
}