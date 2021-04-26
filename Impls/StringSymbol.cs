using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LR_1.Impls
  {
  class StringSymbol : Symbol<StringSymbol>
    {
    public string s = "";
    public StringSymbol(){}
    public StringSymbol(string ss){ s = ss; }
    public override object Clone() => new StringSymbol(s.Clone() as string);

    public override StringSymbol GenerateNew(IEnumerable<StringSymbol> already_used, SymbolType wanted_type, Base_GenSymbolInfo gen_symb_info = null)
      {
      if(gen_symb_info != null && gen_symb_info is StdGenSymbolInfo std && std.wanted_prefix != null && std.wanted_prefix.Length != 0)
        {
        if(((StringSymbol)std.wanted_prefix).GetSymbolType() != wanted_type) throw new Exception("wrong wanted prefix");
        return FindNewWithStart(already_used, std.wanted_prefix);
        }

      switch(wanted_type)
        {
        case SymbolType.Epsilon: return "";
        case SymbolType.Terminal:
          {
          if(gen_symb_info != null && gen_symb_info is StdGenSymbolInfo std0 && std0.is_special_symbol)
            return FindNewWithStart(already_used, "$");
          var letters_amount = 'z' - 'a' + 1;
          var eng_letters = new bool[letters_amount];
          foreach(var x in already_used)
            if('a' <= x.s[0] && x.s[0] <= 'z') eng_letters[x.s[0] - 'a'] = true;
          for(int i = 0; i < letters_amount; i++) if(!eng_letters[i]) return (char)('a' + i);
          return FindNewWithStart(already_used, "t");
          }
        case SymbolType.NotTerminal:
          {
          var letters_amount = 'Z' - 'A' + 1;
          var eng_letters = new bool[letters_amount];
          foreach(var x in already_used)
            {
            if('A' <= x.s[0] && x.s[0] <= 'Z') eng_letters[x.s[0] - 'A'] = true;
            }
          for(int i = 0; i < letters_amount; i++) if(!eng_letters[i]) return (char)('A' + i);
          return FindNewWithStart(already_used, "N");
          }
        default: throw new NotImplementedException("wanted_type is not Eps, N, T ?!");
        }
      }

    private StringSymbol FindNewWithStart(IEnumerable<StringSymbol> a_used, string prefix)
      {
      if(prefix == null || prefix.Length == 0) throw new Exception("bad prefix (null or have zero len)");
      var x = prefix;
      var ss = (StringSymbol)x;
      if(!a_used.Contains(ss)) return ss;
      ss = (x + "*");
      if(!a_used.Contains(ss)) return ss;
      for(int i = 0; i < int.MaxValue; i++)
        {
        ss = (x + "_" + i);
        if(!a_used.Contains(ss)) return ss;
        }
      throw new Exception("too much elements in grammar (more 2*10^9)");
      }

    public override StringSymbol GetSpecial(int special_id)
      {
      if(special_id == 0) return "$";
      if(special_id == 1) return "$*";
      return "$_" + (special_id - 2);
      }

    public override SymbolType GetSymbolType()
      {
      if(s.Length == 0) return SymbolType.Epsilon;
      if(char.IsLower(s[0]) || s[0] == '$') return SymbolType.Terminal;
      if(char.IsUpper(s[0])) return SymbolType.NotTerminal;
      throw new NotImplementedException("strings in this type must have first symbol: {a..z, $} for T, {A..Z} for NT, nothing for Epsilon");
      }

    public static implicit operator StringSymbol(char c) { return new StringSymbol(c+""); }
    public static implicit operator StringSymbol(string s) { return new StringSymbol(s); }

    public override string ToString() => (s == "") ? "_eps_" : ""+s;

    public override bool Equals(object obj)
      {
      if(obj is StringSymbol o) return o.s == s; 
      return false;
      }

    public override int GetHashCode()
      {
      int ret = 1234; 
      for(int i = 0; i < s.Length && i < 10; i++) ret ^= (i + 2) * (i + 1) * s[i];
      return ret;
      }
    }
  }
