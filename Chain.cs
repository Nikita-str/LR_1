using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LR_1
  {
  class Chain<S> where S: Symbol<S>, new()
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
    
    public Chain<S> SubChain(int start_index, int count = -1) => new Chain<S>(chain.GetRange(start_index, (count >= 0) ? count : chain.Count - start_index));

    public Chain(List<S> symbols, bool take_owning = true) { chain = (take_owning) ? symbols : new List<S>(symbols); }
    public Chain(Chain<S> copy) { chain = new List<S>(copy.chain); }


    public int GetHashCode(int symbol_check)
      {
      int ret = chain.Count * 187;
      for(int i = 0; i < chain.Count && i < symbol_check; i++) ret ^= (i + 1) * (i + 1) * chain[i].GetHashCode();
      return ret;
      }
    public override int GetHashCode() => GetHashCode(3);

    public override bool Equals(object obj)
      {
      if(obj is Chain<S> y)
        {
        var x = this;
        int len = x.Length;
        if(len != y.Length) return false;

        for(int i = 0; i < len; i++) if(!x[i].Equals(y[i])) return false;
        return true;
        }
      return false;
      }

    public override string ToString()
      {
      var ret = "";
      foreach(var x in chain) ret += x.ToString();
      return ret;
      }

    public static implicit operator Chain<S>(S symbol) { return new Chain<S>(new List<S>() { symbol }); }

    public static Chain<S> operator +(Chain<S> a, Chain<S> b)
      {
      var ch = new List<S>(a.chain);
      if(ch.Count == 1 && ch[0].isEpsilon) ch = new List<S>(b.chain);
      else ch.AddRange(b.chain);
      return new Chain<S>(ch);
      }

    }

  class ChainComparer<S> : IEqualityComparer<Chain<S>> where S : Symbol<S>, new()
    {
    public IEqualityComparer<S> symbol_comparator;
    public ChainComparer(IEqualityComparer<S> s_comparator)
      {
      symbol_comparator = s_comparator;
      if(symbol_comparator == null) symbol_comparator = EqualityComparer<S>.Default;
      }

    public bool Equals(Chain<S> x, Chain<S> y)
      {
      var len = x.Length;
      if(len != y.Length) return false;

      for(int i = 0; i < len; i++)if(!symbol_comparator.Equals(x[i], y[i])) return false;
      return true;
      }

    public int GetHashCode(Chain<S> obj) => obj.GetHashCode();
    }


  }
