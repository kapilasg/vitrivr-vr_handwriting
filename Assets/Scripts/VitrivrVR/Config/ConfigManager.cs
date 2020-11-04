﻿using System.IO;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Utils;
using UnityEngine;

namespace VitrivrVR.Config
{
  public static class ConfigManager
  {
    private const string ConfigFileName = "vitrivr-vr.json";

    public static readonly VitrivrVrConfig Config = GetConfig();

    private static VitrivrVrConfig GetConfig()
    {
      return File.Exists(GetConfigFilePath())
        ? ReadAndMerge()
        : VitrivrVrConfig.GetDefault();
    }

    private static string GetConfigFilePath()
    {
      string folder;
#if UNITY_EDITOR
      folder = Application.dataPath;
#else
      folder = Application.persistentDataPath;
#endif
      return Path.Combine(folder, ConfigFileName);
    }

    private static VitrivrVrConfig ReadAndMerge()
    {
      var streamReader = File.OpenText(GetConfigFilePath());
      var json = streamReader.ReadToEnd();
      streamReader.Close();
      var config = VitrivrVrConfig.GetDefault();
      JsonUtility.FromJsonOverwrite(json, config);
      return config;
    }
  }
}