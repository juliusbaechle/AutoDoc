using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AutoDoc.Compiler;
using AutoDoc.Comment;
using System.Diagnostics;

[TestClass]
public class Tests {
  [TestMethod]
  public void CreateComments() {
    MethodComment comment = new MethodComment();
    comment.Whitespace = 2;
    comment.Signature = "Routine::getVariable [virtual]";
    comment.Summary = "Gibt Variable der Routine zurück\r\nFalls nicht vorhanden wird ungültige QVariant zurückgegeben";
    comment.Params.Add(new Pair<string, string>("const QString& a_strVariableName", "Name der gesuchten Variablen"));
    comment.Return = new Pair<string, string>("QVariant", "Variablenwert");

    string strComment = new MethodCommentCreator(new MethodConfig()).Create(comment);
    Debug.WriteLine(strComment);

    string strExspectedComment =
      "  ///-----------------------------------------------------------------------------------\r\n" +
      "  /// Function: Routine::getVariable [virtual]\r\n" +
      "  /// \r\n" +
      "  /// Summary : Gibt Variable der Routine zurück\r\n" +
      "  ///           Falls nicht vorhanden wird ungültige QVariant zurückgegeben\r\n" +
      "  /// \r\n" +
      "  /// Params  : const QString& a_strVariableName - Name der gesuchten Variablen\r\n" +
      "  /// \r\n" +
      "  /// Return  : QVariant - Variablenwert\r\n" +
      "  ///-----------------------------------------------------------------------------------\r\n";

    Assert.AreEqual(strExspectedComment, strComment);
  }

  [TestMethod]
  public void MergeMethod() {
    //TODO: Test MethodCommentMerger
  }

  [TestMethod]
  public void Scan() {
    string sourceCode =
      " class Menu::dgPage { } " +
      " public: " +
      " const QMap<QString, QVariant>& func();" +
      " \"Hello \\\"World\\\"\" " +
      " 10.25e-2 " +
      " '\\'' ";

    var scanner = new Scanner(sourceCode);
    List<Token> tokens = new List<Token>();

    Token token;
    do {
      token = new Token(scanner.getToken(), scanner.Text);
      tokens.Add(token);
    } while (token.Type != (int)EToken.END_OF_SCAN);


    List<Token> exspectedTokens = new List<Token>() {
      new Token(EToken.KEYWORD_CLASS, "class"),
      new Token(EToken.IDENTIFIER, "Menu" ),
      new Token(EToken.QUALIFIER),
      new Token(EToken.IDENTIFIER, "dgPage"),
      new Token('{'),
      new Token('}'),

      new Token(EToken.KEYWORD_ACCESS, "public"),
      new Token(':'),

      new Token(EToken.KEYWORD_CV, "const"),
      new Token(EToken.IDENTIFIER, "QMap"),
      new Token('<'),
      new Token(EToken.IDENTIFIER, "QString"),
      new Token(','),
      new Token(EToken.IDENTIFIER, "QVariant"),
      new Token('>'),
      new Token('&'),
      new Token(EToken.IDENTIFIER, "func"),
      new Token('('),
      new Token(')'),
      new Token(';'),

      new Token(EToken.STRING_LITERAL, "Hello \"World\""),
      new Token(EToken.REAL_LITERAL, "10.25e-2"),
      new Token(EToken.CHAR_LITERAL, "'"),

      new Token(EToken.END_OF_SCAN)
    };

    Assert.AreEqual(tokens.Count, exspectedTokens.Count);
    for (int i = 0; i < tokens.Count; i++) {
      Assert.AreEqual(tokens[i].Type, exspectedTokens[i].Type);
      Assert.AreEqual(tokens[i].Text, exspectedTokens[i].Text);
    }
  }

  [TestMethod]
  public void ParseMethod() {
    string sourceCode = "virtual inline const Qt::QMap<QString *const, QVariant>&& Menu::variables(int a_iInt = 5, QString a_strText = new QString()) const";
    Scanner scanner = new Scanner(sourceCode);
    Parser parser = new Parser(scanner);
    Method method = parser.ParseMethod();


    Assert.AreEqual("Menu::variables", method.QualifiedName.ToString());
    Assert.AreEqual("const Qt::QMap<QString* const, QVariant>&&", method.ReturnType.ToString());

    Assert.AreEqual(3, method.Specifiers.Count);
    Assert.AreEqual("virtual", method.Specifiers[0]);
    Assert.AreEqual("inline" , method.Specifiers[1]);
    Assert.AreEqual("const"  , method.Specifiers[2]);

    Assert.AreEqual(2, method.Params.Count);
    Assert.AreEqual("int"   , method.Params[0].Type.ToString());
    Assert.AreEqual("a_iInt", method.Params[0].Name);
    Assert.AreEqual("5"     , method.Params[0].Init);
    Assert.AreEqual("QString"      , method.Params[1].Type.ToString());
    Assert.AreEqual("a_strText"    , method.Params[1].Name);
    Assert.AreEqual("new QString()", method.Params[1].Init);
  }

  [TestMethod]
  public void ParseMethodComment() {
    string commentBlock =
      "  ///----------------------------------------------------------------------------------- \r\n" +
      "  /// Function: dgParser::method \r\n" +
      "  /// \r\n" +
      "  /// Summary : This is a summary \r\n" +
      "  ///           going over two lines \r\n" +
      "  /// \r\n" +
      "  /// Params  : QString a_string - This: It's a string! \r\n" +
      "  ///           int a_count - This is a variable declaration \r\n" +
      "  ///                         going over two lines \r\n" +
      "  /// \r\n" +
      "  /// Return  : bool - Parsing succeeded ! \r\n" +
      "  ///----------------------------------------------------------------------------------- \r\n";

    var config = new MethodConfig();
    var commentParser = new MethodCommentParser(config);
    MethodComment comment = commentParser.Parse(new List<string>(commentBlock.Split('\n')));

    Assert.AreEqual("This is a summary\r\ngoing over two lines", comment.Summary.Trim());

    Assert.AreEqual(2, comment.Params.Count);
    Assert.AreEqual("This: It's a string!", comment.Params.Find(pair => pair.Key == "QString a_string").Value);
    Assert.AreEqual("This is a variable declaration" + config.NewLine + "going over two lines", comment.Params.Find(pair => pair.Key == "int a_count").Value);

    Assert.AreEqual("bool", comment.Return.Key);
    Assert.AreEqual("Parsing succeeded !", comment.Return.Value);

    Assert.AreEqual(2, comment.Whitespace);
  }
}