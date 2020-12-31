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
      CreateSignature();
      MergeSummary();
      MergeParams();
      MergeReturn();
      return m_newComment;
    }

    private void CreateSignature() {
      m_newComment.Signature = m_method.QualifiedName.ToString();

      for (int i = 0; i < m_method.Specifiers.Count; i++) {
        m_newComment.Signature += (i == 0) ? "   " : " ";
        m_newComment.Signature += "[" + m_method.Specifiers[i] + "]";
      }
    }

    private void MergeSummary() {
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

    private void MergeParams() {
      AddCurrentParams();
      AddChangedParams();
    }

    private void AddCurrentParams() {
      for (int i = 0; i < m_method.Params.Count; i++) {
        string value = "";
        foreach (Pair<CommentParam, string> oldPair in m_oldComment.Params) {
          if (m_method.Params[i].Type.ToString() != oldPair.Key.Type) continue;
          if (m_method.Params[i].Name != oldPair.Key.Name) continue;
          value = oldPair.Value;
        }

        var param = new CommentParam();
        param.Type = m_method.Params[i].Type.ToString();
        param.Name = m_method.Params[i].Name;
        param.Default = m_method.Params[i].Default;

        var pair = new Pair<CommentParam, string>(param, value);
        m_newComment.Params.Add(pair);
      }
    }

    private void AddChangedParams() {
      foreach (Pair<CommentParam, string> oldPair in m_oldComment.Params) {
        bool contained = false;
        foreach (Pair<CommentParam, string> newPair in m_newComment.Params) {
          if (newPair.Key.Type != oldPair.Key.Type) continue;
          if (newPair.Key.Name != oldPair.Key.Name) continue;
          contained = true;
        }

        if (!contained) {
          var pair = new Pair<string, string>(oldPair.Key.ToString(), oldPair.Value);
          m_newComment.Changed.Add(pair);
        }
      }
    }

    private void MergeReturn() {
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
