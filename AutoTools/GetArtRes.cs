using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

class GetArtRes:Editor
{
    private static string UnityEXEPath = "";
    private static string ArtStreamingAssetsAndroidPath = "";
    private static string ArtStreamingAssetsWindowPath = "";
    private static string MudGameStreamingAssetsPath = Application.streamingAssetsPath;
    private static string CallArtAndroidPath = AutoBuildForAndroid.rootPath + "Extension/CallArtBatAndroid.bat";
    private static string CallArtWindowPath = AutoBuildForAndroid.rootPath + "Extension/CallArtBatWinsow.bat";

    [MenuItem("Window/CompileArtRes/设置 Unity.exe 路径")]
    public static void SettingUnityPath()
    {//设置Unity.exe路径
        string selectPath = EditorUtility.OpenFilePanel("源目录", "", "");
        Debug.Log("selectPath: " + selectPath);
        if (!selectPath.EndsWith("Unity.exe"))
        {
            EditorUtility.DisplayDialog("提示", "请选择Unity.exe!", "OK");
            return;
        }

        Debug.Log("selectPath: " + selectPath);
        //保存
        UnityEXEPath = selectPath;
        EditorPrefs.SetString("UnityEXEPath", UnityEXEPath);
    }

    [MenuItem("Window/CompileArtRes/查看 Unity.exe 路径")]
    public static void LookUnityPath()
    {//设置Unity.exe路径
        UnityEXEPath = EditorPrefs.GetString("UnityEXEPath");
        EditorUtility.DisplayDialog("提示", UnityEXEPath, "OK");
    }

    [MenuItem("Window/CompileArtRes/设置 Android Art 路径")]
    public static void SettingAndroidArtPath()
    {//设置Android Art路径
        string selectPath = EditorUtility.SaveFolderPanel("源目录", "", "");
        Debug.Log("selectPath: " + selectPath);
        if (!selectPath.EndsWith("Mud_Art"))
        {
            EditorUtility.DisplayDialog("提示", "请选择Android Art工程!", "OK");
            return;
        }

        Debug.Log("selectPath: "+ selectPath);
        //保存
        ArtStreamingAssetsAndroidPath = selectPath;
        EditorPrefs.SetString("ArtStreamingAssetsAndroidPath", ArtStreamingAssetsAndroidPath);
    }

    [MenuItem("Window/CompileArtRes/查看 Android Art 路径")]
    public static void LookAndroidArtPath()
    {//设置Unity.exe路径
        ArtStreamingAssetsAndroidPath = EditorPrefs.GetString("ArtStreamingAssetsAndroidPath");
        EditorUtility.DisplayDialog("提示", ArtStreamingAssetsAndroidPath, "OK");
    }


    [MenuItem("Window/CompileArtRes/设置 Window Art 路径")]
    public static void SettingWindowArtPath()
    {//设置Window Art路径
        string selectPath = EditorUtility.SaveFolderPanel("源目录", "", "");
        Debug.Log("selectPath: " + selectPath);
        if (!selectPath.EndsWith("Mud_Art"))
        {
            EditorUtility.DisplayDialog("提示", "请选择Android Art工程!", "OK");
            return;
        }

        Debug.Log("selectPath: " + selectPath);
        //保存
        ArtStreamingAssetsWindowPath = selectPath;
        EditorPrefs.SetString("ArtStreamingAssetsWindowPath", ArtStreamingAssetsWindowPath);
    }

    [MenuItem("Window/CompileArtRes/查看 Window Art 路径")]
    public static void LookWindowArtPath()
    {//查看 Window Art 路径
        ArtStreamingAssetsWindowPath = EditorPrefs.GetString("ArtStreamingAssetsWindowPath");
        EditorUtility.DisplayDialog("提示", ArtStreamingAssetsWindowPath, "OK");
    }

    [MenuItem("Window/CompileArtRes/FromWindowArt")]
    public static void CompileArtResFromWindowArt()
    {
        SysProgressBar.ShowProgressBar(0, 100, "创建打包MudArt脚本...");

        if (string.IsNullOrEmpty(ArtStreamingAssetsWindowPath))
        {
            ArtStreamingAssetsWindowPath = EditorPrefs.GetString("ArtStreamingAssetsWindowPath");
        }

        bool isSuccess = true;
        if (!File.Exists(CallArtWindowPath))
        {//创建批处理
            string batContent = SetArtBatContent(ArtStreamingAssetsWindowPath);
            if (string.IsNullOrEmpty(batContent))
            {
                isSuccess = false;
                return;
            }

            isSuccess = BatTool.GenerateBat(CallArtWindowPath, SetArtBatContent(ArtStreamingAssetsWindowPath));

        }

        Debug.Log("isSuccess: " + isSuccess);
        if (!isSuccess)
        {
            Debug.Log("批处理创建失败...");
            SysProgressBar.ShowProgressBar(100, 100, "任务完成...");
            return;
        }

        SysProgressBar.ShowProgressBar(30, 100, "正在打包MUDArt...");
        BatTool.CallBat(CallArtWindowPath);

        SysProgressBar.ShowProgressBar(60, 100, "复制资源...");
        ArtStreamingAssetsWindowPath = EditorPrefs.GetString("ArtStreamingAssetsWindowPath");
        //从Art工程Streaming复制到MudGame
        FileHelper.CopyFolder(ArtStreamingAssetsWindowPath + "/Assets/StreamingAssets", MudGameStreamingAssetsPath);

        SysProgressBar.ShowProgressBar(100, 100, "复制完成...");
    }

    [MenuItem("Window/CompileArtRes/FromAndroidArt")]
    public static void CompileArtResFromAndroidArt()
    {
        SysProgressBar.ShowProgressBar(0, 100, "创建打包MudArt脚本...");

        if (string.IsNullOrEmpty(ArtStreamingAssetsAndroidPath))
        {
            ArtStreamingAssetsAndroidPath = EditorPrefs.GetString("ArtStreamingAssetsAndroidPath");
        }

        bool isSuccess = true;
        if (!File.Exists(CallArtAndroidPath))
        {//创建批处理
            string batContent = SetArtBatContent(ArtStreamingAssetsAndroidPath);
            if (string.IsNullOrEmpty(batContent))
            {
                isSuccess = false;
                return;
            }

            isSuccess = BatTool.GenerateBat(CallArtAndroidPath, SetArtBatContent(ArtStreamingAssetsAndroidPath));
            
        }
        
        Debug.Log("isSuccess: " + isSuccess);
        if (!isSuccess)
        {
            Debug.Log("批处理创建失败...");
            SysProgressBar.ShowProgressBar(100, 100, "任务完成...");
            return;
        }

        SysProgressBar.ShowProgressBar(30, 100, "正在打包MUDArt...");
        BatTool.CallBat(CallArtAndroidPath);

        SysProgressBar.ShowProgressBar(60, 100, "复制资源...");
        ArtStreamingAssetsAndroidPath = EditorPrefs.GetString("ArtStreamingAssetsAndroidPath");
        //从Art工程Streaming复制到MudGame
        FileHelper.CopyFolder(ArtStreamingAssetsAndroidPath + "/Assets/StreamingAssets", MudGameStreamingAssetsPath);

        SysProgressBar.ShowProgressBar(100, 100, "复制完成...");
    }

    [MenuItem("Window/CompileArtRes/直接复制MudArt(Android)工程(不打包)")]
    public static void CopyArtToMudGameAndroid()
    {
        SysProgressBar.ShowProgressBar(50, 100, "正在复制...");
        string artPath = EditorPrefs.GetString("ArtStreamingAssetsAndroidPath");
        FileHelper.CopyFolder(artPath + "/Assets/StreamingAssets", MudGameStreamingAssetsPath);
        SysProgressBar.ShowProgressBar(100, 100, "复制完成...");

    }

    [MenuItem("Window/CompileArtRes/直接复制MudArt(Window)工程(不打包)")]
    public static void CopyArtToMudGameWindow()
    {
        SysProgressBar.ShowProgressBar(50, 100, "正在复制...");
        string artPath = EditorPrefs.GetString("ArtStreamingAssetsWindowPath");
        FileHelper.CopyFolder(artPath + "/Assets/StreamingAssets", MudGameStreamingAssetsPath);
        SysProgressBar.ShowProgressBar(100, 100, "复制完成...");

    }

    /// <summary>
    /// 设置批处理脚本内容
    /// </summary>
    /// <returns></returns>
    public static string SetArtBatContent(string mudArtPath)
    {
        
        if (string.IsNullOrEmpty(UnityEXEPath))
        {
            UnityEXEPath = EditorPrefs.GetString("UnityEXEPath");
        }

        if (string.IsNullOrEmpty(UnityEXEPath))
        {
            UnityEngine.Debug.Log("Unity.exe Path not set!");
            EditorUtility.DisplayDialog("提示", "Unity.exe Path not set!", "OK");
            return null;
        }

        if (string.IsNullOrEmpty(mudArtPath))
        {
            mudArtPath = EditorPrefs.GetString("ArtStreamingAssetsAndroidPath");
        }

        if (string.IsNullOrEmpty(mudArtPath))
        {
            UnityEngine.Debug.Log("MudArt Path not set!");
            EditorUtility.DisplayDialog("提示", "MudArt Path not set!", "OK");
            return null;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("@echo off \n");
        sb.Append("\n");
        sb.Append("set UnityEXEPath=\""+ UnityEXEPath + "\"");
        sb.Append("\n");
        sb.Append("set ArtUnityProj=\""+ mudArtPath + "\"");
        sb.Append("\n");
        sb.Append("set logPath=\""+ mudArtPath + "\\buildAllGameRes.log\"");
        sb.Append("\n");
        sb.Append("%UnityEXEPath% -quit -batchmode -logFile %logPath% -projectPath %ArtUnityProj% -executeMethod GR_Common.BuildAll5");

        return sb.ToString();
    }
}
