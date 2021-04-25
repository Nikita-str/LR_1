using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LR_1
  {
  class temp
    {
    public int x { get; set; } = 0;
    public int y { get; set; } = 7;
    }

  class TempComparator : IEqualityComparer<temp>
    {
    public bool Equals(temp x, temp y){ return x.x == y.x && x.y == y.y; }

    public int GetHashCode(temp obj) { return ((obj.x * 131) << 1) ^ obj.y; }
    }

  abstract class Base_GenSymbolInfo { };
  class StdGenSymbolInfo : Base_GenSymbolInfo
    {
    public string wanted_prefix;
    public bool is_special_symbol;
    public StdGenSymbolInfo(string prefix, bool is_special)
      {
      wanted_prefix = prefix;
      is_special_symbol = is_special;
      }
    }

  enum SymbolType
    {
    Epsilon,
    Terminal,
    NotTerminal,
    }

  abstract class Symbol<SelfType> : ICloneable where SelfType: Symbol<SelfType>//, new()
    {
    //protected static SelfType ForStaticCall = new SelfType();

    public bool isEpsilon => GetSymbolType() == SymbolType.Epsilon;
    public bool isTerminal => GetSymbolType() == SymbolType.Terminal; 
    public bool isNotTerminal => GetSymbolType() == SymbolType.NotTerminal; 

    public SymbolType type => this.GetSymbolType();

    public abstract SymbolType GetSymbolType();

    public SelfType Epsilon => GenerateNew(null, SymbolType.Epsilon);

    /// <summary>Are you sure that you want to do that?</summary>
    public SelfType GenerateNewTerminal(List<SelfType> already_used, Base_GenSymbolInfo gen_symb_info) 
      => GenerateNew(already_used, SymbolType.Terminal, gen_symb_info);
    public SelfType GenerateNewNotTerminal(List<SelfType> already_used, Base_GenSymbolInfo gen_symb_info) 
      => GenerateNew(already_used, SymbolType.NotTerminal, gen_symb_info);
    
    /// <summary>Generate new symbol of wanted type. for Start and Epsilon just return them(cause they is uniq)
    /// <para>must be valid : (new SelfType()).GenerateNew(...)</para>
    /// in other words: must be static
    /// </summary>
    public abstract SelfType GenerateNew(IEnumerable<SelfType> already_used, SymbolType wanted_type, Base_GenSymbolInfo gen_symb_info = null);
    abstract public object Clone();
    }

  enum RulePart
    {
    Left,
    Right
    }

  class Rule<S> where S: Symbol<S>//, new()
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

    static private readonly List<RulePart> rule_parts = new List<RulePart>(){ RulePart.Left, RulePart.Right };
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
      foreach(var x in N){ret.Add(x.Clone() as S, First(x));}
      return ret;
      }

    public HashSet<S> First(S symbol)
      {
      var ret = new HashSet<S>();
      if(symbol.isEpsilon | symbol.isTerminal) { ret.Add(symbol.Clone() as S);  return ret; }
      
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

  class CharSymbol : Symbol<CharSymbol>
    {
    static readonly char EpsilonChar = '~';
    static readonly List<char> OtherT = new List<char>() { '+', '=', '-', '(', ')' };
    public char Value { get; protected set; }
    public CharSymbol(char c){ Value = c; }

    public override object Clone(){return new CharSymbol(Value);}

    public override CharSymbol GenerateNew(IEnumerable<CharSymbol> already_used, SymbolType wanted_type, Base_GenSymbolInfo gen_symb_info = null)
      {
      switch(wanted_type)
        {
        case SymbolType.Epsilon: return new CharSymbol(EpsilonChar);
        case SymbolType.Terminal:
          {
          var letters_amount = 'z' - 'a' + 1;
          var eng_letters = new bool[letters_amount];
          var digits_amount = '9' - '0' + 1;
          var digits = new bool[digits_amount];
          foreach(var x in already_used)
            {
            if('a' <= x.Value && x.Value <= 'z') eng_letters[x.Value - 'a'] = true;
            if('0' <= x.Value && x.Value <= '9') digits[x.Value - '0'] = true;
            }
          for(int i = 0; i < letters_amount; i++) if(!eng_letters[i]) return new CharSymbol((char)('a' + i));
          for(int i = 0; i < digits_amount; i++) if(!digits[i]) return new CharSymbol((char)('0' + i));
          throw new NotImplementedException("cant create not used symbol");
          }
        case SymbolType.NotTerminal:
          {
          if(gen_symb_info != null && gen_symb_info is StdGenSymbolInfo std && std.is_special_symbol)
            {
            CharSymbol cs = null;
            cs = new CharSymbol('#');
            if(!already_used.Contains(cs)) return cs;
            cs = new CharSymbol('$');
            if(!already_used.Contains(cs)) return cs;
            cs = new CharSymbol('.');
            if(!already_used.Contains(cs)) return cs;
            throw new NotImplementedException("cant create not used symbol");
            }
          
          var letters_amount = 'Z' - 'A' + 1;
          var eng_letters = new bool[letters_amount];
          foreach(var x in already_used)
            {
            if('a' <= x.Value && x.Value <= 'z') eng_letters[x.Value - 'a'] = true;
            }
          for(int i = 0; i < letters_amount; i++) if(!eng_letters[i]) return new CharSymbol((char)('a' + i));
          throw new NotImplementedException("cant create not used symbol");
          }
        default: throw new NotImplementedException("wanted_type is not Eps, N, T ?!");
        }

      }

    public override SymbolType GetSymbolType()
      {
      if(Value == EpsilonChar) return SymbolType.Epsilon;
      if(char.IsDigit(Value) || char.IsLower(Value) || OtherT.Contains(Value)) return SymbolType.Terminal;
      return SymbolType.NotTerminal;
      }

    public override bool Equals(object obj)
      {
      if(obj is CharSymbol o) return o.Value == Value;
      return false;
      }

    public override int GetHashCode() => Value;

    public static implicit operator CharSymbol(char c) { return new CharSymbol(c); }

    public override string ToString() => "" + Value;
    }

  class Program
    {
    static void Main(string[] args)
      {

      //TODO: add абстракция цепочки 

      var G = new Grammar<CharSymbol>(null);
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'S' }, new List<CharSymbol>() { 'a', 'Z' }));
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'Z' }, new List<CharSymbol>() { 'A', 'b', 'C' }));
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'Z' }, new List<CharSymbol>() { 'b', 'C' }));
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'Z' }, new List<CharSymbol>() { '~' }));
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'C' }, new List<CharSymbol>() { 'B', 'Z' }));
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'C' }, new List<CharSymbol>() { 'Z' }));
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'A' }, new List<CharSymbol>() { 'a', 'X' }));
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'X' }, new List<CharSymbol>() { 'a' }));
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'X' }, new List<CharSymbol>() { 'b' }));
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'B' }, new List<CharSymbol>() { 'c' }));

      foreach(var x in G.FirstAllNotTerminal())
        {
        Console.Write(x.Key);
        Console.Write(" : ");
        foreach(var cs in x.Value) Console.Write(cs + "  ");
        Console.WriteLine();
        }

      /*
      var b = new HashSet<CharSymbol>();
      b.Add(new CharSymbol('+'));
      b.Add(new CharSymbol('+'));
      b.Add(new CharSymbol('+'));
      Console.WriteLine(b.Count);

      List<int> a = new List<int>() { 1, 2, 4 };
      List<int> z = new List<int>() { 7, 2, 4 };
      List<int> c = new List<int>() { 1, 2, 4 };
      Console.WriteLine(a.GetHashCode() == z.GetHashCode());
      Console.WriteLine(a.GetHashCode() == c.GetHashCode());
      */

      Console.ReadKey();
      }
    }
  }
