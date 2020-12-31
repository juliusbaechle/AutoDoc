using System;
using System.Collections.Generic;

namespace AutoDoc.Compiler {
  public class Parser {
    public Parser(IScanner a_scanner) {
      scanner = a_scanner;
      next();
    }

    // method -> { keywords } [type] nested_name parameter_list [cv]
    public Method ParseMethod() {
      if(m_token == EToken.KEYWORD_ACCESS) {
        next();
        next();
      }

      Method method = new Method();
      method.Specifiers = parseSpecifier();
      method.ReturnType = parseType();

      // Konstruktoren oder Operator-Umwandlungen haben keinen return type
      // In diesem Fall wird der Bezeichner als ReturnType-Bezeichner eingelesen
      if (m_text == "(") {
        method.QualifiedName = method.ReturnType.Name;
        method.ReturnType = new Type();
      } else {
        method.QualifiedName = parseNestedName();
      }
            
      method.Params = parseParamList();
      method.Specifiers.AddRange(parseSpecifiersAfterParams());
      return method;
    }

    private List<string> parseSpecifier() {
      var specifiers = new List<string>();
      while(m_token == EToken.KEYWORD_SPECIFIER) {
        specifiers.Add(m_text);
        next();
      }
      return specifiers;
    }

    private List<string> parseSpecifiersAfterParams() {
      var specifiers = new List<string>();
      while (m_token != EToken.END_OF_SCAN) {
        if (m_token == EToken.KEYWORD_CV)
          specifiers.Add(m_text);
        if (m_token == EToken.KEYWORD_SPECIFIER)
          specifiers.Add(m_text);
        next();
      }
      return specifiers;
    }

    // type -> ["const"|"virtual"] nested_name ptr_spec
    private Type parseType() {
      Type type = new Type();      
      type.KeywordCV = parseCV();
      type.Name = parseNestedName();
      type.PtrSpec = parsePointerSpec();
      return type;
    }

    // ptr_spec -> (("*"|"&"|"&&") ["const"|"virtual"]) | EMPTY
    private PtrSpec parsePointerSpec() {
      if (m_text != "*" && m_text != "&" && m_text != "&&")
        return new PtrSpec();
      
      var ptrSpec = new PtrSpec();
      ptrSpec.PtrOrRef = m_text;
      next();
      ptrSpec.KeywordCV = parseCV();
      ptrSpec.RecPtrSpec = parsePointerSpec();
      return ptrSpec;
    }

    private string parseCV() {
      if (m_token == EToken.KEYWORD_CV) {
        string cv = m_text;
        next();
        return cv;
      }
      return "";
    }

    // nested_name -> ["::"] name { "::" name }
    private NestedName parseNestedName() {
      check("::");

      NestedName name = new NestedName();
      do {
        name.Names.Add(parseName());
      } while (check("::"));
      return name;
    }

    // name -> id ["<" typeList ">"] | "~"id | "operator (id|op)"
    // TODO: Support std::function
    private TypeName parseName() {
      TypeName name = new TypeName();

      // Destruktor
      if(m_text == "~") {
        next();
        name.Id = "~" + m_text;
        next();
        return name;
      }

      // operator
      if (m_text == "operator") {
        name.Id = "operator ";
        next();
        do {          
          if (m_text != "")
            name.Id += m_text;
          else
            name.Id += (char)m_token;
          next();
        } while (m_text != "(");
        return name;
      }

      // normal
      assert(EToken.IDENTIFIER);
      name.Id = m_text;
      next();

      // template
      if (check("<")) {
        name.TemplateTypes = parseTypeList();
        assert(">");
        next();
      }
      
      return name;
    }

    // typeList -> type { "," type }
    private List<Type> parseTypeList() {
      List<Type> typeList = new List<Type>();
      do {
        typeList.Add(parseType());
      } while (check(","));
      return typeList;
    }

    // param_list -> "(" param { "," param } ")" | EMPTY
    private List<Param> parseParamList() {
      assert("("); next();
      if (m_text == ")")
        return new List<Param>();

      var parameters = new List<Param>();
      do {
        parameters.Add(parseParam());
      } while (check(","));

      assert(")"); next();
      return parameters;
    }
    
    // param -> type [id] ["=" initializer]
    private Param parseParam() {
      Param param = new Param();
      param.Type = parseType();

      if (m_token == EToken.IDENTIFIER) {
        param.Name = m_text;
        next();
      }

      if (m_text == "=")
        param.Default = parseInitializer();

      return param;
    }
    
    // TODO: Remove this Hack (scanned text for intializer)
    private string parseInitializer() {
      scanner.ScannedText = "";
      next();

      int brace = 0;
      while (true) {
        next();

        if (m_text == ",")
          break;

        if (m_text == "(")
          brace++;

        if (m_text == ")") {
          if (brace == 0) break;
          if (brace > 0) brace--;
        }
      }

      string scannedText = scanner.ScannedText.Trim();
      return scannedText.Substring(0, scannedText.LastIndexOf(m_text));
    }

    private void next() {
      do {
        m_text = scanner.getToken();
        m_token = scanner.TokenType;
      } while (m_token == EToken.COMMENT || m_token == EToken.INVALID);
    }

    // throws exception if token was not correct and calls next()
    private void assert(EToken a_token) {
      if (m_token != a_token)
        throw new Exception("Exspected token: " + a_token.ToString() + ", got: " + m_token.ToString());
    }

    // throws exception if text was not correct and calls next()
    private void assert(string a_text) {
      if (m_text != a_text)
        throw new Exception("Exspected token: " + a_text + ", got: " + m_text);
    }

    // calls next() and returns true if token was correct
    private bool check(EToken a_token, string a_text = "") {
      if (m_token != a_token)
        return false;
      if (a_text != "" && a_text != m_text)
        return false;

      next();
      return true;
    }

    // calls next() and returns true if text was correct
    private bool check(string a_text = "") {
      if (a_text != m_text)
        return false;

      next();
      return true;
    }

    private IScanner scanner;
    private EToken m_token;
    private string m_text;
  }
}
