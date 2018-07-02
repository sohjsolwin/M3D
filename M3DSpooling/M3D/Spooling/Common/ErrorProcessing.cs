using System;

namespace M3D.Spooling.Common
{
  public static class ErrorProcessing
  {
    public static string TranslateError(string received, out int error_code)
    {
      var str1 = (string) null;
      var str2 = (string) null;
      var result = -1;
      if (received.StartsWith("Error:") && received.Length > "Error:".Length)
      {
        str1 = received.Substring("Error:".Length);
      }
      else if (received.StartsWith("!!") && received.Length > "!!".Length)
      {
        str1 = received.Substring("!!".Length);
      }
      else if (received.StartsWith("e") && received.Length > "e".Length)
      {
        str1 = received.Substring("e".Length);
      }

      if (!string.IsNullOrEmpty(str1))
      {
        string[] strArray = str1.Split(new char[1]{ ' ' }, StringSplitOptions.None);
        if (int.TryParse(strArray[0], out result))
        {
          switch (result)
          {
            case 0:
              str2 = "Firmware parser returned parse error";
              break;
            case 1:
              str2 = "Firmware parser returned not supported protocol";
              break;
            case 2:
              str2 = "Firmware parser returned ASCII buffer overflow";
              break;
            case 3:
              str2 = "Firmware parser returned number expected";
              break;
            case 4:
              str2 = "Firmware parser returned number overload";
              break;
            case 5:
              str2 = "Firmware parser returned unknown";
              break;
            case 6:
              str2 = "Firmware interpreter returned unsupported";
              break;
            case 7:
              str2 = "Firmware interpreter returned unknown";
              break;
            case 8:
              str2 = "Firmware parser was unable to buffer the G-Code's text field due its excessive length";
              break;
            case 9:
              str2 = "Firmware parser detected a checksum failure within a G-Code line";
              break;
            case 1000:
              str2 = "Error-1000: M110 without line number";
              break;
            case 1001:
              str2 = "Error-1001: Cannot cold extrude";
              break;
            case 1002:
              str2 = "Error-1002: Cannot calibrate in unknown state";
              break;
            case 1003:
              str2 = "Error-1003: Unknown G-Code";
              break;
            case 1004:
              str2 = "Error-1004: Unknown M-Code";
              break;
            case 1005:
              str2 = "Error-1005: Unknown command";
              break;
            case 1006:
              str2 = "Error-1006: Heater failed";
              break;
            case 1007:
              str2 = "Error-1007: Move too large";
              break;
            case 1008:
              str2 = "Info: Printer has been inactive for too long. The motors and heater have been turned off.";
              break;
            case 1009:
              str2 = "Error-1009: M618 or M619 was sent with invalid parameters";
              break;
            case 1010:
              str2 = "Error-1010: There was an error initializing the hardware.";
              break;
            case 1011:
              str2 = "Error-1011: EEPROM address read/write size is invalid";
              break;
            case 1012:
              if (4 == strArray.Length)
              {
                str2 = "Error-1012: Commanded temperature (" + strArray[3].Trim() + ") is outside the commandable range (" + strArray[1].Trim() + ", " + strArray[2].Trim() + ")";
                break;
              }
              str2 = "Error-1012: Commanded temperature is outside the commandable range";
              break;
            case 1013:
              str2 = "Error-1013: Unable to start bootloader because the IPC is busy.";
              break;
            case 1014:
              str2 = "Error-1014: Heated bed failed";
              break;
            case 1015:
              str2 = "Error-1015: SD Card - File not found";
              break;
            case 1016:
              str2 = "Error-1016: SD Card - File not open for reading";
              break;
            case 1017:
              str2 = "Error-1017: SD Card - File not open for writing";
              break;
            case 1018:
              str2 = "Error-1018: SD Card - Unable to open file";
              break;
            case 1019:
              str2 = "Error-1019: SD Card - File already open";
              break;
            case 1020:
              str2 = "Error-1020: SD Card - Cannot remove open file";
              break;
            case 1021:
              str2 = "Error-1021: SD Card - File system could not be mounted";
              break;
            case 1022:
              str2 = "Error-1022: SD Card - Unknown error while reading file";
              break;
            case 1023:
              str2 = "Error-1023: SD Card - Unknown error while writing file";
              break;
            case 1024:
              str2 = "Error-1024: SD Card - Unknown file operation error";
              break;
            case 1025:
              str2 = "Error-1025: Shutting down for safety reasons as communications between processors were lost";
              break;
          }
        }
      }
      if (string.IsNullOrEmpty(str2))
      {
        str2 = received;
      }

      error_code = result;
      return str2;
    }

    public enum SoftErrorCodes
    {
      PROCESS_PARSER_RET_PARSE_ERROR_RL,
      PROCESS_PARSER_RET_NOT_SUPPORTED_PROTOCOL_RL,
      PROCESS_PARSER_RET_ASCII_BUFFER_OVERFLOW_RL,
      PROCESS_PARSER_RET_NUMBER_EXPECTED_RL,
      PROCESS_PARSER_RET_NUMBER_OVERLOAD_RL,
      PROCESS_PARSER_RET_UNKNOWN_RL,
      PROCESS_INTERPERETER_RET_UNSUPPORTED_CMD_RL,
      PROCESS_INTERPERETER_RET_UNKNOWN_RL,
      PROCESS_PARSER_RET_TEXT_FIELD_OVERFLOW_RL,
      PROCESS_PARSER_RET_CHECKSUM_FAILED_RL,
    }

    public enum CriticalErrorCodes
    {
      CRITICAL_ERR_M110_WITHOUT_LINE_NUM = 1000, // 0x000003E8
      CRITICAL_ERR_COLD_EXTRUDE = 1001, // 0x000003E9
      CRITICAL_ERR_CANNOT_CALIBRATE_IN_UNKNOWN_STATE = 1002, // 0x000003EA
      CRITICAL_ERR_UNKNOWN_G_CODE = 1003, // 0x000003EB
      CRITICAL_ERR_UNKNOWN_M_CODE = 1004, // 0x000003EC
      CRITICAL_ERR_UNKNOWN_COMMAND = 1005, // 0x000003ED
      CRITICAL_ERR_HEATER_FAILED = 1006, // 0x000003EE
      CRITICAL_ERR_MOVE_TOO_LARGE = 1007, // 0x000003EF
      CRITICAL_ERR_INACTIVE_FOR_TOO_LONG = 1008, // 0x000003F0
      CRITICAL_ERR_INVALID_CMD = 1009, // 0x000003F1
      CRITICAL_ERR_MICROMOTION_CHIP_FAILURE = 1010, // 0x000003F2
      CRITICAL_ERR_BEYOND_ACCESSIBLE_LOCATION = 1011, // 0x000003F3
      CRITICAL_ERR_TEMPERATURE_OUT_OF_RANGE = 1012, // 0x000003F4
      CRITICAL_ERR_IPC_BUSY = 1013, // 0x000003F5
      CRITICAL_ERR_HEATED_BED_FAILED = 1014, // 0x000003F6
      CRITICAL_ERR_SD_FILENOTFOUND = 1015, // 0x000003F7
      CRITICAL_ERR_SD_FILENOTOPENFORREADING = 1016, // 0x000003F8
      CRITICAL_ERR_SD_FILENOTOPENFORWRITING = 1017, // 0x000003F9
      CRITICAL_ERR_SD_UNABLETOOPENFILE = 1018, // 0x000003FA
      CRITICAL_ERR_SD_FILEALREADYOPEN = 1019, // 0x000003FB
      CRITICAL_ERR_SD_CANNOTREMOVEOPENFILE = 1020, // 0x000003FC
      CRITICAL_ERR_SD_FSNOTMOUNTED = 1021, // 0x000003FD
      CRITICAL_ERR_SD_FILEREADERROR = 1022, // 0x000003FE
      CRITICAL_ERR_SD_FILEWRITEERROR = 1023, // 0x000003FF
      CRITICAL_ERR_SD_FILEOPERATIONERROR = 1024, // 0x00000400
      CRITICAL_ERR_IPC_NAK = 1025, // 0x00000401
    }
  }
}
