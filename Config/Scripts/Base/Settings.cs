using System;
using UnityEngine;
public sealed class Settings

{
    private static readonly Settings instance = new Settings();
    public Config Config;
    static Settings()
    {
    }

    private Settings()
    {

    }

    
    public static Settings Instance
    {
        get
        {
            return instance;
        }
    }
 
}

