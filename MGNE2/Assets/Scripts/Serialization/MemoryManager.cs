using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class MemoryManager : MonoBehaviour {

    public const int CurrentSaveVersion = 0;
    public const int LowestSupportedSaveVersion = 0;

    private const string SystemMemoryName = "system.sav";
    private const string SaveGameSuffix = ".sav";

    private Dictionary<string, bool> switches;
    private float lastSystemSavedTimestamp;

    // global state, unrelated to playthroughs and things like that
    // things like total play time
    public SystemMemory SystemMemory { get; set; }

    public void Awake() {
        switches = new Dictionary<string, bool>();
        lastSystemSavedTimestamp = Time.realtimeSinceStartup;
        LoadOrCreateSystemMemory();
    }

    public void WriteJsonToFile(object toSerialize, string fileName) {
        FileStream file = File.Open(fileName, FileMode.Create);
        StreamWriter writer = new StreamWriter(file);
        writer.Write(JsonUtility.ToJson(toSerialize));
        writer.Flush();
        writer.Close();
        file.Close();
    }

    public T ReadJsonFromFile<T>(string fileName) {
        string json = File.ReadAllText(fileName);
        T result = JsonUtility.FromJson<T>(json);
        return result;
    }

    public Memory ToMemory() {
        return new Memory(this);
    }

    public void PopulateFromMemory(Memory memory) {
        switches.Clear();
        for (int i = 0; i < memory.switchKeys.Count; i += 1) {
            switches[memory.switchKeys[i]] = memory.switchValues[i];
        }

        // TODO: populate avatar etc
    }

    public bool GetSwitch(string switchName) {
        if (!switches.ContainsKey(switchName)) {
            return false;
        }
        return switches[switchName];
    }

    public void SetSwitch(string switchName, bool value) {
        switches[switchName] = value;
    }

    public void SaveToSlot(int slot) {
        WriteJsonToFile(ToMemory(), FilePathForSlot(slot));
        SystemMemory.lastSlotSaved = slot;
        SaveSystemMemory();
    }

    public void LoadMemory(Memory memory) {
        StartCoroutine(LoadMemoryRoutine(memory));
    }

    public Memory GetMemoryForSlot(int slot) {
        string fileName = FilePathForSlot(slot);
        if (File.Exists(fileName)) {
            return ReadJsonFromFile<Memory>(fileName);
        } else {
            return null;
        }
    }

    public void SaveSystemMemory() {

        // constants we keep track of
        float currentTimestamp = Time.realtimeSinceStartup;
        float deltaSeconds = currentTimestamp - lastSystemSavedTimestamp;
        lastSystemSavedTimestamp = currentTimestamp;
        SystemMemory.totalPlaySeconds += (int)Math.Round(deltaSeconds);

        // TODO: settings and stuff eventually

        WriteJsonToFile(SystemMemory, GetSystemMemoryFilepath());
    }

    private void LoadOrCreateSystemMemory() {
        string path = GetSystemMemoryFilepath();
        if (File.Exists(path)) {
            SystemMemory = ReadJsonFromFile<SystemMemory>(path);

            // TODO: settings and stuff eventually
        } else {
            SystemMemory = new SystemMemory();
        }
    }

    private string GetSystemMemoryFilepath() {
        return Application.persistentDataPath + "/" + SystemMemoryName;
    }

    private string FilePathForSlot(int slot) {
        string fileName = Application.persistentDataPath + "/";
        fileName += Convert.ToString(slot);
        fileName += SaveGameSuffix;
        return fileName;
    }

    public DateTime TimestampToDateTime(double timestamp) {
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(timestamp).ToLocalTime();
        return dtDateTime;
    }

    public double CurrentTimestamp() {
        return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }

    private IEnumerator LoadMemoryRoutine(Memory memory) {
        yield return null;
    }
}
