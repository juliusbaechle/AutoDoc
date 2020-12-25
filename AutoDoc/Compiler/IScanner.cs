using System.Collections.Generic;

namespace AutoDoc.Compiler {
  public interface IScanner {
    string getToken();

    EToken TokenType { get; }
    int Position { get; }
    string Code  { get; }
  };
}
