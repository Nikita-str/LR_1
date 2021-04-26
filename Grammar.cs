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

    int id = 0;
    int next_id { get => id++; }
    Dictionary<Rule<S>, int> rules_id = new Dictionary<Rule<S>, int>();

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
      T.Add(EndSymbol);
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
      rules_id.Add(rule, next_id);
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


    public class ItemsReturn
      {
      /// <summary> [Key: (from, to); Value: by symbol] </summary>
      public Dictionary<(int, int), S> transitions;
      public List<HashSet<ClosureElem<S>>> items;
      public ItemsInfo items_info;

      public ItemsReturn(List<HashSet<ClosureElem<S>>> _items, Dictionary<(int, int), S> _transitions, ItemsInfo i_info)
        {
        items = _items;
        transitions = _transitions;
        items_info = i_info;
        }

      public void PrintTransitions(int tab_sz = 3)
        {
        string tab = new string(' ', tab_sz);
        foreach(var x in transitions)
          Console.WriteLine(tab + "I[" + x.Key.Item1 + "]  -> (by " + x.Value + ") -> " + "I[" + x.Key.Item2 + "]");
        }

      public void PrintItems(int tab_sz = 3)
        {
        string tab = new string(' ', tab_sz);
        int index = 0; 
        foreach(var I in items)
          {
          Console.WriteLine("I[" + index++ + "]: ");
          foreach(var closure in I)
            Console.WriteLine(tab + closure);
          }
        }

      public void PrintGotoTable()
        {
        var all_N = items_info.all_NT_symbols;
        int symbols = all_N.Count;

        Console.WriteLine("GOTO:");
        int max_len = 3;
        Dictionary<int, string> nt_strings = new Dictionary<int, string>();
        foreach(var c in all_N)
          {
          var cur = c.Key.ToString();
          nt_strings.Add(c.Value, cur);
          if(max_len < cur.Length) max_len = cur.Length;
          }
        
        var len_cell = max_len + 2; 
        Console.Write(new string(' ', len_cell) + "|");
        for(int i = 0; i < symbols; i++) Console.Write(" " + nt_strings[i].PadRight(len_cell - 1) + "|");
        Console.WriteLine();
        Console.WriteLine("".PadLeft((len_cell + 1) * (symbols + 1), '-'));
        var table = new int[items.Count, symbols];
        foreach(var x in transitions)
          if(x.Value.isNotTerminal)table[x.Key.Item1, all_N[x.Value]] = x.Key.Item2 + 1;

        for(int i = 0; i < items.Count; i++)
          {
          Console.Write((" "+i).PadRight(len_cell) + "|");
          for(int j = 0; j < symbols; j++) Console.Write( " " + ( table[i, j] == 0 ? "" : ""+(table[i,j] - 1) ).PadRight(len_cell - 1) + "|");
          Console.WriteLine();
          }
        }

      public void PrintActionTable()
        {
        Console.WriteLine("Action:");

        }
      }

    public class ItemsInfo
      {
      public S StartS;
      public S BackStartS;
      public Dictionary<S, int> all_NT_symbols;
      public Dictionary<S, int> all_T_symbols;

      public ItemsInfo(List<S> all_NT, List<S> all_T, S start_cur, S start_prev)
        {
        all_NT_symbols = new Dictionary<S, int>();
        for(int i = 0; i < all_NT.Count; i++) all_NT_symbols.Add(all_NT[i], i);
        all_T_symbols = new Dictionary<S, int>();
        for(int i = 0; i < all_T.Count; i++) all_T_symbols.Add(all_T[i], i);
        StartS = start_cur;
        BackStartS = start_prev;
        }
      }

    static readonly Exception EXC_NotNormalized = new ArgumentException("self must be adding with S'->S where S is preious start subol and S' new");

    /// <summary>Grammar must be already added with S'->S where S is previos start symbol</summary>
    /// <returns></returns>
    public ItemsReturn Items(Comparison<S> comparer_for_sort = null)
      {
      #region check valid of grammar
      var must_have_len_eq_1 = GetWithLeft(StartSymbol);
      if(must_have_len_eq_1.Count() != 1) throw EXC_NotNormalized;
      var start_rule = must_have_len_eq_1.First();
      if(start_rule.GetRuleLen(RulePart.Left) != 1 || start_rule.GetRuleLen(RulePart.Right) != 1) throw EXC_NotNormalized;
      var back_start_s = start_rule.GetSymbol(RulePart.Right, 0);
      #endregion check...

      var I_0 = Closure(new HashSet<ClosureElem<S>>() { new ClosureElem<S>(start_rule, 0, EndSymbol, symbol_comparator) });

      #region get all symbol in grammar and sort them
      var hashmap__all_grammar_symbol = GetAllGrammarSymbol();
      hashmap__all_grammar_symbol.Remove(StartSymbol); // S' is additional not initially
      var all_grammar_symbol = hashmap__all_grammar_symbol.ToList();
      if(comparer_for_sort != null) all_grammar_symbol.Sort(comparer_for_sort);
      #endregion get all ...

      var transitions = new Dictionary<(int, int), S>() { };// <(from, to), by symbol> 
      var items = new List<HashSet<ClosureElem<S>>>() { I_0 };
      
      while(true)
        {
        int last_len = items.Count;
        var added = new List<HashSet<ClosureElem<S>>>();
        for(int i = 0; i < last_len; i++)
          {
          var I = items[i];
          foreach(var X in all_grammar_symbol)
            {
            var added_I = Goto(I, X);
            if(added_I.Count != 0)
              {
              bool add = true;

              #region oh.. no... check that added_I not contain in ret + add transition between both existing I
              for(int exist_i = 0; exist_i < last_len; exist_i++)
                {
                var exist_I = items[exist_i];
                bool is_eq = true;
                foreach(var item in exist_I)
                  {
                  if(!added_I.Contains(item)) is_eq = false;
                  if(is_eq) break;
                  }
                if(is_eq)
                  {
                  var from_to = (i, exist_i);
                  if(transitions.ContainsKey(from_to)) {if(!symbol_comparator.Equals(transitions[from_to], X)) throw new Exception("... blin");}
                  else transitions.Add(from_to, X); // from: I[i] to I[last_len + added.Count] by X-symbol
                  add = false;
                  }
                if(!add) break;
                }
              #endregion oh.. no... ...

              if(add)
                {
                transitions.Add((i, last_len + added.Count), X); // from: I[i] to I[last_len + added.Count] by X-symbol
                added.Add(added_I);
                }
              }
            }
          }

        items.AddRange(added);
        if(items.Count == last_len) break;
        }

      #region create items info
      #region create N-symbols list
      var all_N = N.ToList();
      all_N.Remove(StartSymbol); // S' is additional and not initially
      if(comparer_for_sort != null) all_N.Sort(comparer_for_sort);
      #endregion

      #region create T-symbols list
      var all_T = T.ToList();
      if(comparer_for_sort != null) all_T.Sort(comparer_for_sort);
      #endregion

      var items_info = new ItemsInfo(all_N, all_T, StartSymbol, back_start_s);
      #endregion

      return new ItemsReturn(items, transitions, items_info);
      }

    }

  }
