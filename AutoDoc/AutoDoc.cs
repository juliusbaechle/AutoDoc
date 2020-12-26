using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoDoc.Compiler;
using AutoDoc.Comment;
using System.Diagnostics;
using Microsoft.VisualStudio.Text.Editor;

namespace AutoDoc {
  // TODO: Calculate Method Alignment
  // TODO: Include default values

  class AutoDoc {
    public static OpenViewsListener m_openViewsListener;
    public static MethodConfig m_config = new MethodConfig();
    
    static public void CreateText() {
      try {
        var view = m_openViewsListener.GetCurrentView();
        List<ITextSnapshotLine> lines = new List<ITextSnapshotLine>(view.TextSnapshot.Lines);
        int activeLine = view.Selection.ActivePoint.Position.GetContainingLine().LineNumber; // 0-based !
        List<string> commentLines = GetCommentLines(lines, activeLine);

        var cppMethod = GetCppMethod(view);
        var headerMethod = GetHeaderMethod(GetFilename(view), cppMethod);
        Method method = Merge(headerMethod, cppMethod);

        MethodComment oldComment = new MethodCommentParser(m_config).Parse(commentLines);
        MethodComment newComment = new MethodCommentMerger(method, oldComment).Merge();
        string comment = new MethodCommentCreator(m_config).Create(newComment);

        ReplaceLines(view, activeLine - commentLines.Count, commentLines.Count, comment);
      } catch(Exception e) {
        Debug.WriteLine("AutoDoc failed: " + e.Message);
      }
    }

    static List<string> GetCommentLines(List<ITextSnapshotLine> lines, int lineNbr) {
      if (lineNbr == 0) return new List<string>();
      int endLineNbr = lineNbr - 1;

      int startLineNbr = endLineNbr;
      while (lines[startLineNbr].GetText().Trim().StartsWith(m_config.EmptyLine) && startLineNbr > 0) startLineNbr--;

      var lineRange = lines.GetRange(startLineNbr + 1, endLineNbr - startLineNbr);
      
      List<string> commentLines = new List<string>();
      foreach (var line in lineRange) commentLines.Add(line.GetText());
      return commentLines;
    }

    private static Method GetCppMethod(IWpfTextView view) {
      string content = view.TextBuffer.CurrentSnapshot.GetText();
      int start = view.Selection.ActivePoint.Position;
      string methodDeclaration = GetMethodDeclaration(content, start);

      var scanner = new Scanner(methodDeclaration);
      var parser = new Parser(scanner);
      return parser.ParseMethod();
    }

    private static Method GetHeaderMethod(string filename, Method cppMethod) {
      try {
        string header = m_openViewsListener.GetHeaderContent(filename);
        var scanner = new Scanner(header);
        Method headerMethod = null;

        while (true) {
          string access = "";
          string text = "";
          bool slot = false;

          while (text != cppMethod.QualifiedName.ElementName) {
            text = scanner.getToken();

            if (scanner.TokenType == EToken.KEYWORD_ACCESS)
              access = text;
            if (scanner.TokenType == EToken.KEYWORD_SLOT)
              slot = true;
          }

          string declaration = GetMethodDeclaration(header, scanner.Position);
          headerMethod = new Parser(new Scanner(declaration)).ParseMethod();
          if (!IsMatch(headerMethod, cppMethod)) continue;

          headerMethod.Specifiers.Insert(0, access);
          if(slot) headerMethod.Specifiers.Insert(0, "slot");
          return headerMethod;          
        }     
      } catch (Exception ex) {
        Debug.WriteLine("Failed to parse header: " + ex.Message);
        return null;
      }
    }

    private static bool IsMatch(Method headerMethod, Method cppMethod) {
      if (headerMethod.Params.Count != cppMethod.Params.Count)
        return false;
      for (int i = 0; i < headerMethod.Params.Count; i++)
        if (headerMethod.Params[i].Type.ToString() != cppMethod.Params[i].Type.ToString())
          return false;
      if (headerMethod.ReturnType.ToString() != cppMethod.ReturnType.ToString())
        return false;

      return true;
    }

    private static Method Merge(Method headerMethod, Method cppMethod) {
      if (headerMethod == null) return cppMethod;
      if (cppMethod == null) return headerMethod;

      cppMethod.Specifiers = headerMethod.Specifiers;

      for(int i = 0; i < headerMethod.Params.Count; i++) {
        if (headerMethod.Params[i].Default != "")
          cppMethod.Params[i].Default = headerMethod.Params[i].Default;
      }

      return cppMethod;
    }

    private static string GetMethodDeclaration(string content, int start) {      
      int end = start;
      while (end < content.Length && content[end] != '{' && content[end] != ';') end++;


      string delimiters = "};#-";
      while (!delimiters.Contains(content[start - 1]) && start > 0) start--;

      // Falls auf Präprozessor-Direktive gestoßen: Gehe bis zum Zeilenumbruch vor    
      if (start >= 1 && content[start - 1] == '#')
        while (content[start] != '\n') start++;  


      string declaration = "";
      for(int i = start; i < end; i++)
        declaration += content[i];
      return declaration.Trim();
    }

    private static void ReplaceLines(IWpfTextView view, int startLineNbr, int lineCount, string text) {
      List<ITextSnapshotLine> lines = new List<ITextSnapshotLine>(view.TextSnapshot.Lines);

      using (ITextEdit edit = view.TextBuffer.CreateEdit()) {
        int start = lines[startLineNbr].Start.Position;
        int end = lines[startLineNbr + lineCount].Start.Position;
        edit.Delete(start, end - start);

        edit.Insert(end, text);
        edit.Apply();
      }
    }

    private static string GetFilename(IWpfTextView view) {
      string cppPath = view.TextBuffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument)).FilePath;
      return cppPath.Substring(cppPath.LastIndexOf('\\') + 1).Replace(".cpp", "");
    }

    private static bool isWhitespace(char c) { return c < '!' || c > '~'; }
  }
}