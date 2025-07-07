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

                // 삽입할 스크립트 태그
                string insertScript =
                    "<script src=\"js/libs/socket.io.min.js\"></script>\n" +
                    "<script src=\"js/libs/socket.io.min.js.map\"></script>\n";

                // </head> 태그 앞에 삽입 (원하는 위치에 맞게 조정 가능)
                html = html.Replace("</head>", insertScript + "</head>");

                File.WriteAllText(indexPath, html);
                UnityEngine.Debug.Log("Socket.IO 스크립트 태그가 index.html에 자동 삽입되었습니다.");
            }
            else
            {
                UnityEngine.Debug.LogWarning("index.html 파일을 찾을 수 없습니다: " + indexPath);
            }
        }
    }
}