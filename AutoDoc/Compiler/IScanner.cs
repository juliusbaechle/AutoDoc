using System.Collections.Generic;

namespace AutoDoc.Compiler {
  public interface IScanner {
    string getToken();
    EToken TokenType { get; }
    string ScannedText { get; set; }
  };
}
