namespace LR_1
  {
  abstract class Base_GenSymbolInfo { };

  class StdGenSymbolInfo : Base_GenSymbolInfo
    {
    public string wanted_prefix;
    public bool is_special_symbol;
    public StdGenSymbolInfo(string prefix, bool is_special)
      {
      wanted_prefix = prefix;
      is_special_symbol = is_special;
      }
    }
  }
