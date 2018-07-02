// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.JobOptions
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class JobOptions
  {
    [XmlAttribute]
    public bool use_raft_DetailOnly;
    [XmlAttribute]
    public bool wipe_tower;
    [XmlAttribute]
    public bool ooze_shield;
    [XmlAttribute]
    public bool autostart_ignorewarnings;
    [XmlAttribute]
    public bool turn_on_fan_before_print;
    [XmlAttribute]
    public bool use_wave_bonding;
    [XmlAttribute]
    public bool dont_use_preprocessors;
    [XmlAttribute]
    public bool dont_use_copy_to_spooler;
    [XmlAttribute]
    public bool use_support_DetailOnly;
    [XmlAttribute]
    public bool use_support_everywhere_DetailOnly;
    [XmlAttribute]
    public int quality_layer_resolution_DetailOnly;
    [XmlAttribute]
    public int fill_density_DetailOnly;
    [XmlAttribute]
    public bool calibrate_before_print;
    [XmlAttribute]
    public float calibrate_before_print_z;
    [XmlAttribute]
    public bool bounds_check_xy;
    [XmlAttribute]
    public bool use_heated_bed;
    [XmlAttribute]
    public bool use_fan_preprocessor;

    public JobOptions()
      : this(false)
    {
    }

    public JobOptions(JobOptions other)
    {
      use_raft_DetailOnly = other.use_raft_DetailOnly;
      wipe_tower = other.wipe_tower;
      ooze_shield = other.ooze_shield;
      autostart_ignorewarnings = other.autostart_ignorewarnings;
      turn_on_fan_before_print = other.turn_on_fan_before_print;
      use_wave_bonding = other.use_wave_bonding;
      dont_use_preprocessors = other.dont_use_preprocessors;
      use_support_DetailOnly = other.use_support_DetailOnly;
      use_support_everywhere_DetailOnly = other.use_support_everywhere_DetailOnly;
      quality_layer_resolution_DetailOnly = other.quality_layer_resolution_DetailOnly;
      fill_density_DetailOnly = other.fill_density_DetailOnly;
      calibrate_before_print = other.calibrate_before_print;
      calibrate_before_print_z = other.calibrate_before_print_z;
      bounds_check_xy = other.bounds_check_xy;
      use_fan_preprocessor = other.use_fan_preprocessor;
      use_heated_bed = other.use_heated_bed;
    }

    public JobOptions(bool default_value)
    {
      use_raft_DetailOnly = default_value;
      wipe_tower = default_value;
      ooze_shield = default_value;
      autostart_ignorewarnings = default_value;
      use_wave_bonding = default_value;
      dont_use_preprocessors = default_value;
      use_support_DetailOnly = default_value;
      turn_on_fan_before_print = default_value;
      use_heated_bed = default_value;
      quality_layer_resolution_DetailOnly = default_value ? 1 : 0;
      fill_density_DetailOnly = default_value ? 1 : 0;
      use_fan_preprocessor = true;
      bounds_check_xy = true;
      calibrate_before_print = false;
    }
  }
}
