using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

class FileHelper
{
    /// <summary>
    /// 写内容到文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string WriteToFile(string filePath,string content)
    {
        string result = null;
        try
        {
            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(content);
            sw.Close();
        }
        catch (Exception ex)
        {
            result = ex.Message;
            return result;
        }

        return result;
    }

    /// <summary>
    /// 复制文件
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="destPath"></param>
    public static void CopyFile(string sourcePath, string destPath)
    {
        bool isrewrite = true;
        System.IO.File.Copy(sourcePath, destPath, isrewrite);
    }

    /// <summary>
    /// 复制文件夹及其子目录、文件到目标文件夹
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="destPath"></param>
    public static void CopyFolder(string sourcePath, string destPath)
    {
        if (Directory.Exists(sourcePath))
        {
            if (!Directory.Exists(destPath))
            {
                //目标目录不存在则创建
                try
                {
                    Directory.CreateDirectory(destPath);
                }
                catch (Exception ex)
                {
                    throw new Exception("create target folder fail...：" + ex.Message);
                }
            }

            List<string> files = new List<string>(Directory.GetFiles(sourcePath));
            files.ForEach(c =>
            {
                string destFile = Path.Combine(destPath, Path.GetFileName(c));
                File.Copy(c, destFile, true);
            });

            List<string> folders = new List<string>(Directory.GetDirectories(sourcePath));
            folders.ForEach(c =>
            {
                string destDir = Path.Combine(destPath, Path.GetFileName(c));
                CopyFolder(c, destDir);
            });
        }
        else
        {
            throw new DirectoryNotFoundException("sourcePath: " + "source folder not found！");
        }
    }
}
