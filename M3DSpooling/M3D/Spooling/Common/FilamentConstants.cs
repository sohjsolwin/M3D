using M3D.Spooling.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace M3D.Spooling.Common
{
  public class FilamentConstants
  {
    public static string TypesToString(FilamentSpool.TypeEnum type)
    {
      return type.ToString().Replace('_', '-');
    }

    public static FilamentSpool.TypeEnum StringToType(string typeStr)
    {
      typeStr = typeStr.ToUpperInvariant();
      if (typeStr == "ABSR")
      {
        typeStr = "ABS_R";
      }

      if (typeStr == "ABS-R")
      {
        typeStr = "ABS_R";
      }

      if (typeStr == "FLX")
      {
        return FilamentSpool.TypeEnum.TGH;
      }

      if (typeStr == "CAM")
      {
        return FilamentSpool.TypeEnum.PLA;
      }

      return (FilamentSpool.TypeEnum) Enum.Parse(typeof (FilamentSpool.TypeEnum), typeStr);
    }

    public static List<string> GetTypes()
    {
      var stringList = new List<string>();
      foreach (var obj in Enum.GetValues(typeof (FilamentSpool.TypeEnum)))
      {
        stringList.Add(obj.ToString());
      }

      return stringList;
    }

    public static string BrandingToString(FilamentConstants.Branding brand)
    {
      switch (brand)
      {
        case FilamentConstants.Branding.Ink:
          return "3D Ink™";
        case FilamentConstants.Branding.ExpertInk:
          return "Expert 3D Ink™";
        case FilamentConstants.Branding.ToughInk:
          return "Tough 3D Ink™";
        case FilamentConstants.Branding.SolidInk:
          return "Solid 3D Ink™";
        case FilamentConstants.Branding.TransparentInk:
          return "Transparent 3D Ink™";
        case FilamentConstants.Branding.TriChromeInk:
          return "TriChrome 3D Ink™";
        default:
          if (Debugger.IsAttached && brand != FilamentConstants.Branding.Other)
          {
            Debugger.Break();
          }

          return "";
      }
    }

    public static FilamentConstants.Branding GetBrandingFrom(FilamentSpool.TypeEnum type, FilamentConstants.ColorsEnum color)
    {
      Filament filament = FilamentConstants.ProductList.Find((Predicate<Filament>) (f =>
      {
        if (f.Color == color)
        {
          return f.Type == type;
        }

        return false;
      }));
      if (filament == null)
      {
        filament = FilamentConstants.ProductList.Find((Predicate<Filament>) (f =>
        {
          if (f.Color == FilamentConstants.ColorsEnum.Other)
          {
            return f.Type == type;
          }

          return false;
        }));
        if (filament == null)
        {
          return FilamentConstants.Branding.Other;
        }
      }
      return filament.Brand;
    }

    public static string ColorsToString(FilamentConstants.ColorsEnum color)
    {
      var str = color.ToString();
      for (var startIndex = 1; startIndex < str.Length; ++startIndex)
      {
        if (char.IsUpper(str[startIndex]))
        {
          str = str.Insert(startIndex, " ");
          ++startIndex;
        }
      }
      return str;
    }

    public static FilamentConstants.ColorsEnum StringToFilamentColors(string color)
    {
      color = color.Replace(" ", "");
      try
      {
        return (FilamentConstants.ColorsEnum) Enum.Parse(typeof (FilamentConstants.ColorsEnum), color);
      }
      catch (ArgumentException ex)
      {
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in FilamentProfile.StringToFilamentColors " + ex.Message, "Exception");
      }
      return FilamentConstants.ColorsEnum.Other;
    }

    public static uint generateHEXFromColor(FilamentConstants.ColorsEnum color)
    {
      uint num = 538976511;
      switch (color)
      {
        case FilamentConstants.ColorsEnum.Blue:
          num = 10092543U;
          break;
        case FilamentConstants.ColorsEnum.Brown:
          num = 2336560127U;
          break;
        case FilamentConstants.ColorsEnum.Gold:
          num = 4292280575U;
          break;
        case FilamentConstants.ColorsEnum.Pink:
          num = 4279538687U;
          break;
        case FilamentConstants.ColorsEnum.Green:
          num = 8388863U;
          break;
        case FilamentConstants.ColorsEnum.LightBlue:
          num = 12582911U;
          break;
        case FilamentConstants.ColorsEnum.LightGreen:
          num = 2867986687U;
          break;
        case FilamentConstants.ColorsEnum.Magenta:
          num = 4278255615U;
          break;
        case FilamentConstants.ColorsEnum.Natural:
        case FilamentConstants.ColorsEnum.Clear:
          num = 4210752204U;
          break;
        case FilamentConstants.ColorsEnum.NeonBlue:
          num = (uint) ushort.MaxValue;
          break;
        case FilamentConstants.ColorsEnum.NeonOrange:
          num = 4288610559U;
          break;
        case FilamentConstants.ColorsEnum.NeonYellow:
          num = 4294902015U;
          break;
        case FilamentConstants.ColorsEnum.Orange:
          num = 4289003775U;
          break;
        case FilamentConstants.ColorsEnum.Purple:
        case FilamentConstants.ColorsEnum.MulberryPurple:
          num = 2420546047U;
          break;
        case FilamentConstants.ColorsEnum.Red:
          num = 3591252735U;
          break;
        case FilamentConstants.ColorsEnum.Silver:
          num = 2526451455U;
          break;
        case FilamentConstants.ColorsEnum.White:
        case FilamentConstants.ColorsEnum.PearlWhite:
          num = uint.MaxValue;
          break;
        case FilamentConstants.ColorsEnum.Yellow:
          num = 4294902015U;
          break;
        case FilamentConstants.ColorsEnum.HoneyClear:
          num = 4261410815U;
          break;
        case FilamentConstants.ColorsEnum.FuchsiaRed:
          num = 3425541119U;
          break;
        case FilamentConstants.ColorsEnum.CrimsonRed:
          num = 3591252735U;
          break;
        case FilamentConstants.ColorsEnum.FireOrange:
          num = 4284875007U;
          break;
        case FilamentConstants.ColorsEnum.MangoYellow:
          num = 4288217343U;
          break;
        case FilamentConstants.ColorsEnum.ShamrockGreen:
          num = 1724671743U;
          break;
        case FilamentConstants.ColorsEnum.CobaltBlue:
          num = 6737151U;
          break;
        case FilamentConstants.ColorsEnum.CaribbeanBlue:
          num = 8833279U;
          break;
        case FilamentConstants.ColorsEnum.TitaniumSilver:
          num = 2442236415U;
          break;
        case FilamentConstants.ColorsEnum.CharcoalBlack:
          num = 791621631U;
          break;
        case FilamentConstants.ColorsEnum.WhitePearl:
          num = 4109625855U;
          break;
        case FilamentConstants.ColorsEnum.CrystalClear:
          num = 4092851199U;
          break;
        case FilamentConstants.ColorsEnum.LightFuchsia:
          num = 4279613439U;
          break;
        case FilamentConstants.ColorsEnum.DeepCrimson:
          num = 3456106751U;
          break;
        case FilamentConstants.ColorsEnum.DeepShamrock:
          num = 326911743U;
          break;
        case FilamentConstants.ColorsEnum.DeepColbalt:
          num = 2415359U;
          break;
        case FilamentConstants.ColorsEnum.LightCarribean:
          num = 8833279U;
          break;
        case FilamentConstants.ColorsEnum.DeepMulberry:
          num = 2550188799U;
          break;
        case FilamentConstants.ColorsEnum.SatelliteSilver:
        case FilamentConstants.ColorsEnum.CoolGray:
          num = 2358422783U;
          break;
        case FilamentConstants.ColorsEnum.DeepLemon:
          num = 4226289663U;
          break;
        case FilamentConstants.ColorsEnum.SunsetOrange:
          num = 4288813055U;
          break;
        case FilamentConstants.ColorsEnum.OnyxBlack:
        case FilamentConstants.ColorsEnum.BlackCarbonFiber:
          num = 791621631U;
          break;
        case FilamentConstants.ColorsEnum.DragonRedTouch:
        case FilamentConstants.ColorsEnum.DragonRedHot:
          num = 3858759935U;
          break;
        case FilamentConstants.ColorsEnum.RoseRedTouch:
          num = 4279858687U;
          break;
        case FilamentConstants.ColorsEnum.CoralOrangeTouch:
        case FilamentConstants.ColorsEnum.CoralOrangeWarm:
          num = 4282849791U;
          break;
        case FilamentConstants.ColorsEnum.DespicableYellowTouch:
          num = 4294902015U;
          break;
        case FilamentConstants.ColorsEnum.MonsterGreenTouch:
          num = 1558600959U;
          break;
        case FilamentConstants.ColorsEnum.GenieBlueTouch:
        case FilamentConstants.ColorsEnum.GenieBlueIce:
          num = 869072895U;
          break;
        case FilamentConstants.ColorsEnum.GargoyleBlackTouch:
          num = 741092607U;
          break;
        case FilamentConstants.ColorsEnum.TrichromeNebula:
          num = 2420546047U;
          break;
        case FilamentConstants.ColorsEnum.TrichromeTiger:
          num = 2337751807U;
          break;
        case FilamentConstants.ColorsEnum.Grey:
          num = 2526451455U;
          break;
        case FilamentConstants.ColorsEnum.MilkWhite:
          num = 4210752255U;
          break;
        case FilamentConstants.ColorsEnum.DurableClear:
          num = uint.MaxValue;
          break;
        case FilamentConstants.ColorsEnum.SuperBlue:
          num = 8833279U;
          break;
        case FilamentConstants.ColorsEnum.MightyBlue:
          num = 869072895U;
          break;
        case FilamentConstants.ColorsEnum.YogaGreen:
          num = 92407807U;
          break;
        case FilamentConstants.ColorsEnum.RockyGreen:
          num = 326911743U;
          break;
        case FilamentConstants.ColorsEnum.OrangeHeft:
          num = 4286775551U;
          break;
        case FilamentConstants.ColorsEnum.FlexibleRuby:
          num = 3591252735U;
          break;
        case FilamentConstants.ColorsEnum.StrongPurple:
          num = 2420546047U;
          break;
        case FilamentConstants.ColorsEnum.RavenPurple:
          num = 2550188799U;
          break;
        case FilamentConstants.ColorsEnum.UtilityGrey:
          num = 2442236415U;
          break;
        case FilamentConstants.ColorsEnum.ImpactBlack:
          num = (uint) byte.MaxValue;
          break;
        case FilamentConstants.ColorsEnum.ParakeetTouch:
          num = 1558600959U;
          break;
        case FilamentConstants.ColorsEnum.PeacockTouch:
          num = 2550188799U;
          break;
        case FilamentConstants.ColorsEnum.EclectusTouch:
          num = 2420546047U;
          break;
        case FilamentConstants.ColorsEnum.CandyGreen:
          num = 3170877951U;
          break;
        case FilamentConstants.ColorsEnum.DeepCobalt:
          num = 3368703U;
          break;
        case FilamentConstants.ColorsEnum.BrickRed:
          num = 2216634367U;
          break;
        case FilamentConstants.ColorsEnum.FireTruck:
          num = 3458214399U;
          break;
        case FilamentConstants.ColorsEnum.PumpkinOrange:
          num = 4285864191U;
          break;
        case FilamentConstants.ColorsEnum.BerryPurple:
          num = 1543845119U;
          break;
        case FilamentConstants.ColorsEnum.BlueMoon:
          num = 3233870079U;
          break;
        case FilamentConstants.ColorsEnum.AntiqueGold:
          num = 3467395583U;
          break;
        case FilamentConstants.ColorsEnum.SnakeCopper:
          num = 3094557695U;
          break;
        case FilamentConstants.ColorsEnum.AppleGreen:
          num = 1539454719U;
          break;
        case FilamentConstants.ColorsEnum.SkyBlue:
          num = 2278484991U;
          break;
        case FilamentConstants.ColorsEnum.OrientRed:
          num = 2651918079U;
          break;
      }
      return num;
    }

    public static void HexToRGB(uint hexColor, out float R, out float G, out float B, out float A)
    {
      R = (float) (((long) hexColor & -16777216L) >> 24) / (float) byte.MaxValue;
      G = (float) ((hexColor & 16711680U) >> 16) / (float) byte.MaxValue;
      B = (float) ((hexColor & 65280U) >> 8) / (float) byte.MaxValue;
      A = (float) (hexColor & (uint) byte.MaxValue) / (float) byte.MaxValue;
    }

    public static List<Filament> ProductList
    {
      get
      {
        return new List<Filament>() { FilamentConstants.CFDT(FilamentSpool.TypeEnum.HIPS, FilamentConstants.ColorsEnum.White, "YAW", FilamentConstants.Branding.ExpertInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS, FilamentConstants.ColorsEnum.HoneyClear, "GET", FilamentConstants.Branding.ExpertInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS, FilamentConstants.ColorsEnum.FuchsiaRed, "YAY", FilamentConstants.Branding.ExpertInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS, FilamentConstants.ColorsEnum.CrimsonRed, "FLY", FilamentConstants.Branding.ExpertInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS, FilamentConstants.ColorsEnum.FireOrange, "HOT", FilamentConstants.Branding.ExpertInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS, FilamentConstants.ColorsEnum.MangoYellow, "APE", FilamentConstants.Branding.ExpertInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS, FilamentConstants.ColorsEnum.ShamrockGreen, "BOA", FilamentConstants.Branding.ExpertInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS, FilamentConstants.ColorsEnum.CobaltBlue, "BAM", FilamentConstants.Branding.ExpertInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS, FilamentConstants.ColorsEnum.CaribbeanBlue, "SEA", FilamentConstants.Branding.ExpertInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS, FilamentConstants.ColorsEnum.MulberryPurple, "MOO", FilamentConstants.Branding.ExpertInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS, FilamentConstants.ColorsEnum.TitaniumSilver, "FUN", FilamentConstants.Branding.ExpertInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS, FilamentConstants.ColorsEnum.CharcoalBlack, "COY", FilamentConstants.Branding.ExpertInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.WhitePearl, "ICE", FilamentConstants.Branding.SolidInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.CrystalClear, "WOW", FilamentConstants.Branding.TransparentInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.LightFuchsia, "LUV", FilamentConstants.Branding.TransparentInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.DeepCrimson, "FOX", FilamentConstants.Branding.TransparentInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.DeepShamrock, "PUB", FilamentConstants.Branding.TransparentInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.DeepColbalt, "COW", FilamentConstants.Branding.TransparentInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.LightCarribean, "SKY", FilamentConstants.Branding.TransparentInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.DeepMulberry, "PIE", FilamentConstants.Branding.TransparentInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.SatelliteSilver, "ART", FilamentConstants.Branding.SolidInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.DeepLemon, "WEE", FilamentConstants.Branding.TransparentInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.SunsetOrange, "POP", FilamentConstants.Branding.TransparentInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.OnyxBlack, "WAY", FilamentConstants.Branding.SolidInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.CandyGreen, "CAN", FilamentConstants.Branding.TransparentInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.BrickRed, "ICK", FilamentConstants.Branding.TransparentInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.CoolGray, "GRY", FilamentConstants.Branding.TransparentInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.DragonRedTouch, "DRG", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.DragonRedTouch, "CAM", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.DragonRedHot, "DRA", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.DragonRedHot, "CAM", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.RoseRedTouch, "ROS", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.RoseRedTouch, "CAM", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.CoralOrangeTouch, "COR", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.CoralOrangeTouch, "CAM", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.CoralOrangeWarm, "CRL", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.CoralOrangeWarm, "CAM", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.DespicableYellowTouch, "DSP", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.DespicableYellowTouch, "CAM", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.MonsterGreenTouch, "MON", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.MonsterGreenTouch, "CAM", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.GenieBlueTouch, "GEN", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.GenieBlueTouch, "CAM", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.GenieBlueIce, "GNY", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.GenieBlueIce, "CAM", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.GargoyleBlackTouch, "GRG", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.GargoyleBlackTouch, "CAM", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.TrichromeNebula, "NEB", FilamentConstants.Branding.TriChromeInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.TrichromeNebula, "CAM", FilamentConstants.Branding.TriChromeInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.TrichromeTiger, "TIG", FilamentConstants.Branding.TriChromeInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.TrichromeTiger, "CAM", FilamentConstants.Branding.TriChromeInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.ParakeetTouch, "CAM", FilamentConstants.Branding.TriChromeInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.PeacockTouch, "CAM", FilamentConstants.Branding.TriChromeInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.CAM, FilamentConstants.ColorsEnum.EclectusTouch, "CAM", FilamentConstants.Branding.TriChromeInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS_R, FilamentConstants.ColorsEnum.Black, "ABK", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS_R, FilamentConstants.ColorsEnum.Blue, "ABB", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS_R, FilamentConstants.ColorsEnum.Clear, "ABC", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS_R, FilamentConstants.ColorsEnum.Red, "ABRR", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS_R, FilamentConstants.ColorsEnum.Grey, "ABY", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS_R, FilamentConstants.ColorsEnum.Silver, "ABV", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS_R, FilamentConstants.ColorsEnum.White, "ABW", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.FireTruck, "HUG", FilamentConstants.Branding.SolidInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.PumpkinOrange, "YUM", FilamentConstants.Branding.SolidInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.BerryPurple, "YUM", FilamentConstants.Branding.SolidInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.BlueMoon, "HUG", FilamentConstants.Branding.SolidInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.AntiqueGold, "GLD", FilamentConstants.Branding.SolidInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.SnakeCopper, "COP", FilamentConstants.Branding.SolidInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.AppleGreen, "YUM", FilamentConstants.Branding.SolidInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.SkyBlue, "YUM", FilamentConstants.Branding.SolidInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.OrientRed, "HUG", FilamentConstants.Branding.SolidInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.TGH, FilamentConstants.ColorsEnum.ImpactBlack, "TGB", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.TGH, FilamentConstants.ColorsEnum.MilkWhite, "TGB", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.TGH, FilamentConstants.ColorsEnum.YogaGreen, "TGB", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.TGH, FilamentConstants.ColorsEnum.StrongPurple, "TGB", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.TGH, FilamentConstants.ColorsEnum.UtilityGrey, "TGB", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.TGH, FilamentConstants.ColorsEnum.MightyBlue, "TGB", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.TGH, FilamentConstants.ColorsEnum.FlexibleRuby, "TGB", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.TGH, FilamentConstants.ColorsEnum.SuperBlue, "TGB", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.TGH, FilamentConstants.ColorsEnum.DurableClear, "TGB", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.TGH, FilamentConstants.ColorsEnum.RockyGreen, "TGB", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.TGH, FilamentConstants.ColorsEnum.RavenPurple, "TGB", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.TGH, FilamentConstants.ColorsEnum.OrangeHeft, "TGB", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.BlackCarbonFiber, "CFA", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.BlackCarbonFiber, "CFB", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.BlackCarbonFiber, "CFC", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.BlackCarbonFiber, "CFD", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.FLX, FilamentConstants.ColorsEnum.Other, "FLX", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.TGH, FilamentConstants.ColorsEnum.Other, "TGA", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.TGH, FilamentConstants.ColorsEnum.Other, "TGB", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.TGH, FilamentConstants.ColorsEnum.Other, "TGH", FilamentConstants.Branding.ToughInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.Other, "PLA", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.PLA, FilamentConstants.ColorsEnum.Other, "CAM", FilamentConstants.Branding.Ink), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS, FilamentConstants.ColorsEnum.Other, "ABS", FilamentConstants.Branding.ExpertInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS, FilamentConstants.ColorsEnum.Other, "HIP", FilamentConstants.Branding.ExpertInk), FilamentConstants.CFDT(FilamentSpool.TypeEnum.ABS_R, FilamentConstants.ColorsEnum.Other, "ABR", FilamentConstants.Branding.Ink) };
      }
    }

    private static Filament CFDT(FilamentSpool.TypeEnum type, FilamentConstants.ColorsEnum color, string code, FilamentConstants.Branding brand)
    {
      return new Filament(type, color, code, brand);
    }

    public static class Temperature
    {
      public static FilamentConstants.Temperature.MaxMin MaxMinForFilamentType(FilamentSpool.TypeEnum type)
      {
        var maxMin = new FilamentConstants.Temperature.MaxMin
        {
          Max = 285f,
          Min = 100f
        };
        switch (type)
        {
          case FilamentSpool.TypeEnum.ABS:
            maxMin.Max = 285f;
            maxMin.Min = 240f;
            break;
          case FilamentSpool.TypeEnum.PLA:
          case FilamentSpool.TypeEnum.CAM:
            maxMin.Max = 250f;
            maxMin.Min = 190f;
            break;
          case FilamentSpool.TypeEnum.HIPS:
            maxMin.Max = 285f;
            maxMin.Min = 235f;
            break;
          case FilamentSpool.TypeEnum.FLX:
          case FilamentSpool.TypeEnum.TGH:
            maxMin.Max = 285f;
            maxMin.Min = 215f;
            break;
          case FilamentSpool.TypeEnum.ABS_R:
            maxMin.Max = 270f;
            maxMin.Min = 200f;
            break;
          default:
            throw new NotImplementedException("Constants.MaxMinForFilamentType is not implemented for type: " + (object) type);
        }
        return maxMin;
      }

      public static int Default(FilamentSpool.TypeEnum type)
      {
        var num = 0;
        switch (type)
        {
          case FilamentSpool.TypeEnum.ABS:
            num = 275;
            break;
          case FilamentSpool.TypeEnum.PLA:
          case FilamentSpool.TypeEnum.CAM:
            num = 215;
            break;
          case FilamentSpool.TypeEnum.HIPS:
            num = 265;
            break;
          case FilamentSpool.TypeEnum.FLX:
            num = 220;
            break;
          case FilamentSpool.TypeEnum.TGH:
            num = 220;
            break;
          case FilamentSpool.TypeEnum.ABS_R:
            num = 240;
            break;
        }
        if (num == 0)
        {
          throw new NotImplementedException("Constants.MaxMinForFilamentType is not implemented for type: " + (object) type);
        }

        return num;
      }

      public static int BedDefault(FilamentSpool.TypeEnum type)
      {
        var num = 0;
        switch (type)
        {
          case FilamentSpool.TypeEnum.ABS:
            num = 100;
            break;
          case FilamentSpool.TypeEnum.PLA:
          case FilamentSpool.TypeEnum.HIPS:
          case FilamentSpool.TypeEnum.CAM:
            num = 85;
            break;
          case FilamentSpool.TypeEnum.FLX:
          case FilamentSpool.TypeEnum.TGH:
            num = 0;
            break;
          case FilamentSpool.TypeEnum.ABS_R:
            num = 90;
            break;
        }
        return num;
      }

      public struct MaxMin
      {
        public float Max;
        public float Min;
      }
    }

    public enum Branding
    {
      Other,
      Ink,
      ExpertInk,
      ToughInk,
      SolidInk,
      TransparentInk,
      TriChromeInk,
    }

    public enum ColorsEnum
    {
      Black = 0,
      Blue = 1,
      Brown = 2,
      Gold = 3,
      Pink = 4,
      Green = 5,
      LightBlue = 6,
      LightGreen = 7,
      Magenta = 8,
      Natural = 9,
      NeonBlue = 10, // 0x0000000A
      NeonOrange = 11, // 0x0000000B
      NeonYellow = 12, // 0x0000000C
      Orange = 13, // 0x0000000D
      Purple = 14, // 0x0000000E
      Red = 15, // 0x0000000F
      Silver = 16, // 0x00000010
      White = 17, // 0x00000011
      Yellow = 18, // 0x00000012
      Other = 19, // 0x00000013
      Clear = 20, // 0x00000014
      PhantomWhite = 21, // 0x00000015
      HoneyClear = 22, // 0x00000016
      FuchsiaRed = 23, // 0x00000017
      CrimsonRed = 24, // 0x00000018
      FireOrange = 25, // 0x00000019
      MangoYellow = 26, // 0x0000001A
      ShamrockGreen = 27, // 0x0000001B
      CobaltBlue = 28, // 0x0000001C
      CaribbeanBlue = 29, // 0x0000001D
      MulberryPurple = 30, // 0x0000001E
      TitaniumSilver = 31, // 0x0000001F
      CharcoalBlack = 32, // 0x00000020
      WhitePearl = 33, // 0x00000021
      CrystalClear = 34, // 0x00000022
      LightFuchsia = 35, // 0x00000023
      DeepCrimson = 36, // 0x00000024
      DeepShamrock = 37, // 0x00000025
      DeepColbalt = 38, // 0x00000026
      LightCarribean = 39, // 0x00000027
      DeepMulberry = 40, // 0x00000028
      SatelliteSilver = 41, // 0x00000029
      DeepLemon = 42, // 0x0000002A
      SunsetOrange = 43, // 0x0000002B
      OnyxBlack = 44, // 0x0000002C
      DragonRedTouch = 45, // 0x0000002D
      DragonRedHot = 46, // 0x0000002E
      RoseRedTouch = 47, // 0x0000002F
      CoralOrangeTouch = 48, // 0x00000030
      CoralOrangeWarm = 49, // 0x00000031
      DespicableYellowTouch = 50, // 0x00000032
      MonsterGreenTouch = 51, // 0x00000033
      GenieBlueTouch = 52, // 0x00000034
      GenieBlueIce = 53, // 0x00000035
      GargoyleBlackTouch = 54, // 0x00000036
      TrichromeNebula = 55, // 0x00000037
      TrichromeTiger = 56, // 0x00000038
      Grey = 57, // 0x00000039
      MilkWhite = 58, // 0x0000003A
      DurableClear = 59, // 0x0000003B
      SuperBlue = 60, // 0x0000003C
      MightyBlue = 61, // 0x0000003D
      YogaGreen = 62, // 0x0000003E
      RockyGreen = 63, // 0x0000003F
      OrangeHeft = 64, // 0x00000040
      FlexibleRuby = 65, // 0x00000041
      StrongPurple = 66, // 0x00000042
      RavenPurple = 67, // 0x00000043
      UtilityGrey = 68, // 0x00000044
      ImpactBlack = 69, // 0x00000045
      ParakeetTouch = 70, // 0x00000046
      PeacockTouch = 71, // 0x00000047
      EclectusTouch = 72, // 0x00000048
      CandyGreen = 73, // 0x00000049
      DeepCobalt = 74, // 0x0000004A
      CoolGray = 75, // 0x0000004B
      BrickRed = 76, // 0x0000004C
      FireTruck = 77, // 0x0000004D
      PumpkinOrange = 78, // 0x0000004E
      BerryPurple = 79, // 0x0000004F
      BlueMoon = 80, // 0x00000050
      PearlWhite = 81, // 0x00000051
      AntiqueGold = 82, // 0x00000052
      SnakeCopper = 83, // 0x00000053
      AppleGreen = 84, // 0x00000054
      SkyBlue = 85, // 0x00000055
      OrientRed = 86, // 0x00000056
      BlackCarbonFiber = 150, // 0x00000096
    }
  }
}
