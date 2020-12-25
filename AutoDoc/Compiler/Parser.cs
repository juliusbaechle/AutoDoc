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
      if (m_token == '(') {
        method.QualifiedName = method.ReturnType.Name;
        method.ReturnType = new Type();
      } else {
        method.QualifiedName = parseNestedName();
      }

      // Parameter
      assert('('); next();
      method.Params = parseParamList();
      assert(')'); next();

      // const / noexcept / ...
      if(m_token == (int)EToken.KEYWORD_CV)
        method.Specifiers.Add(parseCV());

      return method;
    }

    private List<string> parseSpecifier() {
      var specifiers = new List<string>();
      while(m_token == (int)EToken.KEYWORD_SPECIFIER) {
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
      if (m_token != '*' && m_token != '&' && m_token != (int)EToken.RVALUE_REF)
        return new PtrSpec();
      
      var ptrSpec = new PtrSpec();
      ptrSpec.PtrOrRef = (m_token == (int)EToken.RVALUE_REF) ? "&&" : ((char)m_token).ToString();
      next();
      ptrSpec.KeywordCV = parseCV();
      ptrSpec.RecPtrSpec = parsePointerSpec();
      return ptrSpec;
    }

    private string parseCV() {
      if (m_token == (int)EToken.KEYWORD_CV) {
        string cv = m_text;
        next();
        return cv;
      }
      return "";
    }

    // nested_name -> ["::"] name { "::" name }
    private QualifiedName parseNestedName() {
      check((int)EToken.QUALIFIER);

      QualifiedName name = new QualifiedName();
      do {
        name.Names.Add(parseName());
      } while (check((int)EToken.QUALIFIER));
      return name;
    }

    // name -> id ["<" typeList ">"]
    // TODO: Support std::function
    private TypeName parseName() {
      TypeName name = new TypeName();

      // Destruktor
      if(m_token == '~') {
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
        } while (m_token != '(');
        return name;
      }

      // normal
      assert((int)EToken.IDENTIFIER);
      name.Id = m_text;
      next();

      // template
      if (check('<')) {
        name.TemplateTypes = parseTypeList();
        assert('>');
        next();
      }
      
      return name;
    }

    // typeList -> type { "," type }
    private List<Type> parseTypeList() {
      List<Type> typeList = new List<Type>();
      do {
        typeList.Add(parseType());
      } while (check(','));
      return typeList;
    }

    // paramList -> (param { "," param }) | EMPTY
    private List<Param> parseParamList() {
      var parameters = new List<Param>();
      if (m_token == ')') return parameters;

      do {
        parameters.Add(parseParam());
      } while (check(','));
      return parameters;
    }
    
    // param -> type [id] ["=" initializer]
    private Param parseParam() {
      Param param = new Param();
      param.Type = parseType();

      if (m_token == (int)EToken.IDENTIFIER) {
        param.Name = m_text;
        next();
      }

      if (m_token == '=')
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

        if (m_token == ',')
          break;

        if (m_token == '(')
          brace++;

        if (m_token == ')') {
          if (brace == 0) break;
          if (brace > 0) brace--;
        }
      }
    
      string init = scanner.Code.Substring(start - 1, end - start);
      return init.Trim();
    }

    private void next() {
      do {
        m_token = scanner.getToken();
        m_text = scanner.Text;
      } while (m_token == (int)EToken.COMMENT);
    }

    // throws exception if token was not correct and calls next()
    private void assert(int a_token) {
      if (m_token != a_token)
        throw new Exception("Exspected token: " + ToString(a_token) + ", got: " + ToString(m_token));
    }

    private string ToString(int a_token) {
      if(a_token < 128) {
        return "" + (char)a_token;
      } else {
        return ((EToken)a_token).ToString() + ": " + m_text;
      }
    }

    // calls next() and returns true if token was correct
    private bool check(int a_token, string a_text = "") {
      if(m_token == a_token && (a_text == "" || a_text == m_text)) {
        next();
        return true;
      }
      return false;
    }

    private IScanner scanner;
    private int m_token;
    private string m_text;
  }
}
