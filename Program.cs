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
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'Z' }, new List<CharSymbol>() { 'S' }));
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'S' }, new List<CharSymbol>() { 'A', 'A' }));
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'A' }, new List<CharSymbol>() { 'a', 'A' }));
      G.AddRule(new Rule<CharSymbol>(new List<CharSymbol>() { 'A' }, new List<CharSymbol>() { 'b' }));

      var first_closure = new ClosureElem<CharSymbol>(G.GetWithLeft((CharSymbol)'Z').First(), 0, G.EndSymbol, G.symbol_comparator);
      Console.WriteLine("closure for: " + first_closure);
      var res_closure = G.Closure(new HashSet<ClosureElem<CharSymbol>>() { first_closure });
      Console.WriteLine("is: ");
      foreach(var x in res_closure) Console.WriteLine(x);

      Console.ReadKey();
      }
    }
  }
