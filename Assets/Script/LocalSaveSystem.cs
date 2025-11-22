using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

public static class LocalSaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "Saves");

    // 保存存档 (覆盖或新建)
    public static void SaveToDisk(SaveDataDTO data, string fileName)
    {
        if (!Directory.Exists(SavePath)) Directory.CreateDirectory(SavePath);

        // 更新时间戳为当前时间
        data.timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        // 记录文件名以便运行时使用
        data.LocalFileName = fileName;

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        string filePath = Path.Combine(SavePath, fileName + ".json");
        
        File.WriteAllText(filePath, json);
        Debug.Log($"[LocalSave] Saved: {fileName}");
    }

    // 读取指定存档
    public static SaveDataDTO LoadFromDisk(string fileName)
    {
        string filePath = Path.Combine(SavePath, fileName + ".json");
        if (!File.Exists(filePath)) return null;

        string json = File.ReadAllText(filePath);
        var data = JsonConvert.DeserializeObject<SaveDataDTO>(json);
        data.LocalFileName = fileName; // 注入文件名
        return data;
    }

    // 获取所有存档列表 (用于 UI 显示)
    public static List<SaveDataDTO> GetAllSaves()
    {
        if (!Directory.Exists(SavePath)) return new List<SaveDataDTO>();

        var info = new DirectoryInfo(SavePath);
        var files = info.GetFiles("*.json");

        return files.Select(f => {
            try {
                string json = File.ReadAllText(f.FullName);
                var data = JsonConvert.DeserializeObject<SaveDataDTO>(json);
                data.LocalFileName = Path.GetFileNameWithoutExtension(f.Name);
                return data;
            } catch { return null; }
        })
        .Where(d => d != null)
        .OrderByDescending(s => s.timestamp) // 按时间戳倒序
        .ToList();
    }
}