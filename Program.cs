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

      Console.ReadKey();
      }
    }
  }
