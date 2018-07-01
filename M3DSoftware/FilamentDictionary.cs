// Decompiled with JetBrains decompiler
// Type: M3D.FilamentDictionary
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Spooling.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace M3D
{
  public class FilamentDictionary
  {
    private ConcurrentDictionary<string, List<Filament>> dictionary;
    private ConcurrentDictionary<FilamentProfile.TypeColorKey, FilamentProfile.CustomOptions> custom_filament_values;

    public FilamentDictionary()
    {
      this.createDictionary();
    }

    public ConcurrentDictionary<FilamentProfile.TypeColorKey, FilamentProfile.CustomOptions> CustomValues
    {
      get
      {
        return this.custom_filament_values;
      }
    }

    public void AddCustomTemperature(FilamentSpool.TypeEnum type, FilamentConstants.ColorsEnum color, int temperature)
    {
      FilamentProfile.TypeColorKey key = new FilamentProfile.TypeColorKey(type, color);
      FilamentProfile.CustomOptions customOptions;
      customOptions.temperature = temperature;
      if (this.custom_filament_values.ContainsKey(key))
        this.custom_filament_values[key] = customOptions;
      else
        this.custom_filament_values.TryAdd(key, customOptions);
    }

    public void RemoveCustomTemperature(FilamentSpool.TypeEnum type, FilamentConstants.ColorsEnum color)
    {
      FilamentProfile.TypeColorKey key = new FilamentProfile.TypeColorKey(type, color);
      if (!this.custom_filament_values.ContainsKey(key))
        return;
      FilamentProfile.CustomOptions customOptions;
      this.custom_filament_values.TryRemove(key, out customOptions);
    }

    public List<Filament> GetFromCheatCode(string code)
    {
      List<Filament> filamentList;
      if (this.dictionary.TryGetValue(code, out filamentList))
        return new List<Filament>((IEnumerable<Filament>) filamentList);
      return this.ResolveToGenericFilamentType(code);
    }

    private List<Filament> ResolveToGenericFilamentType(string toResolve)
    {
      List<Filament> filamentList = (List<Filament>) null;
      if (toResolve.Length >= 3)
      {
        if (toResolve.Substring(0, 2).ToLower().Equals("tg"))
          this.dictionary.TryGetValue("TGH", out filamentList);
        else if (toResolve.Substring(0, 2).ToLower().Equals("ab"))
          this.dictionary.TryGetValue("ARS", out filamentList);
        else if (toResolve.Substring(0, 2).ToLower().Equals("pl"))
          this.dictionary.TryGetValue("PLA", out filamentList);
        else if (toResolve.Substring(0, 2).ToLower().Equals("cf"))
        {
          this.dictionary.TryGetValue("CFA", out filamentList);
        }
        else
        {
          Filament filament;
          int temperature;
          if (this.ResolveToEncodedFilament(toResolve, out filament, out temperature))
          {
            filamentList = new List<Filament>();
            filamentList.Add(filament);
          }
        }
      }
      return filamentList;
    }

    public bool ResolveToEncodedFilament(string toResolve, out Filament filament, out int temperature)
    {
      filament = (Filament) null;
      temperature = -1;
      if (toResolve.Count<char>() != 3)
        return false;
      FilamentSpool.TypeEnum? nullable1 = new FilamentSpool.TypeEnum?();
      FilamentConstants.Branding? nullable2 = new FilamentConstants.Branding?();
      FilamentConstants.ColorsEnum? nullable3 = new FilamentConstants.ColorsEnum?();
      int num = 0;
      switch (char.ToLower(toResolve[0]))
      {
        case 'e':
          nullable1 = new FilamentSpool.TypeEnum?(FilamentSpool.TypeEnum.PLA);
          nullable2 = new FilamentConstants.Branding?(FilamentConstants.Branding.SolidInk);
          break;
        case 'k':
          nullable1 = new FilamentSpool.TypeEnum?(FilamentSpool.TypeEnum.ABS_R);
          nullable2 = new FilamentConstants.Branding?(FilamentConstants.Branding.Ink);
          break;
        case 'q':
          nullable1 = new FilamentSpool.TypeEnum?(FilamentSpool.TypeEnum.ABS);
          nullable2 = new FilamentConstants.Branding?(FilamentConstants.Branding.ExpertInk);
          break;
        case 'v':
          nullable1 = new FilamentSpool.TypeEnum?(FilamentSpool.TypeEnum.TGH);
          nullable2 = new FilamentConstants.Branding?(FilamentConstants.Branding.ToughInk);
          break;
        case 'x':
          nullable1 = new FilamentSpool.TypeEnum?(FilamentSpool.TypeEnum.PLA);
          nullable2 = new FilamentConstants.Branding?(FilamentConstants.Branding.Ink);
          break;
      }
      if (char.IsLetter(toResolve[1]) && nullable1.HasValue)
      {
        num = ((int) char.ToLower(toResolve[1]) - 97) * 5 + 170;
        FilamentConstants.Temperature.MaxMin maxMin = FilamentConstants.Temperature.MaxMinForFilamentType(nullable1.Value);
        if ((double) num < (double) maxMin.Min)
          num = (int) maxMin.Min;
        if ((double) num > (double) maxMin.Max)
          num = (int) maxMin.Max;
      }
      if (char.IsLetter(toResolve[2]))
      {
        switch (char.ToLower(toResolve[2]))
        {
          case 's':
          case 't':
          case 'u':
          case 'v':
            nullable3 = new FilamentConstants.ColorsEnum?(FilamentConstants.ColorsEnum.Other);
            break;
        }
      }
      if (!nullable1.HasValue || !nullable2.HasValue || (!nullable3.HasValue || num == 0))
        return false;
      filament = new Filament(nullable1.Value, nullable3.Value, toResolve, nullable2.Value);
      temperature = num;
      return true;
    }

    public List<string> GenerateColors(FilamentSpool.TypeEnum type = FilamentSpool.TypeEnum.NoFilament)
    {
      List<string> stringList = new List<string>();
      if (type == FilamentSpool.TypeEnum.NoFilament)
      {
        for (int index = 0; index < 20; ++index)
          stringList.Add(FilamentConstants.ColorsToString((FilamentConstants.ColorsEnum) index));
        stringList.Add(FilamentConstants.ColorsToString(FilamentConstants.ColorsEnum.Grey));
      }
      else
      {
        List<Filament> filamentList;
        this.dictionary.TryGetValue(type.ToString(), out filamentList);
        foreach (Filament filament in filamentList.FindAll((Predicate<Filament>) (x => x.Type == type)))
          stringList.Add(filament.ColorStr);
      }
      return stringList;
    }

    public List<string> GenerateGenericFilimentNames()
    {
      List<Filament> values = this.dictionary.Values as List<Filament>;
      List<string> stringList = new List<string>();
      foreach (Filament filament in values.FindAll((Predicate<Filament>) (x => x.Color == FilamentConstants.ColorsEnum.Other)))
        stringList.Add(filament.CodeStr);
      return stringList;
    }

    private void createDictionary()
    {
      this.dictionary = new ConcurrentDictionary<string, List<Filament>>();
      foreach (Filament product in FilamentConstants.ProductList)
      {
        if (!this.dictionary.TryAdd(product.CodeStr, new List<Filament>()
        {
          product
        }))
        {
          List<Filament> filamentList;
          this.dictionary.TryGetValue(product.CodeStr, out filamentList);
          filamentList.Add(product);
        }
      }
      this.custom_filament_values = new ConcurrentDictionary<FilamentProfile.TypeColorKey, FilamentProfile.CustomOptions>();
    }
  }
}
