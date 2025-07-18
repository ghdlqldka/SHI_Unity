﻿// http://www.unity3d-france.com/unity/phpBB3/viewtopic.php?f=24&t=5409 by artemisart

using System.Collections.Generic;
using UnityEngine;

namespace mgear.tools
{
    public class GLDebug : MonoBehaviour
    {
        public struct Line
        {
            public Vector3 start;
            public Vector3 end;
            public Color color;
            public float startTime;
            public float duration;

            public Line(Vector3 start, Vector3 end, Color color, float startTime, float duration)
            {
                this.start = start;
                this.end = end;
                this.color = color;
                this.startTime = startTime;
                this.duration = duration;
            }

            public bool DurationElapsed(bool drawLine)
            {
                if (drawLine)
                {
                    GL.Color(color);
                    GL.Vertex(start);
                    GL.Vertex(end);
                }
                return Time.time - startTime >= duration;
            }
        }

        protected static GLDebug instance;

        public Material matZOn;
        public Material matZOff;

        public KeyCode toggleKey;
        public bool displayLines = true;
#if UNITY_EDITOR
        public bool displayGizmos = true;
#endif
        //public ScreenRect rect = new ScreenRect (0, 0, 150, 20);

        protected List<Line> linesZOn;
        protected List<Line> linesZOff;
        //private float milliseconds;

        protected LineRenderer lineRenderer;

        [Header("SRP")]
        public bool useLineRenderer = false;
        public float lineWidth = 0.1f;
        public Material lineRendererMat;

        protected virtual void Awake()
        {
            Debug.Log("Awake(), this.gameObject : " + this.gameObject.name);
            if (instance)
            {
                DestroyImmediate(this);
                return;
            }
            instance = this;
            linesZOn = new List<Line>();
            linesZOff = new List<Line>();

            if (useLineRenderer)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                lineRenderer.material = lineRendererMat;
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;
                lineRenderer.positionCount = 2;
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey)) displayLines = !displayLines;

            if (!displayLines)
            {
                for (int i = linesZOn.Count - 1; i >= 0; i--)
                {
                    if (linesZOn[i].DurationElapsed(false)) linesZOn.RemoveAt(i);
                }
                for (int i = linesZOff.Count - 1; i >= 0; i--)
                {
                    if (linesZOff[i].DurationElapsed(false)) linesZOff.RemoveAt(i);
                }
            }

            // FIXME no need to draw in update, just draw when changed
            if (useLineRenderer)
            {
                RenderLineRenderers();
            }
        }


#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!displayGizmos || !Application.isPlaying)
                return;
            for (int i = 0; i < linesZOn.Count; i++)
            {
                Gizmos.color = linesZOn[i].color;
                Gizmos.DrawLine(linesZOn[i].start, linesZOn[i].end);
            }
            for (int i = 0; i < linesZOff.Count; i++)
            {
                Gizmos.color = linesZOff[i].color;
                Gizmos.DrawLine(linesZOff[i].start, linesZOff[i].end);
            }
        }
#endif

        void OnPostRender()
        {
            if (!displayLines || useLineRenderer) return;

            matZOn.SetPass(0);
            GL.Begin(GL.LINES);
            for (int i = linesZOn.Count - 1; i >= 0; i--)
            {
                if (linesZOn[i].DurationElapsed(true)) linesZOn.RemoveAt(i);
            }
            for (int i = linesZOff.Count - 1; i >= 0; i--)
            {
                if (linesZOff[i].DurationElapsed(true)) linesZOff.RemoveAt(i);
            }
            GL.End();

            matZOff.SetPass(0);
            GL.Begin(GL.LINES);
            for (int i = linesZOn.Count - 1; i >= 0; i--)
            {
                if (linesZOn[i].DurationElapsed(true)) linesZOn.RemoveAt(i);
            }
            for (int i = linesZOff.Count - 1; i >= 0; i--)
            {
                if (linesZOff[i].DurationElapsed(true)) linesZOff.RemoveAt(i);
            }
            GL.End();
        }

        void RenderLineRenderers()
        {
            for (int i = 0; i < linesZOn.Count; i++)
            {
                lineRenderer.SetPosition(0, linesZOn[i].start);
                lineRenderer.SetPosition(1, linesZOn[i].end);
            }

            // NOTE only z write version is drawn
            /*
            for (int i = 0; i < linesZOff.Count; i++)
            {
                lineRenderer.SetPosition(0, linesZOff[i].start);
                lineRenderer.SetPosition(1, linesZOff[i].end);
            }*/
        }

        private static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0, bool depthTest = false)
        {
            if (duration == 0 && instance.displayLines == false) return;
            if (start == end) return;

            if (depthTest == true)
            {
                // TODO get rid off Time.time, so can call from another thread
                instance.linesZOn.Add(new Line(start, end, color, Time.time, duration));
            }
            else
            {
                instance.linesZOff.Add(new Line(start, end, color, Time.time, duration));
            }
        }

        /// <summary>
        /// Draw a line from start to end with color for a duration of time and with or without depth testing.
        /// If duration is 0 then the line is rendered 1 frame.
        /// </summary>
        /// <param name="start">Point in world space where the line should start.</param>
        /// <param name="end">Point in world space where the line should end.</param>
        /// <param name="color">Color of the line.</param>
        /// <param name="duration">How long the line should be visible for.</param>
        /// <param name="depthTest">Should the line be obscured by objects closer to the camera ?</param>
        public static void DrawLine(Vector3 start, Vector3 end, Color? color = null, float duration = 0, bool depthTest = false)
        {
            DrawLine(start, end, color ?? Color.white, duration, depthTest);
        }

        /// <summary>
        /// Draw a line from start to start + dir with color for a duration of time and with or without depth testing.
        /// If duration is 0 then the ray is rendered 1 frame.
        /// </summary>
        /// <param name="start">Point in world space where the ray should start.</param>
        /// <param name="dir">Direction and length of the ray.</param>
        /// <param name="color">Color of the ray.</param>
        /// <param name="duration">How long the ray should be visible for.</param>
        /// <param name="depthTest">Should the ray be obscured by objects closer to the camera ?</param>
        public static void DrawRay(Vector3 start, Vector3 dir, Color? color = null, float duration = 0, bool depthTest = false)
        {
            if (dir == Vector3.zero)
                return;
            DrawLine(start, start + dir, color, duration, depthTest);
        }

        /// <summary>
        /// Draw an arrow from start to end with color for a duration of time and with or without depth testing.
        /// If duration is 0 then the arrow is rendered 1 frame.
        /// </summary>
        /// <param name="start">Point in world space where the arrow should start.</param>
        /// <param name="end">Point in world space where the arrow should end.</param>
        /// <param name="arrowHeadLength">Length of the 2 lines of the head.</param>
        /// <param name="arrowHeadAngle">Angle between the main line and each of the 2 smaller lines of the head.</param>
        /// <param name="color">Color of the arrow.</param>
        /// <param name="duration">How long the arrow should be visible for.</param>
        /// <param name="depthTest">Should the arrow be obscured by objects closer to the camera ?</param>
        public static void DrawLineArrow(Vector3 start, Vector3 end, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null, float duration = 0, bool depthTest = false)
        {
            DrawArrow(start, end - start, arrowHeadLength, arrowHeadAngle, color, duration, depthTest);
        }

        /// <summary>
        /// Draw an arrow from start to start + dir with color for a duration of time and with or without depth testing.
        /// If duration is 0 then the arrow is rendered 1 frame.
        /// </summary>
        /// <param name="start">Point in world space where the arrow should start.</param>
        /// <param name="dir">Direction and length of the arrow.</param>
        /// <param name="arrowHeadLength">Length of the 2 lines of the head.</param>
        /// <param name="arrowHeadAngle">Angle between the main line and each of the 2 smaller lines of the head.</param>
        /// <param name="color">Color of the arrow.</param>
        /// <param name="duration">How long the arrow should be visible for.</param>
        /// <param name="depthTest">Should the arrow be obscured by objects closer to the camera ?</param>
        public static void DrawArrow(Vector3 start, Vector3 dir, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, Color? color = null, float duration = 0, bool depthTest = false)
        {
            if (dir == Vector3.zero)
                return;
            DrawRay(start, dir, color, duration, depthTest);
            Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
            DrawRay(start + dir, right * arrowHeadLength, color, duration, depthTest);
            DrawRay(start + dir, left * arrowHeadLength, color, duration, depthTest);
        }

        /// <summary>
        /// Draw a square with color for a duration of time and with or without depth testing.
        /// If duration is 0 then the square is renderer 1 frame.
        /// </summary>
        /// <param name="pos">Center of the square in world space.</param>
        /// <param name="rot">Rotation of the square in euler angles in world space.</param>
        /// <param name="scale">Size of the square.</param>
        /// <param name="color">Color of the square.</param>
        /// <param name="duration">How long the square should be visible for.</param>
        /// <param name="depthTest">Should the square be obscured by objects closer to the camera ?</param>
        public static void DrawSquare(Vector3 pos, Vector3? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
        {
            DrawSquare(Matrix4x4.TRS(pos, Quaternion.Euler(rot ?? Vector3.zero), scale ?? Vector3.one), color, duration, depthTest);
        }
        /// <summary>
        /// Draw a square with color for a duration of time and with or without depth testing.
        /// If duration is 0 then the square is renderer 1 frame.
        /// </summary>
        /// <param name="pos">Center of the square in world space.</param>
        /// <param name="rot">Rotation of the square in world space.</param>
        /// <param name="scale">Size of the square.</param>
        /// <param name="color">Color of the square.</param>
        /// <param name="duration">How long the square should be visible for.</param>
        /// <param name="depthTest">Should the square be obscured by objects closer to the camera ?</param>
        public static void DrawSquare(Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
        {
            DrawSquare(Matrix4x4.TRS(pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color, duration, depthTest);
        }
        /// <summary>
        /// Draw a square with color for a duration of time and with or without depth testing.
        /// If duration is 0 then the square is renderer 1 frame.
        /// </summary>
        /// <param name="matrix">Transformation matrix which represent the square transform.</param>
        /// <param name="color">Color of the square.</param>
        /// <param name="duration">How long the square should be visible for.</param>
        /// <param name="depthTest">Should the square be obscured by objects closer to the camera ?</param>
        public static void DrawSquare(Matrix4x4 matrix, Color? color = null, float duration = 0, bool depthTest = false)
        {
            Vector3
                    p_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, 0, .5f)),
                    p_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, 0, -.5f)),
                    p_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, 0, -.5f)),
                    p_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, 0, .5f));

            DrawLine(p_1, p_2, color, duration, depthTest);
            DrawLine(p_2, p_3, color, duration, depthTest);
            DrawLine(p_3, p_4, color, duration, depthTest);
            DrawLine(p_4, p_1, color, duration, depthTest);
        }

        /// <summary>
        /// Draw a cube with color for a duration of time and with or without depth testing.
        /// If duration is 0 then the square is renderer 1 frame.
        /// </summary>
        /// <param name="pos">Center of the cube in world space.</param>
        /// <param name="rot">Rotation of the cube in euler angles in world space.</param>
        /// <param name="scale">Size of the cube.</param>
        /// <param name="color">Color of the cube.</param>
        /// <param name="duration">How long the cube should be visible for.</param>
        /// <param name="depthTest">Should the cube be obscured by objects closer to the camera ?</param>
        public static void DrawCube(Vector3 pos, Vector3? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
        {
            DrawCube(Matrix4x4.TRS(pos, Quaternion.Euler(rot ?? Vector3.zero), scale ?? Vector3.one), color, duration, depthTest);
        }
        /// <summary>
        /// Draw a cube with color for a duration of time and with or without depth testing.
        /// If duration is 0 then the square is renderer 1 frame.
        /// </summary>
        /// <param name="pos">Center of the cube in world space.</param>
        /// <param name="rot">Rotation of the cube in world space.</param>
        /// <param name="scale">Size of the cube.</param>
        /// <param name="color">Color of the cube.</param>
        /// <param name="duration">How long the cube should be visible for.</param>
        /// <param name="depthTest">Should the cube be obscured by objects closer to the camera ?</param>
        public static void DrawCube(Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0, bool depthTest = false)
        {
            DrawCube(Matrix4x4.TRS(pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color, duration, depthTest);
        }
        /// <summary>
        /// Draw a cube with color for a duration of time and with or without depth testing.
        /// If duration is 0 then the square is renderer 1 frame.
        /// </summary>
        /// <param name="matrix">Transformation matrix which represent the cube transform.</param>
        /// <param name="color">Color of the cube.</param>
        /// <param name="duration">How long the cube should be visible for.</param>
        /// <param name="depthTest">Should the cube be obscured by objects closer to the camera ?</param>
        public static void DrawCube(Matrix4x4 matrix, Color? color = null, float duration = 0, bool depthTest = false)
        {
            Vector3
                    down_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, -.5f, .5f)),
                    down_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, -.5f, -.5f)),
                    down_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, -.5f, -.5f)),
                    down_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, -.5f, .5f)),
                    up_1 = matrix.MultiplyPoint3x4(new Vector3(.5f, .5f, .5f)),
                    up_2 = matrix.MultiplyPoint3x4(new Vector3(.5f, .5f, -.5f)),
                    up_3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, .5f, -.5f)),
                    up_4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, .5f, .5f));

            DrawLine(down_1, down_2, color, duration, depthTest);
            DrawLine(down_2, down_3, color, duration, depthTest);
            DrawLine(down_3, down_4, color, duration, depthTest);
            DrawLine(down_4, down_1, color, duration, depthTest);

            DrawLine(down_1, up_1, color, duration, depthTest);
            DrawLine(down_2, up_2, color, duration, depthTest);
            DrawLine(down_3, up_3, color, duration, depthTest);
            DrawLine(down_4, up_4, color, duration, depthTest);

            DrawLine(up_1, up_2, color, duration, depthTest);
            DrawLine(up_2, up_3, color, duration, depthTest);
            DrawLine(up_3, up_4, color, duration, depthTest);
            DrawLine(up_4, up_1, color, duration, depthTest);
        }


        // EXTRAS
        public static void DrawCircle(Vector3 center, float radius, Color? color = null, float duration = 0, bool depthTest = false)
        {
            var resolution = 0.2f;
            for (float theta = 0.0f; theta < (2 * Mathf.PI); theta += resolution)
            {
                Vector3 ci = (new Vector3(Mathf.Cos(theta) * radius + center.x, Mathf.Sin(theta) * radius + center.y, center.z));
                DrawLine(ci, ci + new Vector3(0, 0.001f, 0), color, duration, depthTest);
            }
        }

    }
}
