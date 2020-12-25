using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System;

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
      return m_currentDocs.Find(view => view.HasAggregateFocus);
    }

    public void TextViewCreated(IWpfTextView textView) {
      m_currentDocs.Add(textView);
      textView.Closed += TextViewClosed;
    }

    private void TextViewClosed(object sender, EventArgs e) {
      IWpfTextView closedTextView = sender as IWpfTextView;
      if (closedTextView == null) return;
      m_currentDocs.Remove(closedTextView);
    }
    
    private List<IWpfTextView> m_currentDocs = new List<IWpfTextView>();
  }
}