using M3D.Spooling.Common;

namespace M3D.Spooling.Core.Controllers
{
  internal interface IGCodePluginable
  {
    PluginResult LinkGCodeWithPlugin(string gcode, string pluginID);

    CommandResult RegisterExternalPluginGCodes(string pluginID, string[] gCodeList);
  }
}
