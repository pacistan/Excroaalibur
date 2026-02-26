using UnityEngine;
using System.IO;
using File = System.IO.File;

[System.Serializable]
// T is Game State Data, V is Serializable Data
public abstract class GSaveHandler<T> where T : class
{
    protected string fullFilePath => Path.Combine(Application.streamingAssetsPath, _filePath + ".json");

    [SerializeField]
    protected string _filePath;

    public bool IsSaveFileCreated() => File.Exists(fullFilePath);

    public void SerializeToJson()
    {
        T data = GenerateSaveData();
        string jsonData = JsonUtility.ToJson(data, true);
        File.WriteAllText(fullFilePath, jsonData);
    }

    public T DeserializeFromJson()
    {
        if(!IsSaveFileCreated())
        {
            Debug.LogWarning($"File {fullFilePath} does not exist");
            return null;
        }
        string jsonData = File.ReadAllText(fullFilePath);
        T data = JsonUtility.FromJson<T>(jsonData);
        return data;
    }

    public void DeleteSaveFile()
    {
        if (IsSaveFileCreated())
        {
            File.Delete(fullFilePath);
        }
    }

    protected abstract T GenerateSaveData();


}

