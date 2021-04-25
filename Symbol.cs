﻿using System;
using System.Collections.Generic;

namespace LR_1
  {
  enum SymbolType
    {
    Epsilon,
    Terminal,
    NotTerminal,
    }

  abstract class Symbol<SelfType> : ICloneable where SelfType : Symbol<SelfType>
    {
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
  }
