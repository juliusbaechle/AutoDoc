using System.Collections.Generic;

namespace AutoDoc.Compiler {
  public class Method {
    public List<string> Specifiers = new List<string>();
    public Type ReturnType = new Type();
    public QualifiedName QualifiedName = new QualifiedName();
    public List<Param> Params = new List<Param>();
  }

  public class Param {
    public Type Type = new Type();
    public string Name = "";
    public string Init = "";
  }

  public class Type {
    public override string ToString() {
      string str = "";
      if (KeywordCV.Length > 0)
        str = KeywordCV + " ";
      str += Name.ToString();
      str += PtrSpec.ToString();
      return str;
    }

    public string KeywordCV = "";
    public QualifiedName Name = new QualifiedName();
    public PtrSpec PtrSpec = new PtrSpec();
  }

  public class PtrSpec {
    public override string ToString() {
      string str = PtrOrRef;
      if(KeywordCV != "")
        str += " " + KeywordCV;
      if(RecPtrSpec != null)
        str += RecPtrSpec.ToString();
      return str;
    }

    public string PtrOrRef = "";
    public string KeywordCV = "";
    public PtrSpec RecPtrSpec = null;
  }

  public class QualifiedName {
    public override string ToString() {
      string name = "";
      for (int i = 0; i < Names.Count; i++) {
        if (i > 0) name += "::";
        name += Names[i].ToString();
      }
      return name;
    }

    public List<TypeName> Names = new List<TypeName>();
  }

  public class TypeName {
    public override string ToString() {
      string str = Id;
      if (TemplateTypes.Count > 0) {
        str += "<" + TemplateTypes[0].ToString();
        for (int i = 1; i < TemplateTypes.Count; i++)
          str += ", " + TemplateTypes[i].ToString();
        str += ">";
      }
      return str;
    }

    public string Id = "";
    public List<Type> TemplateTypes = new List<Type>();
  }
}
