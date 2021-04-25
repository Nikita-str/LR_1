using System;
using System.Collections.Generic;
using System.Linq;

namespace LR_1
  {
  class Grammar<S> where S : Symbol<S>//, new()
    {
    IEqualityComparer<S> symbol_comparator;
    RuleComparer<S> rule_comparator;
    HashSet<Rule<S>> rules;

    /// <summary>Terminal symbols</summary>
    HashSet<S> T;
    /// <summary>Not terminal symbols</summary>
    HashSet<S> N;

    public S StartSymbol { get; set; } = null;

    private int amount_not_CF_left = 0;
    public bool isCF => (amount_not_CF_left == 0);

    public Grammar(EqualityComparer<S> s_comparator)
      {
      symbol_comparator = (s_comparator == null) ? EqualityComparer<S>.Default : s_comparator;
      rule_comparator = new RuleComparer<S>(symbol_comparator);
      rules = new HashSet<Rule<S>>(rule_comparator);
      T = new HashSet<S>(symbol_comparator);
      N = new HashSet<S>(symbol_comparator);
      }

    private void AddTerminal(IEnumerable<S> from) => T.UnionWith(from.Where(o => o.isTerminal));
    private void AddNotTerminal(IEnumerable<S> from) => N.UnionWith(from.Where(o => o.isNotTerminal));
    private void AddTerminal(Rule<S> r)
      {
      AddTerminal(r.GetRulePart(RulePart.Left));
      AddTerminal(r.GetRulePart(RulePart.Right));
      }

    private void AddNotTerminal(Rule<S> r)
      {
      AddNotTerminal(r.GetRulePart(RulePart.Left));
      AddNotTerminal(r.GetRulePart(RulePart.Right));
      }

    public bool AddRule(Rule<S> rule)
      {
      if(!rules.Add(rule)) return false;
      if(rule.GetRuleLen(RulePart.Left) != 1) amount_not_CF_left++;
      if(!rule.GetSymbol(RulePart.Left, 0).isNotTerminal) amount_not_CF_left++;
      AddTerminal(rule);
      AddNotTerminal(rule);
      return true;
      }

    public Dictionary<S, HashSet<S>> FirstAllNotTerminal()
      {
      var ret = new Dictionary<S, HashSet<S>>();
      foreach(var x in N) { ret.Add(x.Clone() as S, First(x)); }
      return ret;
      }

    public HashSet<S> First(S symbol)
      {
      var ret = new HashSet<S>();
      if(symbol.isEpsilon | symbol.isTerminal) { ret.Add(symbol.Clone() as S); return ret; }

      if(!symbol.isNotTerminal) throw new NotImplementedException("symbol is not Terminal, NotTerminal and not epsilon, what is it?");

      if(!isCF) throw new NotImplementedException("curently FIRST realised only for CF grammar");
      foreach(var x in rules.Where(r => symbol_comparator.Equals(r.GetSymbol(RulePart.Left, 0), symbol)))
        {
        var right = x.GetRulePart(RulePart.Right);
        foreach(var z in right)
          {
          var union = First(z);
          ret.UnionWith(union);
          if(!union.Contains(symbol.Epsilon)) break;
          }
        }
      return ret;
      }
    }

  }
