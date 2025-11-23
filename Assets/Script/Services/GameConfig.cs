using System.Collections.Generic;
using UnityEngine;

// 这个脚本定义了游戏中的关卡配置和相关数据结构

// 关卡 ID 枚举
public enum TherapyLevelID
{
    None,
    Mindfulness,       // 正念冥想
    MoodJournal,       // 情绪日记
    Reappraisal,       // 认知重评
    Gratitude          // 感恩练习
}

[System.Serializable]
public class LevelData
{
    public TherapyLevelID id;
    public string title;
    public string description;
    // public string sceneName; // 未来用来加载场景
}

public static class GameConfig
{
    // 静态配置所有关卡
    public static List<LevelData> AllLevels = new List<LevelData>()
    {
        new LevelData { id = TherapyLevelID.Mindfulness, title = "正念冥想练习", description = "通过呼吸调节平复情绪，回到当下。" },
        new LevelData { id = TherapyLevelID.MoodJournal, title = "情绪日记", description = "记录当下的情绪触发点，分析心理模式。" },
        new LevelData { id = TherapyLevelID.Reappraisal, title = "认知重评挑战", description = "换个角度看问题，挑战消极的自动化思维。" },
        new LevelData { id = TherapyLevelID.Gratitude, title = "感恩练习", description = "发现生活中的微小美好，提升心理韧性。" }
    };

    // 辅助方法：根据字符串查找 ID
    public static TherapyLevelID ParseLevelID(string levelNameFromBackend)
    {
        if (string.IsNullOrEmpty(levelNameFromBackend)) return TherapyLevelID.None;
        
        // 这里做简单的关键词匹配，防止后端名字和前端不完全一致
        if (levelNameFromBackend.Contains("正念")) return TherapyLevelID.Mindfulness;
        if (levelNameFromBackend.Contains("日记")) return TherapyLevelID.MoodJournal;
        if (levelNameFromBackend.Contains("认知")) return TherapyLevelID.Reappraisal;
        if (levelNameFromBackend.Contains("感恩")) return TherapyLevelID.Gratitude;
        
        return TherapyLevelID.None;
    }
}