using AutoDoc.Compiler;
using System.Collections.Generic;

namespace AutoDoc.Comment {
  public class MethodCommentMerger {
    public MethodCommentMerger(Method method, MethodComment oldComment) {
      m_method = method;
      if(m_oldComment != null)
        m_oldComment = oldComment;
    }

    public MethodComment Merge() {
      m_newComment.Whitespace = m_oldComment.Whitespace;
      createSignature();
      mergeSummary();
      mergeParams();
      mergeReturn();
      return m_newComment;
    }

    private void createSignature() {
      m_newComment.Signature = m_method.QualifiedName.ToString();

      for (int i = 0; i < m_method.Specifiers.Count; i++) {
        m_newComment.Signature += (i == 0) ? "   " : " ";
        m_newComment.Signature += "[" + m_method.Specifiers[i] + "]";
      }
    }

    private void mergeSummary() {
      if(m_oldComment.Summary != "") {
        m_newComment.Summary = m_oldComment.Summary;
        return;
      }

      List<TypeName> names = m_method.QualifiedName.Names;

      if (names.Count <= 1) return;
      string className = names[names.Count - 2].ToString();
      string methodName = names[names.Count - 1].ToString();

      if (className == methodName)
        m_newComment.Summary = "Konstruktor";

      if ("~" + className == methodName)
        m_newComment.Summary = "Destruktor";
    }

    private void mergeParams() {
      List<string> newKeys = new List<string>();

      for (int i = 0; i < m_method.Params.Count; i++) {
        string key = m_method.Params[i].Type.ToString() + " " + m_method.Params[i].Name;

        string value = "";
        foreach (Pair<string, string> oldPair in m_oldComment.Params)
          if (oldPair.Key == key)
            value = oldPair.Value;

        m_newComment.Params.Add(new Pair<string, string>(key, value));
        newKeys.Add(key);
      }
      
      foreach(Pair<string, string> oldPair in m_oldComment.Params) {
        if (!newKeys.Contains(oldPair.Key))
          m_newComment.Changed.Add(oldPair);
      }
    }

    private void mergeReturn() {
      string newKey = m_method.ReturnType.ToString();
      string oldKey = m_oldComment.Return.Key;

      if (newKey == oldKey) {                                         // Return-Typ gleichgeblieben
        m_newComment.Return.Key = newKey;
        m_newComment.Return.Value = m_oldComment.Return.Value;
      } else if(oldKey != null && oldKey != "" && oldKey != "void") { // Return-Typ geändert
        m_newComment.Changed.Add(m_oldComment.Return);
        m_newComment.Return.Key = newKey;
      } else {                                                        // Return-Typ erstmalig eingetragen
        m_newComment.Return.Key = newKey;
      }
    }

    MethodComment m_newComment = new MethodComment();
    MethodComment m_oldComment = new MethodComment();
    Method m_method = null;
  }
}
