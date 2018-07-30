using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

class BatTool
{
    public static string logFile = System.IO.Directory.GetCurrentDirectory()+"/packageAndroid.log";
    public static StringBuilder returnMsg = new StringBuilder();

    /// <summary>
    /// 调用批处理
    /// </summary>
    /// <param name="batPath">Bat路径</param>
    public static void CallBat(string batPath)
    {
        string outPutString = string.Empty;
        string errMsg = string.Empty;
        bool resultFlag = false;
        returnMsg = new StringBuilder();

        Thread t1 = new Thread(new ParameterizedThreadStart(ReadOutput));
        t1.IsBackground = true;
        Thread t2 = new Thread(new ParameterizedThreadStart(ReadError));
        t1.IsBackground = true;

        using (Process pro = new Process())
        {
            FileInfo file = new FileInfo(batPath);
            pro.StartInfo.WorkingDirectory = file.Directory.FullName;
            pro.StartInfo.FileName = batPath;
            pro.StartInfo.CreateNoWindow = true;
            pro.StartInfo.RedirectStandardOutput = true;
            pro.StartInfo.RedirectStandardError = true;
            pro.StartInfo.UseShellExecute = false;

            pro.Start();
            t1.Start(pro);
            t2.Start(pro);
            pro.WaitForExit();

            //逐行读取标准输出
            while ((outPutString = pro.StandardOutput.ReadLine()) != null)
            {
                //+ Environment.NewLine
                returnMsg.Append(outPutString);
                resultFlag = true;
            }

            if (!resultFlag)
            {
                returnMsg.Append(pro.StandardError.ReadToEnd());
            }

            pro.Close();
            //string str = get_ansi(returnMsg.ToString());

            FileHelper.WriteToFile(logFile, returnMsg.ToString());
        }
    }

    /// <summary>
    /// 读取错误输出流
    /// </summary>
    /// <param name="data"></param>
    public static void ReadError(object data)
    {
        Process temp = (Process)data;
        if (temp == null)
        {
            return;
        }

        string outputStr = string.Empty;
        while ((outputStr = temp.StandardError.ReadLine()) != null)
        {
            returnMsg.Append(outputStr);
        }
    }
    
    /// <summary>
    /// 读取标准输出流
    /// </summary>
    /// <param name="data"></param>
    public static void ReadOutput(object data)
    {
        Process temp = (Process)data;
        if (temp == null)
        {
            return;
        }

        string outputStr = string.Empty;
        while ((outputStr = temp.StandardOutput.ReadLine()) != null)
        {
            returnMsg.Append(outputStr);
        }
    }

    /// <summary>
    /// 创建批处理
    /// </summary>
    /// <param name="bat">批处理内容</param>
    /// <returns></returns>
    public static bool GenerateBat(string batPath,string bat)
    {
        //if (!File.Exists(batPath) && File.Create(batPath) != null)
        //{
        //    UnityEngine.Debug.Log("Create CallArtBat.bat Success....");
        //}
        //else if(File.Exists(batPath))
        //{
        //    UnityEngine.Debug.Log("Bat exist...");
        //    return true;
        //}

        string resultMsg = FileHelper.WriteToFile(batPath, bat);

        if (!string.IsNullOrEmpty(resultMsg))
        {
            UnityEngine.Debug.Log(resultMsg);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 转换字符串编码UTF8
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string get_uft8(string str)
    {
        UTF8Encoding utf8 = new UTF8Encoding();
        Byte[] encodedBytes = utf8.GetBytes(str);
        String decodedString = utf8.GetString(encodedBytes);
        return decodedString;
    }

    /// <summary>
    /// 转换字符串编码ASCII
    /// </summary>
    /// <param name="str">ascii</param>
    /// <returns></returns>
    public static string get_ascii(string str)
    {
        byte[] Buff = System.Text.Encoding.ASCII.GetBytes(str);
        string retStr = System.Text.Encoding.Default.GetString(Buff, 0, Buff.Length);

        return retStr;
    }
}
