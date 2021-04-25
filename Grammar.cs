using System;
using System.Collections.Generic;
using System.Linq;

namespace LR_1
  {
  class Grammar<S> where S : Symbol<S>, new()
    {
    public IEqualityComparer<S> symbol_comparator { get => chain_comparator.symbol_comparator; }
    public ChainComparer<S> chain_comparator { get => rule_comparator.chain_comparer; }
    public RuleComparer<S> rule_comparator { get; private set; }

    HashSet<Rule<S>> rules;

    /// <summary>Terminal symbols</summary>
    HashSet<S> T;
    /// <summary>Not terminal symbols</summary>
    HashSet<S> N;


    public HashSet<S> GetAllGrammarSymbol()
      {
      var ret = new HashSet<S>(symbol_comparator);
      ret.Add(EndSymbol);
      ret.UnionWith(T);
      ret.UnionWith(N);
      return ret;
      }

    public S StartSymbol { get; set; } = null;
    public void SetStartSymbol(S new_start_symbol) => StartSymbol = new_start_symbol;

    /// <summary>special symbol for end of string</summary>
    public S EndSymbol { get; } = Symbol<S>.GetSpecialSymbol(0);

    private int amount_not_CF_left = 0;
    public bool isCF => (amount_not_CF_left == 0);

    public Grammar(EqualityComparer<S> s_comparator)
      {
      if (s_comparator == null) s_comparator = EqualityComparer<S>.Default;
      rule_comparator = new RuleComparer<S>(s_comparator);
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
      bool is_end = symbol_comparator.Equals(symbol, EndSymbol);
      if(is_end || symbol.isEpsilon || symbol.isTerminal) { ret.Add(symbol.Clone() as S); return ret; }

      if(!symbol.isNotTerminal) throw new NotImplementedException("symbol is not Terminal, NotTerminal and not epsilon, what is it?");

      if(!isCF) throw new NotImplementedException("curently FIRST realised only for CF grammar");
      foreach(var x in rules.Where(r => symbol_comparator.Equals(r.GetSymbol(RulePart.Left, 0), symbol)))
        {
        ret.UnionWith(First(x.GetRulePart(RulePart.Right)));
        }
      return ret;
      }

    public IEnumerable<Rule<S>> GetWithLeft(Chain<S> chain) => rules.Where(x => chain_comparator.Equals(chain, x.GetRulePart(RulePart.Left)));

    public HashSet<ClosureElem<S>> Closure(HashSet<ClosureElem<S>> items, bool can_change_items = true)
      {
      if(!can_change_items) items = new HashSet<ClosureElem<S>>(items);
      while(true)
        {
        var items_clone = new HashSet<ClosureElem<S>>(items);
        foreach(var closure_elem in items_clone)
          {
          foreach(var r in GetWithLeft(closure_elem.SymbolAfterPoint))
            {
            var first = First(closure_elem.SymbolsAfterFirstAfterPoint + closure_elem.symbol);
            foreach(var b in first) items.Add(new ClosureElem<S>(r, 0, b, symbol_comparator));
            }
          }
        if(items_clone.Count == items.Count) break;
        }
      return items; 
      }

    public HashSet<ClosureElem<S>> Goto(HashSet<ClosureElem<S>> items, Symbol<S> symbol)
      {
      var for_ret = new HashSet<ClosureElem<S>>();
      foreach(var closure in items)
        {
        if(!symbol_comparator.Equals(closure.SymbolAfterPoint, symbol as S)) continue; 
        for_ret.Add(new ClosureElem<S>(closure.rule, closure.position + 1, closure.symbol, symbol_comparator));
        }
      return Closure(for_ret);
      }

    /// <summary>Grammar must be already added with S'->S where S is previos start symbol</summary>
    /// <returns></returns>
    public List<HashSet<ClosureElem<S>>> Items()
      {
      var must_have_len_eq_1 = GetWithLeft(StartSymbol);
      if(must_have_len_eq_1.Count() != 1) throw new ArgumentException("self must be adding with S'->S where S is preious start subol and S' new");
      var start_rule = must_have_len_eq_1.First();
      var I_0 = Closure(new HashSet<ClosureElem<S>>() { new ClosureElem<S>(start_rule, 0, EndSymbol, symbol_comparator) });

      var all_grammar_symbol = GetAllGrammarSymbol();
      all_grammar_symbol.Remove(StartSymbol); // S' is additional not initially

      //TODO: sort all_grammar_symbol

      var state = new Dictionary<(int, int), S>() { };// <(from, to), by symbol> 
      var ret = new List<HashSet<ClosureElem<S>>>() { I_0 };

      while(true)
        {
        int last_len = ret.Count;
        var added = new List<HashSet<ClosureElem<S>>>();
        for(int i = 0; i < last_len; i++)
          {
          var I = ret[i];
          foreach(var X in all_grammar_symbol)
            {
            var added_I = Goto(I, X);
            if(added_I.Count != 0)
              {
              bool add = true;

              #region oh.. no... check that added_I not contain in ret
              for(int exist_i = 0; exist_i < last_len; exist_i++)
                {
                var exist_I = ret[exist_i];
                bool is_eq = true;
                foreach(var item in exist_I)
                  {
                  if(!added_I.Contains(item)) is_eq = false;
                  if(is_eq) break;
                  }
                if(is_eq)
                  {
                  var from_to = (i, exist_i);
                  if(state.ContainsKey(from_to)) {if(!symbol_comparator.Equals(state[from_to], X)) throw new Exception("... blin");}
                  else state.Add(from_to, X); // from: I[i] to I[last_len + added.Count] by X-symbol
                  add = false;
                  }
                if(!add) break;
                }
              #endregion oh.. no... ...

              if(add)
                {
                state.Add((i, last_len + added.Count), X); // from: I[i] to I[last_len + added.Count] by X-symbol
                added.Add(added_I);
                }
              }
            }
          }

        ret.AddRange(added);
        if(ret.Count == last_len) break;
        }

      Console.WriteLine("states: ");
      foreach(var x in state)
        Console.WriteLine("I[" + x.Key.Item1 + "]  -> (by " + x.Value + ") -> " + "I[" + x.Key.Item2 + "]");
      Console.WriteLine();

      return ret;
      }

    }

  }
