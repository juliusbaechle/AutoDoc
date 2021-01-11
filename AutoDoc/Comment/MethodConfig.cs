namespace AutoDoc.Comment {
  public class MethodConfig {
    public string UpperLimiter   = "///-----------------------------------------------------------------------------------";
    public string EmptyLine      = "///";
    public string LowerLimiter   = "///-----------------------------------------------------------------------------------";
    public string NewLine        = "\r\n";

    public string Signature  = "Function";
    public string Summary    = "Summary";
    public string Changed    = "CHANGED";

    public string Parameter1 = "\\param";
    public string Parameter2 = "\\param";
    public string Return     = "\\return";

    public int AlignColon = 8;
    public int AlignText = 10;
  }
}
