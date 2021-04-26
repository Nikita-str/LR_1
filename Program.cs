using System;
using System.Collections.Generic;
using System.Linq;
using LR_1.Impls;

namespace LR_1
  {
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

      Console.WriteLine("---    ---    ---    ---    ---    ---    ---    ---    ---");

      foreach(var x in G.GetWithLeft(new Chain<CharSymbol>(new List<CharSymbol>() { 'Z' }))) Console.WriteLine(x);
      Console.WriteLine("---    ---    ---    ---    ---    ---    ---    ---    ---");

      G = new Grammar<CharSymbol>(null);
      G.SetStartSymbol('S');
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'S' }, new List<CharSymbol>() { 'A', 'A' }));
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'A' }, new List<CharSymbol>() { 'a', 'A' }));
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'A' }, new List<CharSymbol>() { 'b' }));
      G.NormalizedForItems();

      Comparison<CharSymbol> items_comparision = (xc, yc) =>
        {
          var x = xc.Value;
          var y = yc.Value;
          if(x == y) return 0;
          bool x_up = char.IsUpper(x);
          bool y_up = char.IsUpper(y);
          if(x == 'S' || (x_up && !y_up) || y == '$') return -1;
          if(y == 'S' || (y_up && !x_up) || x == '$') return 1;
          return x - y;
        };
      var items = G.Items(items_comparision);
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

      //TODO: special symbols are Terminal ... 

      Console.ReadKey();
      }
    }
  }
