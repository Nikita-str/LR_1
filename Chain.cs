using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LR_1
  {
  class Chain<S> where S: Symbol<S>
    {
    List<S> chain;
    /// <summary>only for read</summary>
    public List<S> GetChainSymbols() => chain;

    public int Length => chain.Count;

    public S this[int index]
      {
      get
        {
        if(index < 0 || chain.Count <= index) throw new IndexOutOfRangeException("index must be in [0; GetRuleLen(rule_part))");
        return chain[index].Clone() as S;
        }
      }

    /// <summary>Rarely used</summary>
    /// <param name="ind">index before adding symbol</param>
    public void AddSymbol(int ind, S symbol)
      {
      if(ind < 0 || chain.Count < ind) throw new IndexOutOfRangeException("index must be in [0; GetRuleLen(rule_part)]");
      if(ind == chain.Count)
        {
        if(ind == 1 && chain[0].isEpsilon) chain[0] = symbol;
        else chain.Add(symbol);
        }
      else chain.Insert(ind, symbol);
      }

    public void DelSymbol(int ind)
      {
      if(ind < 0 || chain.Count <= ind) throw new IndexOutOfRangeException("index must be in [0; GetRuleLen(rule_part))");
      if(ind == 0 && chain.Count == 1 && chain[0].isEpsilon) return;
      else chain.RemoveAt(ind);
      }

    public Chain(List<S> symbols, bool take_owning = true) { chain = (take_owning) ? symbols : new List<S>(symbols); }
    public Chain(Chain<S> copy) { chain = new List<S>(copy.chain); }


    public int GetHashCode(int symbol_check)
      {
      int ret = chain.Count * 187;
      for(int i = 0; i < chain.Count && i < symbol_check; i++) ret ^= (i + 1) * (i + 1) * chain[i].GetHashCode();
      return ret;
      }
    public override int GetHashCode() => GetHashCode(3);
    }
  }
