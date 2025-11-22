public static class GameLaunchConfig
{
    // 如果为空，代表开始新游戏；如果有值，代表读取该存档
    public static string TargetSaveFileName = ""; 
    
    // 辅助属性
    public static bool IsNewGame => string.IsNullOrEmpty(TargetSaveFileName);
}