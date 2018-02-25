using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class MemoryManager : MonoBehaviour, MemoryPopulater {

    public const int CurrentSaveVersion = 1;
    public const int LowestSupportedSaveVersion = 1;

    private const string SystemMemoryName = "erebus.sav";
    private const string SaveGameSuffix = ".sav";
    private const float ScreenshotScaleFactor = 6.0f;
    private const float LoadDelaySeconds = 1.5f;
    private const int MaxMessages = 200;

    private Dictionary<string, bool> switches;
    private Dictionary<string, int> variables;
    private Dictionary<string, int> maxSeenCommands;
    private List<MemoryPopulater> listeners;
    private List<LogItem> messageHistory;
    private Texture2D screenshot;
    private float lastSystemSavedTimestamp;

    // this thing will be read by the dialog scene when spawning
    // if non-null, it'll be loaded automatically
    public Memory ActiveMemory { get; set; }

    // global state, unrelated to playthroughs and things like that
    // things like total play time
    public SystemMemory SystemMemory { get; set; }

    public void Awake() {
        switches = new Dictionary<string, bool>();
        listeners = new List<MemoryPopulater>();
        variables = new Dictionary<string, int>();
        maxSeenCommands = new Dictionary<string, int>();
        messageHistory = new List<LogItem>();
        lastSystemSavedTimestamp = Time.realtimeSinceStartup;
        LoadOrCreateSystemMemory();
        RegisterMemoryPopulater(this);

        int width = (int)(Screen.width / ScreenshotScaleFactor);
        int height = (int)(Screen.height / ScreenshotScaleFactor);
        screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
    }

    public void OnDestroy() {
        DestroyImmediate(screenshot);
    }

    public void RegisterMemoryPopulater(MemoryPopulater populater) {
        listeners.Add(populater);
    }

    public void AppendLogItem(LogItem item) {
        messageHistory.Add(item);
        if (messageHistory.Count > MaxMessages) {
            messageHistory.RemoveAt(0);
        }
    }

    public List<LogItem> GetMessageHistory() {
        return messageHistory;
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

    public int GetVariable(string variableName) {
        if (!variables.ContainsKey(variableName)) {
            variables[variableName] = 0;
        }
        return variables[variableName];
    }

    public void IncrementVariable(string variableName) {
        variables[variableName] = GetVariable(variableName) + 1;
    }

    public void DecrementVariable(string variableName) {
        variables[variableName] = GetVariable(variableName) - 1;
    }

    public bool HasSeenCommand(string sceneName, int commandIndex) {
        if (maxSeenCommands.ContainsKey(sceneName)) {
            return maxSeenCommands[sceneName] >= commandIndex;
        } else {
            return false;
        }
    }

    public void AcknowledgeCommand(string sceneName, int commandIndex) {
        if (maxSeenCommands.ContainsKey(sceneName)) {
            maxSeenCommands[sceneName] = Math.Max(maxSeenCommands[sceneName], commandIndex);
        } else {
            maxSeenCommands[sceneName] = commandIndex;
        }
    }

    public void SaveToSlot(int slot) {
        Memory memory = new Memory();

        foreach (MemoryPopulater listener in listeners) {
            listener.PopulateMemory(memory);
        }

        ScenePlayer player = FindObjectOfType<ScenePlayer>();
        memory.screen = player.ToMemory();

        foreach (string key in variables.Keys) {
            memory.variableKeys.Add(key);
        }
        foreach (int value in variables.Values) {
            memory.variableValues.Add(value);
        }

        AttachScreenshotToMemory(memory);

        WriteJsonToFile(memory, FilePathForSlot(slot));
        SystemMemory.lastSlotSaved = slot;
        SaveSystemMemory();
    }

    // will instantly change globals and avatar to match the memory
    // assumes the main scene is the current scene
    public void LoadMemory(Memory memory) {
        foreach (MemoryPopulater listener in listeners) {
            listener.PopulateFromMemory(memory);
        }
    }

    public void LoadFromLastSaveSlot() {
        LoadMemory(GetMemoryForSlot(SystemMemory.lastSlotSaved));
    }

    public Memory GetMemoryForSlot(int slot) {
        string fileName = FilePathForSlot(slot);
        if (File.Exists(fileName)) {
            return ReadJsonFromFile<Memory>(fileName);
        } else {
            return null;
        }
    }

    public bool AnyMemoriesExist() {
        // sort of a todo
        return GetMemoryForSlot(0) != null;
    }

    public void SaveSystemMemory() {

        // constants we keep track of
        float currentTimestamp = Time.realtimeSinceStartup;
        float deltaSeconds = currentTimestamp - lastSystemSavedTimestamp;
        lastSystemSavedTimestamp = currentTimestamp;
        SystemMemory.totalPlaySeconds += (int)Math.Round(deltaSeconds);

        // seen history
        SystemMemory.maxSeenCommandsKeys.Clear();
        foreach (string key in maxSeenCommands.Keys) {
            SystemMemory.maxSeenCommandsKeys.Add(key);
        }
        SystemMemory.maxSeenCommandsValues.Clear();
        foreach (int value in maxSeenCommands.Values) {
            SystemMemory.maxSeenCommandsValues.Add(value);
        }

        // other garbage in other managers
        SystemMemory.settings = Global.Instance().Settings.ToMemory();

        WriteJsonToFile(SystemMemory, GetSystemMemoryFilepath());
    }

    public void PopulateMemory(Memory memory) {
        memory.switchKeys = new List<string>();
        memory.switchValues = new List<bool>();
        foreach (KeyValuePair<string, bool> pair in switches) {
            memory.switchKeys.Add(pair.Key);
            memory.switchValues.Add(pair.Value);
        }

        memory.savedAt = CurrentTimestamp();
        memory.saveVersion = CurrentSaveVersion;
    }

    public void PopulateFromMemory(Memory memory) {
        // just need to handle the stuff actually stored in this manager
        switches.Clear();
        for (int i = 0; i < memory.switchKeys.Count; i += 1) {
            switches[memory.switchKeys[i]] = memory.switchValues[i];
        }
        variables.Clear();
        for (int i = 0; i < memory.variableKeys.Count; i += 1) {
            variables[memory.variableKeys[i]] = memory.variableValues[i];
        }
    }

    public Sprite SpriteFromBase64(string encodedString) {
        int width = (int)(Screen.width / ScreenshotScaleFactor);
        int height = (int)(Screen.height / ScreenshotScaleFactor);
        byte[] pngBytes = Convert.FromBase64String(encodedString);
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        texture.LoadImage(pngBytes);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0, 0));
    }

    // takes a screenshot and keeps it in memory, to be saved later maybe
    public void RememberScreenshot() {
        int width = (int)(Screen.width / ScreenshotScaleFactor);
        int height = (int)(Screen.height / ScreenshotScaleFactor);
        RenderTexture renderTexture = new RenderTexture(width, height, 24);

        List<Camera> cameras = new List<Camera>(Camera.allCameras);
        cameras.Sort((Camera c1, Camera c2) => {
            return c2.transform.GetSiblingIndex().CompareTo(c1.transform.GetSiblingIndex());
        });
        foreach (Camera camera in cameras) {
            RenderTexture oldTexture = camera.targetTexture;
            camera.targetTexture = renderTexture;
            camera.Render();
            camera.targetTexture = oldTexture;
        }

        RenderTexture active = RenderTexture.active;
        RenderTexture.active = renderTexture;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        RenderTexture.active = active;
        Destroy(renderTexture);
    }

    private void WriteJsonToFile(object toSerialize, string fileName) {
        FileStream file = File.Open(fileName, FileMode.Create);
        StreamWriter writer = new StreamWriter(file);
        writer.Write(JsonUtility.ToJson(toSerialize));
        writer.Flush();
        writer.Close();
        file.Close();
    }

    private T ReadJsonFromFile<T>(string fileName) {
        string json = File.ReadAllText(fileName);
        T result = JsonUtility.FromJson<T>(json);
        return result;
    }

    private double CurrentTimestamp() {
        return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }

    private void LoadOrCreateSystemMemory() {
        string path = GetSystemMemoryFilepath();
        if (File.Exists(path)) {
            SystemMemory = ReadJsonFromFile<SystemMemory>(path);
            Global.Instance().Settings.PopulateFromMemory(SystemMemory.settings);
        } else {
            SystemMemory = new SystemMemory();
            Global.Instance().Settings.LoadDefaults();
        }

        // time to populate from system memory
        maxSeenCommands.Clear();
        for (int i = 0; i < SystemMemory.maxSeenCommandsKeys.Count; i += 1) {
            variables[SystemMemory.maxSeenCommandsKeys[i]] = SystemMemory.maxSeenCommandsValues[i];
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

    private DateTime TimestampToDateTime(double timestamp) {
        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(timestamp).ToLocalTime();
        return dtDateTime;
    }

    private void AttachScreenshotToMemory(Memory memory) {
        byte[] pngBytes = screenshot.EncodeToPNG();
        memory.base64ScreenshotPNG = Convert.ToBase64String(pngBytes);
    }

    private IEnumerator LoadActiveMemoryRoutine() {
        FadeComponent fade = FindObjectOfType<FadeComponent>();
        yield return fade.FadeToBlackRoutine();
        yield return new WaitForSeconds(LoadDelaySeconds);
        //ScenePlayer.LoadScreen();
    }
}
