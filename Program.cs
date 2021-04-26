using System;
using System.Collections.Generic;
using System.Linq;
using LR_1.Impls;

namespace LR_1
  {

  class Program
    {

    static void Print__LR_1<S>(Grammar<S> G, Comparison<S> items_comparison = null) where S: Symbol<S>, new()
      {
      var items = G.Items(items_comparison);
      Console.WriteLine("Transitions:");
      items.PrintTransitions();
      Console.WriteLine("\nItems:");
      items.PrintItems();
      Console.WriteLine();
      items.PrintGotoTable();
      Console.WriteLine();
      items.PrintActionTable();

      Console.WriteLine("\n Rules:");
      G.PrintGramar(true);

      Console.ReadKey();
      }


    static List<Rule<StringSymbol>> GetRyleSS(string s)
      {
      int arrow_ind = s.IndexOf("->");
      if(arrow_ind == -1 || s.IndexOf("->", arrow_ind + 1) != -1) throw new ArgumentException("s must contain only one ->");
      var left = s.Substring(0, arrow_ind).Trim();
      var right = s.Substring(arrow_ind + 2).Trim();
      var rs = right.Split('|');

      var ret = new List<Rule<StringSymbol>>();
      var left_l = new List<StringSymbol>(left.Split(' ').Select(z => (StringSymbol)z));
      foreach(var xx in rs)
        {
        var x = xx.Trim();
        var right_l = new List<StringSymbol>(x.Split(' ').Select(z => (StringSymbol)z));
        ret.Add(new Rule<StringSymbol>(left_l, right_l));
        }

      return ret;
      }

    static List<Rule<CharSymbol>> GetRyleCS(string s)
      {
      int arrow_ind = s.IndexOf("->");
      if(arrow_ind == -1 || s.IndexOf("->", arrow_ind + 1) != -1) throw new ArgumentException("s must contain only one ->");
      var left = s.Substring(0, arrow_ind).Trim();
      var right = s.Substring(arrow_ind + 2).Trim();
      var rs = right.Split('|');

      var ret = new List<Rule<CharSymbol>>();
      var left_l = new List<CharSymbol>(left.ToList().Select(z => (CharSymbol)z));
      foreach(var xx in rs)
        {
        var x = xx.Trim();
        var right_l = new List<CharSymbol>(x.ToList().Select(z => (CharSymbol)z));
        ret.Add(new Rule<CharSymbol>(left_l, right_l));
        }

      return ret;
      }

    static int CS_Comparison(CharSymbol xc, CharSymbol yc)
      {
      var x = xc.Value;
      var y = yc.Value;
      if(x == y) return 0;
      bool x_up = char.IsUpper(x);
      bool y_up = char.IsUpper(y);
      if(x == 'S' || (x_up && !y_up) || y == '$') return -1;
      if(y == 'S' || (y_up && !x_up) || x == '$') return 1;
      return x - y;
      }

    static int SS_Comparison(StringSymbol xc, StringSymbol yc)
      {
      if(xc.s == yc.s) return 0;
      var x = xc.s[0];
      var y = yc.s[0];
      bool x_up = char.IsUpper(x);
      bool y_up = char.IsUpper(y);
      if(x == 'S' || (x_up && !y_up) || y == '$') return -1;
      if(y == 'S' || (y_up && !x_up) || x == '$') return 1;
      return String.Compare(xc.s, yc.s); 
      }

    static void Test(bool use_ss)
      {
      if(use_ss)
        {
        var G = new Grammar<StringSymbol>(null);
        G.SetStartSymbol('S');
        G.AddRules(GetRyleSS("S -> A A"));
        G.AddRules(GetRyleSS("A -> a A | b"));
        G.NormalizedForItems();
        Print__LR_1(G, SS_Comparison); 
        }
      else
        {
        var G = new Grammar<CharSymbol>(null);
        G.SetStartSymbol('S');
        G.AddRules(GetRyleCS("S -> AA"));
        G.AddRules(GetRyleCS("A -> aA|b"));
        G.NormalizedForItems();
        Print__LR_1(G, CS_Comparison);
        }
      }

    static void Test_2()
      {
      var G = new Grammar<StringSymbol>(null);
      G.SetStartSymbol('E');
      G.AddRules(GetRyleSS("E -> E plus T | T"));
      G.AddRules(GetRyleSS("T -> T mul F | F"));
      G.AddRules(GetRyleSS("F ->id"));
      G.NormalizedForItems();
      Print__LR_1(G, SS_Comparison);
      }

    static void Main(string[] args)
      {
      //choose your fighter
      Test(false);
      //Test(true);
      //Test_2();
      Console.ReadKey();
      }
    }
  }
