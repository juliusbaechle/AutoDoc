using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Shell;

namespace AutoDoc {
  ///<summary>
  ///  Merkt sich die aktuell geöffneten Editoren. 
  ///  Zweck: IWpfTextView GetCurrentView() 
  ///</summary>
  [Export(typeof(IWpfTextViewCreationListener))]
  [ContentType("text")]
  [TextViewRole(PredefinedTextViewRoles.Document)]
  internal sealed class OpenViewsListener : IWpfTextViewCreationListener {
    public OpenViewsListener() {
      AutoDoc.m_openViewsListener = this;
    }

    public IWpfTextView GetCurrentView() {
      return m_currentViews.Find(view => view.HasAggregateFocus);
    }

    public string GetHeaderContent(string filename) {
      IWpfTextView headerView = null;

      try {
        headerView = searchView(filename);
        if (headerView == null)
          headerView = openView(filename);
        return headerView.TextBuffer.CurrentSnapshot.GetText();
      } catch (Exception) {
        throw new Exception("Not able to open header file: " + filename + "    Solution: .h and .cpp must have the same file name");
      }
    }

    private IWpfTextView openView(string filename) {
      ThreadHelper.ThrowIfNotOnUIThread();
      VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, filename + ".h");
      IWpfTextView view = m_currentViews[m_currentViews.Count - 1];
      VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, filename + ".cpp");
      return view;
    }

    private IWpfTextView searchView(string filename) {
      foreach (IWpfTextView view in m_currentViews) {
        string path = view.TextBuffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument)).FilePath;
        if (path.EndsWith(filename + ".h"))
          return view;
      }
      return null;
    }

    public void TextViewCreated(IWpfTextView textView) {
      m_currentViews.Add(textView);
      textView.Closed += TextViewClosed;
    }

    private void TextViewClosed(object sender, EventArgs e) {
      IWpfTextView closedTextView = sender as IWpfTextView;
      if (closedTextView == null) return;
      m_currentViews.Remove(closedTextView);
    }
    
    private List<IWpfTextView> m_currentViews = new List<IWpfTextView>();
  }
}