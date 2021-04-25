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

  abstract class Symbol<SelfType> : ICloneable where SelfType : Symbol<SelfType>, new()
    {
    public static SelfType ForStaticCall = new SelfType();

    public bool isEpsilon => GetSymbolType() == SymbolType.Epsilon;
    public bool isTerminal => GetSymbolType() == SymbolType.Terminal;
    public bool isNotTerminal => GetSymbolType() == SymbolType.NotTerminal;

    public SymbolType type => this.GetSymbolType();

    public abstract SymbolType GetSymbolType();

    public static SelfType Epsilon => ForStaticCall.GenerateNew(null, SymbolType.Epsilon);
    
    /// <summary>Are you sure that you want to do that?</summary>
    public static SelfType GenerateNewTerminal(List<SelfType> already_used, Base_GenSymbolInfo gen_symb_info)
      => ForStaticCall.GenerateNew(already_used, SymbolType.Terminal, gen_symb_info);
    public static SelfType GenerateNewNotTerminal(List<SelfType> already_used, Base_GenSymbolInfo gen_symb_info)
      => ForStaticCall.GenerateNew(already_used, SymbolType.NotTerminal, gen_symb_info);
    
    /// <summary>Generate new symbol of wanted type. for Start and Epsilon just return them(cause they is uniq)
    /// <para>must be valid : (new SelfType()).GenerateNew(...)</para>
    /// in other words: must be static
    /// </summary>
    public abstract SelfType GenerateNew(IEnumerable<SelfType> already_used, SymbolType wanted_type, Base_GenSymbolInfo gen_symb_info = null);


    public static SelfType GetSpecialSymbol(int special_id) => ForStaticCall.GetSpecial(special_id);
    /// <summary>return special NotTerminal symbol. 
    /// <br/>may panic if type has limited amount of special symbols
    /// <br/>please not panic at id = 0, 1, 2
    /// <br/>also(as GenerateNew) must be valid for new().GenSpecial() i.e. must be static</summary>
    /// <param name="special_id">special symbol id</param>
    public abstract SelfType GetSpecial(int special_id);
    abstract public object Clone();
    }
  }
