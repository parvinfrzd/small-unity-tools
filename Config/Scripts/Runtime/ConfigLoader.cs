using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ConfigLoader : MonoBehaviour {

	public string ConfigFileName = "config.json";
	private string filePath;



	void Awake()
	{
		filePath = Application.dataPath + "/" + ConfigFileName;
		Debug.Log("Config file at : " + filePath);
		LoadConfig();
	}

	void LoadConfig()
	{
		try{
			if(!File.Exists(filePath))
			{
				CreateConfigTemplate();
			}
			else
			{
				string sringifiedJson = File.ReadAllText(filePath);
				Settings.Instance.Config =JsonUtility.FromJson<Config>(sringifiedJson);
			}
		}
		catch(Exception ex)
		{
			Debug.LogError("Config could not be loaded or created");
		}
		

	}

	void SaveConfig(Config config)
	{
		string dataAsJson = JsonUtility.ToJson (config);
        File.WriteAllText (filePath, dataAsJson);

	}

	void CreateConfigTemplate()
	{
		Config config = new Config();
		SaveConfig(config);

	}
}
