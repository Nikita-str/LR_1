using System;
using System.Collections.Generic;

namespace LR_1
  {

  enum RulePart
    {
    Left,
    Right
    }

  class Rule<S> where S : Symbol<S>
    {
    private Chain<S> left;
    private Chain<S> right;

    /// <summary>only for read</summary>
    public Chain<S> GetRulePart(RulePart rule_part) => (rule_part == RulePart.Left) ? left : right;

    public int GetRuleLen(RulePart rule_part) => (rule_part == RulePart.Left) ? left.Length : right.Length;
    public S GetSymbol(RulePart rule_part, int ind) => GetRulePart(rule_part)[ind];

    /// <summary>Rarely used</summary>
    /// <param name="ind">index before adding symbol</param>
    public void AddSymbol(int ind, S symbol, RulePart rule_part = RulePart.Right) => GetRulePart(rule_part).AddSymbol(ind, symbol);

    public Rule(List<S> left, List<S> right, bool take_owning = true)
      {
      this.left = new Chain<S>(left, take_owning);
      this.right = new Chain<S>(right, take_owning);
      }

    public Rule(Rule<S> copy)
      {
      left = new Chain<S>(copy.left);
      right = new Chain<S>(copy.right);
      }

    public override int GetHashCode()
      {
      int ret = left.Length * 131 + right.Length * 2;
      ret ^= left.GetHashCode(2);
      ret ^= right.GetHashCode(4);
      return ret;
      }
    }

  class RuleComparer<S> : IEqualityComparer<Rule<S>> where S : Symbol<S>
    {
    IEqualityComparer<S> symbol_comparator;
    public RuleComparer(IEqualityComparer<S> s_comparator)
      {
      symbol_comparator = s_comparator;
      if(symbol_comparator == null) symbol_comparator = EqualityComparer<S>.Default;
      }

    static private readonly List<RulePart> rule_parts = new List<RulePart>() { RulePart.Left, RulePart.Right };
    public bool Equals(Rule<S> x, Rule<S> y)
      {
      foreach(var cur_part in rule_parts)
        { if(x.GetRuleLen(cur_part) != y.GetRuleLen(cur_part)) return false; }

      foreach(var cur_part in rule_parts)
        {
        var len = x.GetRuleLen(cur_part);
        for(int i = 0; i < len; i++)
          if(!symbol_comparator.Equals(x.GetSymbol(RulePart.Left, i), y.GetSymbol(RulePart.Left, i))) return false;
        }
      return true;
      }

    public int GetHashCode(Rule<S> obj) => obj.GetHashCode();
    }

  }
