using System;
using System.Collections.Generic;

namespace LR_1
  {

  enum RulePart
    {
    Left,
    Right
    }

  class Rule<S> where S : Symbol<S>//, new()
    {
    private List<S> left;
    private List<S> right;

    /// <summary>only for read</summary>
    public List<S> GetRulePart(RulePart rule_part) => (rule_part == RulePart.Left) ? left : right;

    public int GetRuleLen(RulePart rule_part) => (rule_part == RulePart.Left) ? left.Count : right.Count;
    public S GetSymbol(RulePart rule_part, int ind)
      {
      var r_part = (rule_part == RulePart.Left) ? left : right;
      if(ind < 0 || r_part.Count <= ind) throw new IndexOutOfRangeException("index must be in [0; GetRuleLen(rule_part))");
      return r_part[ind].Clone() as S;
      }

    /// <summary>Rarely used</summary>
    /// <param name="ind">index before adding symbol</param>
    public void AddSymbol(int ind, S symbol, RulePart rule_part = RulePart.Right)
      {
      var r_part = (rule_part == RulePart.Left) ? left : right;
      if(ind < 0 || r_part.Count < ind) throw new IndexOutOfRangeException("index must be in [0; GetRuleLen(rule_part)]");
      if(ind == r_part.Count) r_part.Add(symbol);
      else r_part.Insert(ind, symbol);
      }

    public Rule(List<S> left, List<S> right, bool take_owning = true)
      {
      if(take_owning)
        {
        this.left = left;
        this.right = right;
        }
      else
        {
        this.left = new List<S>(left);
        this.right = new List<S>(right);
        }
      }

    public Rule(Rule<S> copy)
      {
      left = new List<S>(copy.left);
      right = new List<S>(copy.right);
      }

    public override int GetHashCode()
      {
      int ret = left.Count * 131 + right.Count * 2;
      for(int i = 0; i < left.Count && i < 2; i++) ret ^= (i + 1) * (i + 1) * left[i].GetHashCode();
      for(int i = 0; i < right.Count && i < 4; i++) ret ^= (i + 1) * (i + 1) * right[i].GetHashCode();
      return ret;
      }
    }

  class RuleComparer<S> : IEqualityComparer<Rule<S>> where S : Symbol<S>///, new()
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
