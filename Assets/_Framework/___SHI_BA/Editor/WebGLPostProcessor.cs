using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

public class WebGLPostProcessor : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {

        if (report.summary.platform == BuildTarget.WebGL)
        {
            string indexPath = Path.Combine(report.summary.outputPath, "index.html");
            if (File.Exists(indexPath))
            {
                string html = File.ReadAllText(indexPath);

                // ������ ��ũ��Ʈ �±�
                string insertScript =
                    "<script src=\"js/libs/socket.io.min.js\"></script>\n" +
                    "<script src=\"js/libs/socket.io.min.js.map\"></script>\n";

                // </head> �±� �տ� ���� (���ϴ� ��ġ�� �°� ���� ����)
                html = html.Replace("</head>", insertScript + "</head>");

                File.WriteAllText(indexPath, html);
                UnityEngine.Debug.Log("Socket.IO ��ũ��Ʈ �±װ� index.html�� �ڵ� ���ԵǾ����ϴ�.");
            }
            else
            {
                UnityEngine.Debug.LogWarning("index.html ������ ã�� �� �����ϴ�: " + indexPath);
            }
        }
    }
}