using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class SaveManager
{
    static Dictionary<string, float> floatDictionary = new Dictionary<string, float>();
    static Dictionary<string, int> intDictionary = new Dictionary<string, int>();
    static Dictionary<string, string> stringDictionary = new Dictionary<string, string>();
    static Dictionary<string, Vector3> vectorDictionary = new Dictionary<string, Vector3>();
    static Dictionary<string, Quaternion> rotationDictionary = new Dictionary<string, Quaternion>();
    static Dictionary<string, Vector3> scaleDictionary = new Dictionary<string, Vector3>();


    private static List<string> floatKeyLocal = new List<string>(), intKeyLocal = new List<string>(), stringKeyLocal = new List<string>(), vectorKeyLocal = new List<string>(), rotationKeyLocal = new List<string>(), scaleKeyLocal = new List<string>();
    private static List<float> floatValueLocal = new List<float>();
    private static List<int> intValueLocal = new List<int>();
    private static List<string> stringValueLocal = new List<string>();
    private static List<Vector3> vectorValueLocal = new List<Vector3>();
    private static List<Quaternion> rotationValueLocal = new List<Quaternion>();
    private static List<Vector3> scaleValueLocal = new List<Vector3>();


    public static void SetFloat(string key, float value)
    {
        floatDictionary[key] = value;
    }

    public static void SetInt(string key, int value)
    {
        intDictionary[key] = value;
    }

    public static void SetString(string key, string value)
    {
        stringDictionary[key] = value;
    }

    public static void SetVector(string key, Vector3 value)
    {
        vectorDictionary[key] = value;
    }

    public static void SetRotation(string key, Quaternion value)
    {
        rotationDictionary[key] = value;
    }

    public static void SetScale(string key, Vector3 value)
    {
        scaleDictionary[key] = value;
    }

    public static void Save()
    {

        PopulateSaveLists();

        SaveData saveData = new SaveData
        {
            floatKeys = floatKeyLocal,
            floatValues = floatValueLocal,

            intKeys = intKeyLocal,
            intValues = intValueLocal,

            stringKeys = stringKeyLocal,
            stringValues = stringValueLocal,

            vectorKeys = vectorKeyLocal,
            vectorValues = vectorValueLocal,

            rotationKeys = rotationKeyLocal,
            rotationValues = rotationValueLocal,

            scaleKeys = scaleKeyLocal,
            scaleValues = scaleValueLocal
        };

        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(Application.dataPath + "/SaveData.json", json);
    }

    private static void PopulateSaveLists()
    {
        foreach(var f in floatDictionary)
        {
            floatKeyLocal.Add(f.Key);
            floatValueLocal.Add(f.Value);
        }

        foreach(var i in intDictionary)
        {
            intKeyLocal.Add(i.Key);
            intValueLocal.Add(i.Value);
        }

        foreach (var s in stringDictionary)
        {
            stringKeyLocal.Add(s.Key);
            stringValueLocal.Add(s.Value);
        }

        foreach (var v in vectorDictionary)
        {
            vectorKeyLocal.Add(v.Key);
            vectorValueLocal.Add(v.Value);
        }

        foreach (var r in rotationDictionary)
        {
            rotationKeyLocal.Add(r.Key);
            rotationValueLocal.Add(r.Value);
        }

        foreach (var s in scaleDictionary)
        {
            scaleKeyLocal.Add(s.Key);
            scaleValueLocal.Add(s.Value);
        }
    }

    public static void Load()
    {
        if (!File.Exists(Application.dataPath + "SaveData.json"))
            return;

        string json = File.ReadAllText(Application.dataPath + "SaveData.json");

        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        for (int i = 0; i < saveData.floatKeys.Count; i++)
            floatDictionary[saveData.floatKeys[i]] = saveData.floatValues[i];

        for (int i = 0; i < saveData.intKeys.Count; i++)
            intDictionary[saveData.intKeys[i]] = saveData.intValues[i];

        for (int i = 0; i < saveData.stringKeys.Count; i++)
            stringDictionary[saveData.stringKeys[i]] = saveData.stringValues[i];

        for (int i = 0; i < saveData.vectorKeys.Count; i++)
            vectorDictionary[saveData.vectorKeys[i]] = saveData.vectorValues[i];

        for (int i = 0; i < saveData.rotationKeys.Count; i++)
            rotationDictionary[saveData.rotationKeys[i]] = saveData.rotationValues[i];

        for (int i = 0; i < saveData.scaleKeys.Count; i++)
            scaleDictionary[saveData.scaleKeys[i]] = saveData.scaleValues[i];
    }

    public static float GetFloat(string key, float defaultValue)
    {
        if (floatDictionary.TryGetValue(key, out float value)) 
            return value;

        return defaultValue;
    }

    public static int GetInt(string key, int defaultValue)
    {
        if (intDictionary.TryGetValue(key, out int value))
            return value;

        return defaultValue;
    }

    public static string GetString(string key, string defaultValue)
    {
        if (stringDictionary.TryGetValue(key, out string value))
            return value;

        return defaultValue;
    }

    public static Vector3 GetVector(string key, Vector3 defaultValue)
    {
        if (vectorDictionary.TryGetValue(key, out Vector3 value))
            return value;

        return defaultValue;
    }

    public static Quaternion GetRotation(string key, Quaternion defaultValue)
    {
        if (rotationDictionary.TryGetValue(key, out Quaternion value))
            return value;

        return defaultValue;
    }

    public static Vector3 GetScale(string key, Vector3 defaultValue)
    {
        if (scaleDictionary.TryGetValue(key, out Vector3 value))
            return value;

        return defaultValue;
    }

    [System.Serializable]
    private class SaveData
    {
        public List<string> floatKeys;
        public List<float> floatValues;

        public List<string> intKeys;
        public List<int> intValues;

        public List<string> stringKeys;
        public List<string> stringValues;

        public List<string> vectorKeys;
        public List<Vector3> vectorValues;

        public List<string> rotationKeys;
        public List<Quaternion> rotationValues;

        public List<string> scaleKeys;
        public List<Vector3> scaleValues;
    }
}
