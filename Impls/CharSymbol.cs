using System;
using System.Collections.Generic;
using System.Linq;

namespace LR_1.Impls
  {
  class CharSymbol : Symbol<CharSymbol>
    {
    static readonly char EpsilonChar = '~';
    static readonly List<char> OtherT = new List<char>() { '+', '=', '-', '(', ')' };
    static readonly List<char> SpecialT = new List<char>() { '$', '#', '&', '.'};
    public char Value { get; protected set; }
    public CharSymbol(char c) { Value = c; }

    public CharSymbol() => Value = EpsilonChar;
    public override object Clone() { return new CharSymbol(Value); }

    public override CharSymbol GenerateNew(IEnumerable<CharSymbol> already_used, SymbolType wanted_type, Base_GenSymbolInfo gen_symb_info = null)
      {
      switch(wanted_type)
        {
        case SymbolType.Epsilon: return new CharSymbol(EpsilonChar);
        case SymbolType.Terminal:
          {
          if(gen_symb_info != null && gen_symb_info is StdGenSymbolInfo std && std.is_special_symbol)
            {
            foreach(var x in SpecialT)
              {
              CharSymbol cs = new CharSymbol(x); 
              if(!already_used.Contains(cs)) return cs;
              }
            throw new NotImplementedException("cant create not used symbol");
            }
          var letters_amount = 'z' - 'a' + 1;
          var eng_letters = new bool[letters_amount];
          var digits_amount = '9' - '0' + 1;
          var digits = new bool[digits_amount];
          foreach(var x in already_used)
            {
            if('a' <= x.Value && x.Value <= 'z') eng_letters[x.Value - 'a'] = true;
            if('0' <= x.Value && x.Value <= '9') digits[x.Value - '0'] = true;
            }
          for(int i = 0; i < letters_amount; i++) if(!eng_letters[i]) return new CharSymbol((char)('a' + i));
          for(int i = 0; i < digits_amount; i++) if(!digits[i]) return new CharSymbol((char)('0' + i));
          throw new NotImplementedException("cant create not used symbol");
          }
        case SymbolType.NotTerminal:
          {
          if(gen_symb_info != null && gen_symb_info is StdGenSymbolInfo std && std.wanted_prefix == "S")
            {
            CharSymbol cs = null;
            cs = new CharSymbol('S');
            if(!already_used.Contains(cs)) return cs;
            cs = new CharSymbol('Z');
            if(!already_used.Contains(cs)) return cs;
            cs = new CharSymbol('X');
            if(!already_used.Contains(cs)) return cs;
            }
          var letters_amount = 'Z' - 'A' + 1;
          var eng_letters = new bool[letters_amount];
          foreach(var x in already_used)
            {
            if('A' <= x.Value && x.Value <= 'Z') eng_letters[x.Value - 'A'] = true;
            }
          for(int i = 0; i < letters_amount; i++) if(!eng_letters[i]) return new CharSymbol((char)('A' + i));
          throw new NotImplementedException("cant create not used symbol");
          }
        default: throw new NotImplementedException("wanted_type is not Eps, N, T ?!");
        }

      }

    public override SymbolType GetSymbolType()
      {
      if(Value == EpsilonChar) return SymbolType.Epsilon;
      if(char.IsDigit(Value) || char.IsLower(Value) || OtherT.Contains(Value) || SpecialT.Contains(Value)) return SymbolType.Terminal;
      return SymbolType.NotTerminal;
      }

    public override bool Equals(object obj)
      {
      if(obj is CharSymbol o) return o.Value == Value;
      return false;
      }

    public override int GetHashCode() => Value;

    public static implicit operator CharSymbol(char c) { return new CharSymbol(c); }

    public override string ToString() => (Value == EpsilonChar) ? "_eps_" : "" + Value;


    public override CharSymbol GetSpecial(int special_id)
      {
      if(special_id < SpecialT.Count)return new CharSymbol(SpecialT[special_id]);
      throw new ArgumentException("that type has only "+ SpecialT.Count + " special symbols"); 
      }
    }
  }
