using UnityEditor;
using UnityEngine;
using System.IO;

public class ProjectTreeExporter
{
    [MenuItem("Tools/Export Project Structure")]
    public static void Export()
    {
        string root = Application.dataPath;
        string outputPath = Path.Combine(Application.dataPath, "../project_structure.txt");

        using (StreamWriter writer = new StreamWriter(outputPath))
        {
            WriteDirectory(writer, root, "");
        }

        Debug.Log("Project structure exported to: " + outputPath);
    }

    private static void WriteDirectory(StreamWriter writer, string path, string indent)
    {
        string[] dirs = Directory.GetDirectories(path);
        string[] files = Directory.GetFiles(path);

        foreach (string dir in dirs)
        {
            writer.WriteLine(indent + "[D] " + Path.GetFileName(dir));
            WriteDirectory(writer, dir, indent + "   ");
        }

        foreach (string file in files)
        {
            if (!file.EndsWith(".meta"))
                writer.WriteLine(indent + "    " + Path.GetFileName(file));
        }
    }
}