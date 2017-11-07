using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class FileManager
{
    public static FileManager m_inst;
    public static FileManager GetInstance()
    {
        if (m_inst == null)
            m_inst = new FileManager();
        return m_inst;
    }

    public void LoadFile(string path, string name, List<string> str_list)
    {
        StreamWriter sw;
        FileInfo t = new FileInfo(path + "//" + "Engine/Tools/SceneEditor/" + name);

        if (!t.Exists)
        {
            sw = t.CreateText();
        }
        else
        {
            sw = t.AppendText();
        }

        sw.WriteLine("" + str_list.Count);
        for (int i = 0; i < str_list.Count; i++)
            sw.WriteLine(str_list[i]);
        sw.Close();
        sw.Dispose();
    }

    public List<string> ReadFile(string path, string name)
    {
        StreamReader sr = null;
        sr = File.OpenText(path + "//" + "Engine/Tools/SceneEditor/" + name);
        if (sr == null) return null;

        string line;
        List<string> str_list = new List<string>();
        while ((line = sr.ReadLine()) != null)
        {
            str_list.Add(line);
        }
        sr.Close();
        sr.Dispose();

        return str_list;
    }

    public void DeleteFile(string path, string name)
    {
        File.Delete(path + "//" + "Engine/Tools/SceneEditor/" + name);
    }
}
