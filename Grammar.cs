﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace LR_1
  {
  class Grammar<S> where S : Symbol<S>, new()
    {
    IEqualityComparer<S> symbol_comparator;
    ChainComparer<S> chain_comparator { get => rule_comparator.chain_comparer; }
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
      AddTerminal(r.GetRulePart(RulePart.Left).GetChainSymbols());
      AddTerminal(r.GetRulePart(RulePart.Right).GetChainSymbols());
      }

    private void AddNotTerminal(Rule<S> r)
      {
      AddNotTerminal(r.GetRulePart(RulePart.Left).GetChainSymbols());
      AddNotTerminal(r.GetRulePart(RulePart.Right).GetChainSymbols());
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

    public HashSet<S> First(Chain<S> chain)
      {
      var ret = new HashSet<S>();
      foreach(var z in chain.GetChainSymbols())
        {
        var union = First(z);
        ret.UnionWith(union);
        if(!union.Contains(Symbol<S>.Epsilon)) break;
        }
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
        ret.UnionWith(First(x.GetRulePart(RulePart.Right)));
        }
      return ret;
      }

    public IEnumerable<Rule<S>> GetWithLeft(Chain<S> chain) => rules.Where(x => chain_comparator.Equals(chain, x.GetRulePart(RulePart.Left)));

    public HashSet<ClosureElem<S>> Closure(HashSet<ClosureElem<S>> items)
      {
      while(true)
        {
        int len = items.Count;
        foreach(var closure_elem in items)
          {
          foreach(var r in GetWithLeft(closure_elem.SymbolAfterPoint))
            {
            var first = First(closure_elem.SymbolsAfterFirstAfterPoint + closure_elem.symbol);
            foreach(var b in first) items.Add(new ClosureElem<S>(r, 0, b, symbol_comparator));
            }
          }
        if(items.Count == len) break;
        }
      return items;
      }

    }

  }
