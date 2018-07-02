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
      createDictionary();
    }

    public ConcurrentDictionary<FilamentProfile.TypeColorKey, FilamentProfile.CustomOptions> CustomValues
    {
      get
      {
        return custom_filament_values;
      }
    }

    public void AddCustomTemperature(FilamentSpool.TypeEnum type, FilamentConstants.ColorsEnum color, int temperature)
    {
      var key = new FilamentProfile.TypeColorKey(type, color);
      FilamentProfile.CustomOptions customOptions;
      customOptions.temperature = temperature;
      if (custom_filament_values.ContainsKey(key))
      {
        custom_filament_values[key] = customOptions;
      }
      else
      {
        custom_filament_values.TryAdd(key, customOptions);
      }
    }

    public void RemoveCustomTemperature(FilamentSpool.TypeEnum type, FilamentConstants.ColorsEnum color)
    {
      var key = new FilamentProfile.TypeColorKey(type, color);
      if (!custom_filament_values.ContainsKey(key))
      {
        return;
      }

      custom_filament_values.TryRemove(key, out FilamentProfile.CustomOptions customOptions);
    }

    public List<Filament> GetFromCheatCode(string code)
    {
      if (dictionary.TryGetValue(code, out List<Filament> filamentList))
      {
        return new List<Filament>((IEnumerable<Filament>)filamentList);
      }

      return ResolveToGenericFilamentType(code);
    }

    private List<Filament> ResolveToGenericFilamentType(string toResolve)
    {
      var filamentList = (List<Filament>) null;
      if (toResolve.Length >= 3)
      {
        if (toResolve.Substring(0, 2).ToLower().Equals("tg"))
        {
          dictionary.TryGetValue("TGH", out filamentList);
        }
        else if (toResolve.Substring(0, 2).ToLower().Equals("ab"))
        {
          dictionary.TryGetValue("ARS", out filamentList);
        }
        else if (toResolve.Substring(0, 2).ToLower().Equals("pl"))
        {
          dictionary.TryGetValue("PLA", out filamentList);
        }
        else if (toResolve.Substring(0, 2).ToLower().Equals("cf"))
        {
          dictionary.TryGetValue("CFA", out filamentList);
        }
        else
        {
          if (ResolveToEncodedFilament(toResolve, out Filament filament, out var temperature))
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
      {
        return false;
      }

      var nullable1 = new FilamentSpool.TypeEnum?();
      var nullable2 = new FilamentConstants.Branding?();
      var nullable3 = new FilamentConstants.ColorsEnum?();
      var num = 0;
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
        {
          num = (int) maxMin.Min;
        }

        if ((double) num > (double) maxMin.Max)
        {
          num = (int) maxMin.Max;
        }
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
      {
        return false;
      }

      filament = new Filament(nullable1.Value, nullable3.Value, toResolve, nullable2.Value);
      temperature = num;
      return true;
    }

    public List<string> GenerateColors(FilamentSpool.TypeEnum type = FilamentSpool.TypeEnum.NoFilament)
    {
      var stringList = new List<string>();
      if (type == FilamentSpool.TypeEnum.NoFilament)
      {
        for (var index = 0; index < 20; ++index)
        {
          stringList.Add(FilamentConstants.ColorsToString((FilamentConstants.ColorsEnum) index));
        }

        stringList.Add(FilamentConstants.ColorsToString(FilamentConstants.ColorsEnum.Grey));
      }
      else
      {
        dictionary.TryGetValue(type.ToString(), out List<Filament> filamentList);
        foreach (Filament filament in filamentList.FindAll((Predicate<Filament>) (x => x.Type == type)))
        {
          stringList.Add(filament.ColorStr);
        }
      }
      return stringList;
    }

    public List<string> GenerateGenericFilimentNames()
    {
      var values = dictionary.Values as List<Filament>;
      var stringList = new List<string>();
      foreach (Filament filament in values.FindAll((Predicate<Filament>) (x => x.Color == FilamentConstants.ColorsEnum.Other)))
      {
        stringList.Add(filament.CodeStr);
      }

      return stringList;
    }

    private void createDictionary()
    {
      dictionary = new ConcurrentDictionary<string, List<Filament>>();
      foreach (Filament product in FilamentConstants.ProductList)
      {
        if (!dictionary.TryAdd(product.CodeStr, new List<Filament>()
        {
          product
        }))
        {
          dictionary.TryGetValue(product.CodeStr, out List<Filament> filamentList);
          filamentList.Add(product);
        }
      }
      custom_filament_values = new ConcurrentDictionary<FilamentProfile.TypeColorKey, FilamentProfile.CustomOptions>();
    }
  }
}
