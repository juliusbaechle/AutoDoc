using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace AutoDoc {
  internal sealed class TextReplacer {
    public TextReplacer(IWpfTextView a_view) {
      m_view = a_view ?? throw new ArgumentNullException("view");
      m_view.LayoutChanged += OnLayoutChanged;
    }

    internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e) {
      using (ITextEdit edit = m_view.TextBuffer.CreateEdit()) {
        ITextSnapshot snap = edit.Snapshot;
        if (!snap.GetText().Contains("$Text")) return;

        int position = snap.GetText().IndexOf("$Text");
        edit.Delete(position, 5);
        edit.Insert(position, "$NewText$");
        edit.Apply();
      }
    }

    private readonly IWpfTextView m_view;
  }
}
