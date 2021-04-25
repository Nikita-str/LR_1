﻿using System;
using System.Collections.Generic;

namespace LR_1
  {

  enum RulePart
    {
    Left,
    Right
    }

  class Rule<S> where S : Symbol<S>, new()
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

    public Rule(Chain<S> left, Chain<S> right, bool take_owning = true)
      {
      if(take_owning)
        {
        this.left = left;
        this.right = right;
        }
      else
        {
        this.left = new Chain<S>(left);
        this.right = new Chain<S>(right); 
        }
      }

    public Rule(Rule<S> copy)
      {
      left = new Chain<S>(copy.left);
      right = new Chain<S>(copy.right);
      }

    static private readonly List<RulePart> rule_parts = new List<RulePart>() { RulePart.Left, RulePart.Right };
    public override bool Equals(object obj)
      {
      if(obj is Rule<S> y)
        {
        var x = this;
        foreach(var cur_part in rule_parts)
          if(!x.GetRulePart(cur_part).Equals(y.GetRulePart(cur_part))) return false; 
        return true;
        }
      return false;
      }

    public override int GetHashCode()
      {
      int ret = left.Length * 131 + right.Length * 2;
      ret ^= left.GetHashCode(2);
      ret ^= right.GetHashCode(4);
      return ret;
      }

    public override string ToString() => left + " -> " + right;
    }

  class RuleComparer<S> : IEqualityComparer<Rule<S>> where S : Symbol<S>, new()
    {
    public ChainComparer<S> chain_comparer;
    public RuleComparer(IEqualityComparer<S> s_comparator)
      {
      if(s_comparator == null) s_comparator = EqualityComparer<S>.Default;
      chain_comparer = new ChainComparer<S>(s_comparator);
      }

    static private readonly List<RulePart> rule_parts = new List<RulePart>() { RulePart.Left, RulePart.Right };
    public bool Equals(Rule<S> x, Rule<S> y)
      {
      foreach(var cur_part in rule_parts)
          if(!chain_comparer.Equals(x.GetRulePart(cur_part), y.GetRulePart(cur_part))) return false;
      return true;
      }
    
    public int GetHashCode(Rule<S> obj) => obj.GetHashCode();
    }

  }
