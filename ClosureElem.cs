using System;
using System.Collections.Generic;
using System.Linq;

namespace LR_1
  {
  class ClosureElem<S> where S: Symbol<S>, new()
    {
    IEqualityComparer<S> s_comparer { get => r_comparer.chain_comparer.symbol_comparator; }
    RuleComparer<S> r_comparer;

    public Rule<S> rule { get; }
    public int position { get; }
    public S symbol { get; }

    public ClosureElem(Rule<S> r, int pos, S symb, IEqualityComparer<S> comparer)
      {
      rule = r;
      position = pos;
      symbol = symb;
      r_comparer = new RuleComparer<S>(comparer);
      }

    public override bool Equals(object obj)
      {
      if(obj is ClosureElem<S> y)
        {
        var x = this;
        if(x.position != y.position) return false;
        if(!s_comparer.Equals(x.symbol, y.symbol)) return false;
        if(!r_comparer.Equals(x.rule, y.rule)) return false;
        return true;
        }
      return false;
      }


    /// <summary>return B from [alpha . B beta; symbol]</summary>
    public S SymbolAfterPoint { get => rule.GetRulePart(RulePart.Right)[position]; }
    /// <summary>return beta from [alpha . B beta; symbol]</summary>
    public Chain<S> SymbolsAfterFirstAfterPoint { get => rule.GetRulePart(RulePart.Right).SubChain(position + 1); }

    public override int GetHashCode() => (rule.GetHashCode() + position * 17) ^ (symbol.GetHashCode() << 1);

    public override string ToString()
      {
      var right = "";
      var r_part_len = rule.GetRuleLen(RulePart.Right);
      if(r_part_len == 1 && rule.GetSymbol(RulePart.Right, 0).isEpsilon) right = ".";
      else
        {
        for(int i = 0; i < r_part_len; i++)
          {
          if(position == i) right += ".";
          right += rule.GetSymbol(RulePart.Right, i).ToString();
          }
        if(position == r_part_len) right += ".";
        }
      return "[" + rule.GetRulePart(RulePart.Left) + " -> " + right + "; " + symbol + "]";
      }
    }
  }
