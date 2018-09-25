using System.IO;

/// <summary>
/// 改变脚本模板
/// </summary>
public class ChangeScriptTemplates : UnityEditor.AssetModificationProcessor
{
    // 脚本注释模板
    private static string mTemplate =
      "/* =========================\r\n"
    + " * 描 述：\r\n"
    + " * 作 者：\r\n"
    + " * 创建时间：#CreateTime#\r\n"
    + " * ========================= */\r\n";

    // 创建资源调用
    public static void OnWillCreateAsset(string varPath)
    {
        // 只修改C#脚本
        varPath = varPath.Replace(".meta", "");
        if (varPath.EndsWith(".cs"))
        {
            string tempAllText = mTemplate;
            tempAllText += File.ReadAllText(varPath);
            // 替换字符串为系统时间
            tempAllText = tempAllText.Replace("#CreateTime#", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            File.WriteAllText(varPath, tempAllText);
        }
    }
}
