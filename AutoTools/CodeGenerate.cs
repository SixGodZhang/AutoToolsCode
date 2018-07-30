using MobaGo.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CodeGenerate : Editor
{
    public static bool initFlag = false;
    public static Dictionary<string, string> predefineRules = new Dictionary<string, string>();
    public static string targetCSPath = new DirectoryInfo(Application.dataPath).Parent.FullName + "/targetCode.cs";
    public static string uiPanelTemplatePath = Application.dataPath + "/Editor/AutoTools/UIPanel.cs";
    public static string externEXEPath = @"D:\Sublime Text 3\sublime_text.exe";

    public enum PartCode
    {
        Define = 1<<1,//定义
        Path = 1<<2,//路径
        Component = 1<<3,//获取
        Method = 1<<4,//方法
        EventStatement = 1<<5,//事件
        RegisterEvent = 1<<6,//注册事件
        UnRegisterEvent = 1<<7,//声明事件
    }

    public enum UIComponentType
    {
        Button,//按钮
        GameObject,//GameObject
        UGUIImage2,//自定义图片
        Image,//UGUI自带图片
        Text,//UGUI文本组件
        SpriteNumber,//自定义数字组件
        GridLayoutGroup,//布局组件
        UGUIButton,//UGUI自带Button
        InputField,//UGUI自带输入文本
    }

    [MenuItem("GameObject/CodeGenerate/UIPanelTemplate", false, 2)]
    public static void UIPanel()
    {
        System.Diagnostics.Process.Start(externEXEPath, uiPanelTemplatePath);
    }

    [MenuItem("GameObject/CodeGenerate/Clean", false, 1)]
    public static void CleanUp()
    {
        if (File.Exists(targetCSPath))
        {
            File.Delete(targetCSPath);
        }

        predefineRules.Clear();

        Debug.Log("clean up the target csfile.....");
        //UnityEditor.EditorUtility.DisplayDialog("标题", "提示内容", "确认", "取消");
        //UnityEditor.EditorUtility.DisplayDialog("提示", "清除完成!", "确认", "取消");
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="menuCommand"></param>
    /// <param name="Bug">一次选择只能存在同一层级,不能包含父子</param>
    [MenuItem("GameObject/CodeGenerate/ByParentRoot", false, 0)]
    public static void CodeGenerateByRoot(MenuCommand menuCommand)
    {
        //限制方法仅执行一次
        if (Selection.objects.Length > 1)
        {
            if (menuCommand.context != Selection.objects[0] && !JudgeParentChildRelative(menuCommand.context as GameObject, Selection.objects[0] as GameObject))
            {
                return;
            }
        }

        if (!initFlag)
        {
            initFlag = true;
            CleanUp();
            InitPredefineCode();
        }

        GameObject[] selectObjects = Selection.gameObjects;
        Dictionary<string, string> allCode = new Dictionary<string, string>();
        StringBuilder pathCode = new StringBuilder();
        StringBuilder defineCode = new StringBuilder();
        StringBuilder componentCode = new StringBuilder();
        StringBuilder methodCode = new StringBuilder();
        StringBuilder eventStatementCode = new StringBuilder();
        StringBuilder eventRegisterCode = new StringBuilder();
        StringBuilder eventUnRegisterCode = new StringBuilder();

        foreach (var selectGo in selectObjects)
        {
            if (selectGo == null)
            {
                continue;
            }

            string selctGoName = selectGo.name;//需修改

            string selctGoPath = "";//当前选中Go的路径
            GetPath(selectGo, ref selctGoPath);

            //1.
            string key = JudgeGoType(selectGo);
            if (key == null)
            {
                Debug.Log("Not Found PredefineCode,the code not include or GameObject don't have component...");
                Debug.LogError("please check " + selectGo.name + "carefully!");
                continue;
            }

            Dictionary<string, string> code = GenerateRelativeCode(key, selctGoName, selctGoPath);
            foreach (var item in code.Keys)
            {
                allCode.Add(item, code[item]);
            }
        }

        //排序:整理代码
        foreach (var item in allCode.Keys)
        {
            Debug.Log(item);
            if (item.Contains("Path"))
            {
                pathCode.Append(allCode[item]);
            }
            else if (item.Contains("Define"))
            {
                defineCode.Append(allCode[item]);
            }
            else if (item.Contains("Component"))
            {
                componentCode.Append(allCode[item]);
            }
            else if (item.Contains(PartCode.EventStatement + ""))
            {
                eventStatementCode.Append(allCode[item]);
            }
            else if (item.Contains(PartCode.UnRegisterEvent + ""))
            {
                eventUnRegisterCode.Append(allCode[item]);
            }
            else if (item.Contains(PartCode.RegisterEvent + ""))
            {
                eventRegisterCode.Append(allCode[item]);
            }
            else if (item.Contains("Method"))
            {
                methodCode.Append(allCode[item]);
            }
        }

        Write2CSFile(targetCSPath, 
            "//UI对象路径"
            + "\n" + pathCode.ToString()
            + "\n" + "//UI对象定义"
            + "\n" + defineCode.ToString()
            + "\n" + "//UI对象初始化"
            + "\n" + componentCode.ToString()
            + "\n" + "//Button对象事件声明"
            + "\n" + eventStatementCode.ToString()
            + "\n" + "//Button对象事件注册"
            + "\n" + eventRegisterCode.ToString()
            + "\n" + "//Button对象事件取消注册"
            + "\n" + eventUnRegisterCode.ToString()
            + "\n" + "//方法"
            + "\n" + methodCode.ToString()

            );

        if (!string.IsNullOrEmpty(targetCSPath))
        {
            Debug.Log("Start.....");
            System.Diagnostics.Process.Start(externEXEPath, targetCSPath);
        }

        initFlag = false;
        //Debug.Log("final Code: "+ targetCode);
    }

    public static Dictionary<string, string> GenerateRelativeCode(string key, string selctGoName,string selctGoPath)
    {
        Dictionary<string, string> codeDict = new Dictionary<string, string>();
        switch (key)
        {
            case "ButtonDefine":
                {
                    codeDict = GetAppropriateCodeFromDic(UIComponentType.Button, PartCode.Define | PartCode.Path | PartCode.Component | PartCode.EventStatement | PartCode.RegisterEvent | PartCode.UnRegisterEvent, selctGoName,selctGoPath);
                }
                break;
            case "UGUIImage2Define":
                {
                    codeDict = GetAppropriateCodeFromDic(UIComponentType.UGUIImage2, PartCode.Define | PartCode.Path | PartCode.Component, selctGoName,selctGoPath);
                }
                break;
            case "GameObjectDefine":
                {
                    codeDict = GetAppropriateCodeFromDic(UIComponentType.GameObject, PartCode.Define | PartCode.Path | PartCode.Component, selctGoName, selctGoPath);
                }
                break;
            case "ImageDefine":
                {
                    codeDict = GetAppropriateCodeFromDic(UIComponentType.Image, PartCode.Define | PartCode.Path | PartCode.Component, selctGoName, selctGoPath);
                }
                break;
            case "TextDefine":
                {
                    codeDict = GetAppropriateCodeFromDic(UIComponentType.Text, PartCode.Define | PartCode.Path | PartCode.Component, selctGoName, selctGoPath);
                }
                break;
            case "SpriteNumberDefine":
                {
                    codeDict = GetAppropriateCodeFromDic(UIComponentType.SpriteNumber, PartCode.Define | PartCode.Path | PartCode.Component, selctGoName, selctGoPath);
                }
                break;
            case "GridLayoutGroupDefine":
                {
                    codeDict = GetAppropriateCodeFromDic(UIComponentType.GridLayoutGroup, PartCode.Define | PartCode.Path | PartCode.Component, selctGoName, selctGoPath);
                }
                break;
            case "UGUIButtonDefine":
                {
                    codeDict = GetAppropriateCodeFromDic(UIComponentType.UGUIButton, PartCode.Define | PartCode.Path | PartCode.Component, selctGoName, selctGoPath);
                }
                break;
            case "InputFieldDefine":
                {
                    codeDict = GetAppropriateCodeFromDic(UIComponentType.InputField, PartCode.Define | PartCode.Path | PartCode.Component, selctGoName, selctGoPath);
                }
                break;
            default:
                {
                    codeDict = GetAppropriateCodeFromDic(UIComponentType.GameObject, PartCode.Define | PartCode.Component, selctGoName, selctGoPath);
                }
                break;
        }

        return codeDict;
    }

    /// <summary>
    /// 从字典中获取合适的代码
    /// </summary>
    /// <param name="type">UI类型</param>
    /// <param name="part">获取代码片段</param>
    /// <param name="selctGoName">选择的GameObject名称</param>
    /// <returns></returns>
    public static Dictionary<string, string> GetAppropriateCodeFromDic(UIComponentType type, PartCode part, string selctGoName, string selectGoPath)
    {
        Dictionary<string, string> codeDict = new Dictionary<string, string>();

        string partCode;

        switch (part)
        {
            case PartCode.Define | PartCode.Path | PartCode.Component:
                {
                    //Path
                    predefineRules.TryGetValue(type + "" + PartCode.Path, out partCode);
                    codeDict.Add(selctGoName + PartCode.Path, partCode.Replace("XXXXX", selctGoName).Replace("PPPPP",selectGoPath));

                    //Define
                    predefineRules.TryGetValue(type + "" + PartCode.Define, out partCode);
                    codeDict.Add(selctGoName + PartCode.Define, partCode.Replace("XXXXX", selctGoName));

                    //Component
                    predefineRules.TryGetValue(type + "" + PartCode.Component, out partCode);
                    codeDict.Add(selctGoName + PartCode.Component, partCode.Replace("XXXXX", selctGoName));
                }
                break;

            case PartCode.Define | PartCode.Path | PartCode.Component | PartCode.Method:
                {
                    predefineRules.TryGetValue(type + "" + PartCode.Define, out partCode);
                    codeDict.Add(selctGoName + PartCode.Define, partCode.Replace("XXXXX", selctGoName));

                    predefineRules.TryGetValue(type + "" + PartCode.Component, out partCode);
                    codeDict.Add(selctGoName + PartCode.Component, partCode.Replace("XXXXX", selctGoName));

                    predefineRules.TryGetValue(type + "" + PartCode.Method, out partCode);
                    codeDict.Add(selctGoName + PartCode.Method, partCode.Replace("XXXXX", selctGoName));
                }
                break;

            case PartCode.Path | PartCode.Define | PartCode.Component | PartCode.EventStatement | PartCode.RegisterEvent | PartCode.UnRegisterEvent:
                {
                    //Path
                    predefineRules.TryGetValue(type + "" + PartCode.Path, out partCode);
                    codeDict.Add(selctGoName + PartCode.Path, partCode.Replace("XXXXX", selctGoName).Replace("PPPPP", selectGoPath));

                    //Define
                    predefineRules.TryGetValue(type + "" + PartCode.Define, out partCode);
                    codeDict.Add(selctGoName + PartCode.Define, partCode.Replace("XXXXX", selctGoName));

                    //Component
                    predefineRules.TryGetValue(type + "" + PartCode.Component, out partCode);
                    codeDict.Add(selctGoName + PartCode.Component, partCode.Replace("XXXXX", selctGoName));

                    //EventStatement
                    predefineRules.TryGetValue(type + "" + PartCode.EventStatement, out partCode);
                    codeDict.Add(selctGoName + PartCode.EventStatement, partCode.Replace("XXXXX", selctGoName));

                    //RegisterEvent
                    predefineRules.TryGetValue(type + "" + PartCode.RegisterEvent, out partCode);
                    codeDict.Add(selctGoName + PartCode.RegisterEvent, partCode.Replace("XXXXX", selctGoName));

                    //UnRegisterEvent
                    predefineRules.TryGetValue(type + "" + PartCode.UnRegisterEvent, out partCode);
                    codeDict.Add(selctGoName + PartCode.UnRegisterEvent, partCode.Replace("XXXXX", selctGoName));
                }
                break;


        }

        return codeDict;
    }

    public static void InitPredefineCode()
    {
        //Button
        if (!predefineRules.ContainsKey("ButtonPath"))
        {
            predefineRules.Add("ButtonPath", "private const string s_XXXXX_Path = \"PPPPP\";\n");
        }
        if (!predefineRules.ContainsKey("ButtonDefine"))
        {
            predefineRules.Add("ButtonDefine", "private GameObject XXXXX;\n");
        }
        if (!predefineRules.ContainsKey("ButtonComponent"))
        {
            predefineRules.Add("ButtonComponent", "XXXXX = Root.Find(s_XXXXX_Path).gameObject;\n");
        }
        if (!predefineRules.ContainsKey("ButtonEventStatement"))
        {
            predefineRules.Add("ButtonEventStatement", "UGUIEventScript.Get(XXXXX).SetUIEvent(kEventType.Click, ButtonEventID);\n");
        }
        if (!predefineRules.ContainsKey("ButtonRegisterEvent"))
        {
            predefineRules.Add("ButtonRegisterEvent", "UGUIEventManager.instance.AddUIEventListener(ButtonEventID, OnButtonClickListener);\n");
        }
        if (!predefineRules.ContainsKey("ButtonUnRegisterEvent"))
        {
            predefineRules.Add("ButtonUnRegisterEvent", "UGUIEventManager.instance.RemoveUIEventListener(ButtonEventID, OnButtonClickListener);\n");
        }

        //GameObject
        if (!predefineRules.ContainsKey("GameObjectPath"))
        {
            predefineRules.Add("GameObjectPath", "private const string s_XXXXX_Path = \"PPPPP\";\n");
        }
        if (!predefineRules.ContainsKey("GameObjectDefine"))
        {
            predefineRules.Add("GameObjectDefine", "private GameObject XXXXX;\n");
        }
        if (!predefineRules.ContainsKey("GameObjectComponent"))
        {
            predefineRules.Add("GameObjectComponent", "XXXXX = Root.Find(s_XXXXX_Path).gameObject;\n");
        }

        //UGUIImage2
        if (!predefineRules.ContainsKey("UGUIImage2Path"))
        {
            predefineRules.Add("UGUIImage2Path", "private const string s_XXXXX_Path = \"PPPPP\";\n");
        }
        if (!predefineRules.ContainsKey("UGUIImage2Define"))
        {
            predefineRules.Add("UGUIImage2Define", "private UGUIImage2 XXXXX;\n");
        }
        if (!predefineRules.ContainsKey("UGUIImage2Component"))
        {
            predefineRules.Add("UGUIImage2Component", "XXXXX = Root.Find(s_XXXXX_Path).GetComponent<UGUIImage2>();\n");
        }

        //Image
        if (!predefineRules.ContainsKey("ImagePath"))
        {
            predefineRules.Add("ImagePath", "private const string s_XXXXX_Path = \"PPPPP\";\n");
        }
        if (!predefineRules.ContainsKey("ImageDefine"))
        {
            predefineRules.Add("ImageDefine", "private Image XXXXX;\n");
        }
        if (!predefineRules.ContainsKey("ImageComponent"))
        {
            predefineRules.Add("ImageComponent", "XXXXX = Root.Find(s_XXXXX_Path).GetComponent<Image>();\n");
        }

        //Text
        if (!predefineRules.ContainsKey("TextPath"))
        {
            predefineRules.Add("TextPath", "private const string s_XXXXX_Path = \"PPPPP\";\n");
        }
        if (!predefineRules.ContainsKey("TextDefine"))
        {
            predefineRules.Add("TextDefine", "private Text XXXXX;\n");
        }
        if (!predefineRules.ContainsKey("TextComponent"))
        {
            predefineRules.Add("TextComponent", "XXXXX = Root.Find(s_XXXXX_Path).GetComponent<Text>();\n");
        }

        //SpriteNumber
        if (!predefineRules.ContainsKey("SpriteNumberPath"))
        {
            predefineRules.Add("SpriteNumberPath", "private const string s_XXXXX_Path = \"PPPPP\";\n");
        }
        if (!predefineRules.ContainsKey("SpriteNumberDefine"))
        {
            predefineRules.Add("SpriteNumberDefine", "private SpriteNumber XXXXX;\n");
        }
        if (!predefineRules.ContainsKey("SpriteNumberComponent"))
        {
            predefineRules.Add("SpriteNumberComponent", "XXXXX = Root.Find(s_XXXXX_Path).GetComponent<SpriteNumber>();\n");
        }

        //GridLayoutGroup
        if (!predefineRules.ContainsKey("GridLayoutGroupPath"))
        {
            predefineRules.Add("GridLayoutGroupPath", "private const string s_XXXXX_Path = \"PPPPP\";\n");
        }
        if (!predefineRules.ContainsKey("GridLayoutGroupDefine"))
        {
            predefineRules.Add("GridLayoutGroupDefine", "private GridLayoutGroup XXXXX;\n");
        }
        if (!predefineRules.ContainsKey("GridLayoutGroupComponent"))
        {
            predefineRules.Add("GridLayoutGroupComponent", "XXXXX = Root.Find(s_XXXXX_Path).GetComponent<GridLayoutGroup>();\n");
        }

        //UGUIButton 
        if (!predefineRules.ContainsKey("UGUIButtonPath"))
        {
            predefineRules.Add("UGUIButtonPath", "private const string s_XXXXX_Path = \"PPPPP\";\n");
        }
        if (!predefineRules.ContainsKey("UGUIButtonDefine"))
        {
            predefineRules.Add("UGUIButtonDefine", "private Button XXXXX;\n");
        }
        if (!predefineRules.ContainsKey("UGUIButtonComponent"))
        {
            predefineRules.Add("UGUIButtonComponent", "XXXXX = Root.Find(s_XXXXX_Path).GetComponent<Button>();\n");
        }

        //InputField
        if (!predefineRules.ContainsKey("InputFieldPath"))
        {
            predefineRules.Add("InputFieldPath", "private const string s_XXXXX_Path = \"PPPPP\";\n");
        }
        if (!predefineRules.ContainsKey("InputFieldDefine"))
        {
            predefineRules.Add("InputFieldDefine", "private InputField XXXXX;\n");
        }
        if (!predefineRules.ContainsKey("InputFieldComponent"))
        {
            predefineRules.Add("InputFieldComponent", "XXXXX = Root.Find(s_XXXXX_Path).GetComponent<InputField>();\n");
        }


    }

    /// <summary>
    /// 判断GameObject的UI类型
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string JudgeGoType(GameObject source)
    {
        if (source.GetComponent<UGUIImage2>() != null)
        {
            Debug.Log("source type : UGUIImage2" + "...." + source.name);
            return "UGUIImage2" + "Define";
        }
        else if (source.GetComponent<Button>() != null)
        {
            Debug.Log("source type : UGUIButton" + "...." + source.name);
            return "UGUIButton" + "Define";
        }
        else if (source.GetComponent<Image>() != null && !source.name.Contains("Btn"))
        {
            Debug.Log("source type : Image" + "...." + source.name);
            return "Image" + "Define";
        }
        else if (source.GetComponent<UGUIRectEventDetector>() != null)
        {//按钮点击
            Debug.Log("source type : Button" + "...." + source.name);
            return "Button" + "Define";
        }
        else if (source.GetComponent<Text>() != null)
        {
            Debug.Log("source type : Text" + "...." + source.name);
            return "Text" + "Define";
        }
        else if (source.GetComponent<SpriteNumber>() != null)
        {
            Debug.Log("source type : SpriteNumber" + "...." + source.name);
            return "SpriteNumber" + "Define";
        }
        else if (source.GetComponent<GridLayoutGroup>() != null)
        {
            Debug.Log("source type : GridLayoutGroup" + "...." + source.name);
            return "GridLayoutGroup" + "Define";
        }
        else if (source.GetComponent<InputField>() != null)
        {
            Debug.Log("source type : InputField" + "...." + source.name);
            return "InputField" + "Define";
        }
        else
        {
            Debug.Log("source type : GameObject" + "...." + source.name);
            return "GameObject" + "Define";
        }
    }

    public static bool JudgeParentChildRelative(GameObject parent, GameObject child)
    {
        foreach (Transform item in parent.transform)
        {
            if (child.name.Equals(item.name))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 从文件中读取内容
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="content">将要写入的内容</param>
    /// <param name="isAppend">是否在文件中追加内容</param>
    private static string Read4File(string path)
    {
        StringBuilder targetStr = new StringBuilder();
        using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                targetStr.Append(line);
            }
        }

        return targetStr.ToString();
    }

    /// <summary>
    /// 将内容写入文件中
    /// </summary>
    /// <param name="path"></param>
    /// <param name="content"></param>
    /// <param name="isAppend"></param>
    private static void Write2CSFile(string path, string content, FileMode fileMode = FileMode.OpenOrCreate)
    {
        FileStream fs = null;
        StreamWriter sw = null;

        try
        {
            fs = new FileStream(path, fileMode);
            sw = new StreamWriter(fs);
            sw.Write(content);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        finally
        {
            sw.Flush();
            sw.Close();
            fs.Close();
        }
    }

    public static string GetPath(GameObject obj, ref string path)
    {
        if (obj == null) return path;

        if (obj.transform.parent == null) return path;

        if (!string.IsNullOrEmpty(path))
            path = obj.name + "/" + path;
        else
        {
            path = obj.name;
        }
        return GetPath(obj.transform.parent.gameObject, ref path);
    }
}