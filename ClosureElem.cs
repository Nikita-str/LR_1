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
      => "[" + rule.GetRulePart(RulePart.Left) + " -> " + rule.GetRulePart(RulePart.Right).ToString().Insert(position, ".") + "; " + symbol + "]";
    }
  }
