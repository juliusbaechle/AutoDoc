using AutoDoc.Compiler;
using System;
using System.Collections.Generic;

namespace AutoDoc.Compiler {
  public class Parser {
    public Parser(IScanner a_scanner) {
      scanner = a_scanner;
      next();
    }

    // method -> { keywords } [type] nested_name "(" parameter_list ")" [cv]
    public Method ParseMethod() {
      Method method = new Method();
      method.Specifiers = parseSpecifier();

      // Return type
      method.ReturnType = parseType();

      // Konstruktoren oder Operator-Umwandlungen haben keinen return type
      // In diesem Fall wird der Bezeichner als ReturnType-Bezeichner eingelesen
      if (m_text == "(") {
        method.QualifiedName = method.ReturnType.Name;
        method.ReturnType = new Type();
      } else {
        method.QualifiedName = parseNestedName();
      }

      // Parameter
      assert("("); next();
      method.Params = parseParamList();
      assert(")"); next();

      // const / noexcept / ...
      if(m_token == EToken.KEYWORD_CV)
        method.Specifiers.Add(parseCV());

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
    private QualifiedName parseNestedName() {
      check("::");

      QualifiedName name = new QualifiedName();
      do {
        name.Names.Add(parseName());
      } while (check("::"));
      return name;
    }

    // name -> id ["<" typeList ">"]
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

    // paramList -> (param { "," param }) | EMPTY
    private List<Param> parseParamList() {
      var parameters = new List<Param>();
      if (m_text == ")") return parameters;

      do {
        parameters.Add(parseParam());
      } while (check(","));
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
        param.Init = parseInitializer();

      return param;
    }
    
    // TODO: parseInitializer: remove this hack
    private string parseInitializer() {
      int start = scanner.Position;
      int end = start;
      next();

      int brace = 0;
      while (true) {
        end = scanner.Position;
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
    
      string init = scanner.Code.Substring(start - 1, end - start);
      return init.Trim();
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
