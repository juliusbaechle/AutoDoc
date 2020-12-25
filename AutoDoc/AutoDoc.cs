using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoDoc.Compiler;
using AutoDoc.Comment;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace AutoDoc {
  class AutoDoc {
    public static OpenViewsListener m_openViewsListener;
    public static MethodConfig config = new MethodConfig();
    
    static public void CreateText() {
      try {
        var view = m_openViewsListener.GetCurrentView();
        var doc = view.TextBuffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument));
        int lineNbr = view.Selection.ActivePoint.Position.GetContainingLine().LineNumber; // 0-based !

        // Parse .cpp Function Definition
        string methodDeclaration = GetMethodDeclaration(view.TextBuffer.CurrentSnapshot, view.Selection.ActivePoint.Position);
        var parser = new Parser(new Scanner(methodDeclaration));
        var method = parser.ParseMethod();
        // TODO: Parse .h Function Declaration
        // TODO: Merge method declarations

        VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, "TestApp.h");        

        // Parse old comment, merge method and create new comment
        // TODO: Include default values
        List<ITextSnapshotLine> lines = new List<ITextSnapshotLine>(view.TextSnapshot.Lines);
        List<string> commentLines = GetCommentLines(lines, lineNbr);
        var oldComment = new MethodCommentParser(config).Parse(commentLines);
        var newComment = new MethodCommentMerger(method, oldComment).Merge();
        string comment = new MethodCommentCreator(config).Create(newComment);

        using (ITextEdit edit = view.TextBuffer.CreateEdit()) {
          int start = lines[lineNbr - commentLines.Count].Start.Position;
          int end = lines[lineNbr].Start.Position;

          edit.Delete(start, end - start);
          edit.Insert(end, comment);
          edit.Apply();
        }
      } catch(Exception e) {
        Debug.WriteLine("AutoDoc failed: " + e.Message);
      }
    }

    static List<string> GetCommentLines(List<ITextSnapshotLine> lines, int lineNbr) {
      if (lineNbr == 0) return new List<string>();
      int endLineNbr = lineNbr - 1;

      int startLineNbr = endLineNbr;
      while (lines[startLineNbr].GetText().Trim().StartsWith(config.EmptyLine) && startLineNbr > 0) startLineNbr--;

      var lineRange = lines.GetRange(startLineNbr + 1, endLineNbr - startLineNbr);
      
      List<string> commentLines = new List<string>();
      foreach (var line in lineRange) commentLines.Add(line.GetText());
      return commentLines;
    }

    static string GetMethodDeclaration(ITextSnapshot snap, int activePoint) {
      char[] buffer = snap.ToCharArray(0, snap.Length);
      
      int start = activePoint;
      string delimiters = "};#-";
      while (start > 0 && !delimiters.Contains(buffer[start])) start--; 
      if (buffer[start] == '#') while (buffer[start] != '\n') start++;  // Falls auf Präprozessor-Direktive gestoßen: Gehe bis zum Zeilenumbruch vor
      while (delimiters.Contains(buffer[start])) start++;               // Gehe bis zum Whitespace vor

      int end = activePoint;
      while (end < buffer.Length && buffer[end] != '{') end++;

      string declaration = "";
      for(int i = start; i < end; i++)
        declaration += buffer[i];
      return declaration.Trim();
    }

    static bool isWhitespace(char c) { return c < '!' || c > '~'; }
  }
}