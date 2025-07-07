using System;
using UnityEditor;
using UnityEngine;

namespace EA.Editor
{
    /// <summary>
    /// Editor Controls Abreviations.
    /// </summary>
    public static class ec
    {
        public static Color colorBG;
        public static Color colorFG;
        public static Color colorTxt;

        #region Standard Controls

        public static void hbar(int tickness = 4)
        {
            GUI.enabled = false;
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider, height(tickness));
            space(5);
            GUI.enabled = true;
        }

        public static void vbar(int tickness = 4)
        {
            GUI.enabled = false;
            EditorGUILayout.TextArea("", GUI.skin.verticalSlider, width(tickness));
            space(5);
            GUI.enabled = true;
        }

        public static void h(Action content, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(options);
            if (content != null) content.Invoke();
            GUILayout.EndHorizontal();
        }

        public static void h(Action content, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(style, options);
            if (content != null) content.Invoke();
            GUILayout.EndHorizontal();
        }

        public static void v(Action content, params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(options);
            if (content != null) content.Invoke();
            GUILayout.EndVertical();
        }

        public static void v(Action content, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(style, options);
            if (content != null) content.Invoke();
            GUILayout.EndVertical();
        }

        public static Vector2 scroll(Action content, Vector2 scroll, params GUILayoutOption[] options)
        {
            scroll = GUILayout.BeginScrollView(scroll, options);
            if (content != null) content.Invoke();
            GUILayout.EndScrollView();
            return scroll;
        }

        public static Vector2 scrollv(Action content, Vector2 scroll, bool alwaysShow = false, params GUILayoutOption[] options)
        {
            scroll = GUILayout.BeginScrollView(scroll, false, alwaysShow, options);
            if (content != null) content.Invoke();
            GUILayout.EndScrollView();
            return scroll;
        }

        public static Vector2 scrollh(Action content, Vector2 scroll, bool alwaysShow = false, params GUILayoutOption[] options)
        {
            scroll = GUILayout.BeginScrollView(scroll, alwaysShow, false, options);
            if (content != null) content.Invoke();
            GUILayout.EndScrollView();
            return scroll;
        }

        public static Vector2 scroll(Action content, Vector2 scroll, GUIStyle style, params GUILayoutOption[] options)
        {
            scroll = GUILayout.BeginScrollView(scroll, style, options);
            if (content != null) content.Invoke();
            GUILayout.EndScrollView();
            return scroll;
        }

        public static Vector2 scroll_hidden(Action content, Vector2 scroll, params GUILayoutOption[] options)
        {
            scroll = GUILayout.BeginScrollView(scroll, new GUIStyle(), new GUIStyle(), options);
            if (content != null) content.Invoke();
            GUILayout.EndScrollView();
            return scroll;
        }

        public static Vector2 scroll_hidden_h(Action content, Vector2 scroll, params GUILayoutOption[] options)
        {
            scroll = GUILayout.BeginScrollView(scroll, new GUIStyle(), GUI.skin.verticalScrollbar, options);
            if (content != null) content.Invoke();
            GUILayout.EndScrollView();
            return scroll;
        }

        public static Vector2 scroll_hidden_v(Action content, Vector2 scroll, params GUILayoutOption[] options)
        {
            scroll = GUILayout.BeginScrollView(scroll, GUI.skin.horizontalScrollbar, new GUIStyle(), options);
            if (content != null) content.Invoke();
            GUILayout.EndScrollView();
            return scroll;
        }

        public static void lb(string content, params GUILayoutOption[] options)
        {
            GUILayout.Label(content, options);
        }

        public static void lb(GUIContent content, params GUILayoutOption[] options)
        {
            GUILayout.Label(content, options);
        }

        public static void lb(string content, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Label(content, style, options);
        }

        public static string tf(string content, params GUILayoutOption[] options)
        {
            return EditorGUILayout.TextField(content, options);
        }

        public static string ta(string content, params GUILayoutOption[] options)
        {
            return EditorGUILayout.TextArea(content, options);
        }

        public static string ta(string content, GUIStyle style, params GUILayoutOption[] options)
        {
            return EditorGUILayout.TextArea(content, style, options);
        }

        public static int tf_int(string label, int value, params GUILayoutOption[] options)
        {
            return EditorGUILayout.IntField(label, value, options);
        }

        public static int tf_int(int value, params GUILayoutOption[] options)
        {
            return EditorGUILayout.IntField(value, options);
        }

        public static float tf_float(string label, float value, params GUILayoutOption[] options)
        {
            return EditorGUILayout.FloatField(label, value, options);
        }

        public static float tf_float(float value, params GUILayoutOption[] options)
        {
            return EditorGUILayout.FloatField(value, options);
        }

        public static bool bt(GUIContent content, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(content, options))
            {
                GUI.FocusControl(string.Empty);
                return true;
            }
            return false;
        }

        public static bool bt(string content, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(content, options))
            {
                GUI.FocusControl(string.Empty);
                return true;
            }
            return false;
        }

        public static bool bt(string content, GUIStyle style, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(content, style, options))
            {
                GUI.FocusControl(string.Empty);
                return true;
            }
            return false;
        }

        public static bool bt(Texture2D content, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(content, options))
            {
                GUI.FocusControl(string.Empty);
                return true;
            }
            return false;
        }

        public static bool bt(Texture2D content, GUIStyle style, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(content, style, options))
            {
                GUI.FocusControl(string.Empty);
                return true;
            }
            return false;
        }

        public static bool toggle(string label, bool val, params GUILayoutOption[] options)
        {
            return EditorGUILayout.Toggle(label, val, options);
        }

        public static bool toggle(bool val, params GUILayoutOption[] options)
        {
            return EditorGUILayout.Toggle(val, options);
        }

        public static int toolbar(int index, string[] values, params GUILayoutOption[] options)
        {
            int i = GUILayout.Toolbar(index, values, options);
            if (i != index) unfocus();
            return i;
        }

        public static int toolbar(int index, string[] values, GUIStyle style, params GUILayoutOption[] options)
        {
            return GUILayout.Toolbar(index, values, style, options);
        }

        public static int toolbar(int index, Texture[] values, params GUILayoutOption[] options)
        {
            return GUILayout.Toolbar(index, values, options);
        }

        public static int pop(int index, string[] values, params GUILayoutOption[] options)
        {
            return EditorGUILayout.Popup(index, values, options);
        }

        public static int popint(int index, string[] values, int[] ivalues, params GUILayoutOption[] options)
        {
            return EditorGUILayout.IntPopup(index, values, ivalues, options);
        }

        public static Enum popenum(string label, Enum val, params GUILayoutOption[] options)
        {
            return EditorGUILayout.EnumPopup(label, val, options);
        }

        public static Enum popenum(Enum val, params GUILayoutOption[] options)
        {
            return EditorGUILayout.EnumPopup(val, options);
        }

        public static Enum popenum_mask(Enum val, params GUILayoutOption[] options)
        {
            return EditorGUILayout.EnumFlagsField(val, options);
        }

        public static Color color(string label, Color c, params GUILayoutOption[] options)
        {
            return EditorGUILayout.ColorField(label, c, options);
        }

        public static Color color(Color c, params GUILayoutOption[] options)
        {
            return EditorGUILayout.ColorField(c, options);
        }

        public static void space(float pixels)
        {
            GUILayout.Space(pixels);
        }

        public static float sliderf(float value, float left, float right, params GUILayoutOption[] options)
        {
            return EditorGUILayout.Slider(value, left, right, options);
        }

        public static int slideri(int value, int left, int right, params GUILayoutOption[] options)
        {
            return EditorGUILayout.IntSlider(value, left, right, options);
        }

        public static void sliderfminmax(ref float valueMin, ref float valueMax, float limitMin, float limitMax, params GUILayoutOption[] options)
        {
            EditorGUILayout.MinMaxSlider(ref valueMin, ref valueMax, limitMin, limitMin, options);
        }

        public static void box(string content, params GUILayoutOption[] options)
        {
            GUILayout.Box(content, options);
        }

        public static void box(string content, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Box(content, style, options);
        }

        public static int mask(int value, string[] values, params GUILayoutOption[] options)
        {
            return EditorGUILayout.MaskField(value, values, options);
        }

        public static bool foldout(bool value, string label)
        {
            return EditorGUILayout.Foldout(value, label);
        }

        public static Vector2 vector2(string label, Vector2 v, params GUILayoutOption[] options)
        {
            return EditorGUILayout.Vector2Field(label, v, options);
        }

        public static Vector3 vector3(string label, Vector3 v, params GUILayoutOption[] options)
        {
            return EditorGUILayout.Vector3Field(label, v, options);
        }

        public static UnityEngine.Object objectfield(string label, UnityEngine.Object obj, Type type, bool allowSceneObjects, params GUILayoutOption[] options)
        {
            return EditorGUILayout.ObjectField(label, obj, type, allowSceneObjects, options);
        }

        public static UnityEngine.Object objectfield(UnityEngine.Object obj, Type type, bool allowSceneObjects, params GUILayoutOption[] options)
        {
            return EditorGUILayout.ObjectField(obj, type, allowSceneObjects, options);
        }

        public static int intslider(int value, int left, int right, params GUILayoutOption[] options)
        {
            return EditorGUILayout.IntSlider(value, left, right, options);
        }

        public static AnimationCurve curve(AnimationCurve ac, params GUILayoutOption[] options)
        {
            return EditorGUILayout.CurveField(ac, options);
        }

        #endregion

        #region Custom Controls

        static GUIStyle _styleProgressBar;
        static GUIStyle styleProgressBar
        {
            get
            {
                if (_styleProgressBar == null)
                {
                    _styleProgressBar = new GUIStyle(GUI.skin.box);
                    _styleProgressBar.border = new RectOffset(1, 1, 1, 1);
                }
                return _styleProgressBar;
            }
        }

        public static void progressbar(float value, string text = "", params GUILayoutOption[] options)
        {
            Color cl = GUI.color;
            GUI.color = Color.clear;
            GUILayout.Box("", options);

            value = Mathf.Clamp01(value);
            if (Event.current.type == EventType.Repaint)
            {
                Rect r = GUILayoutUtility.GetLastRect();
                GUI.color = colorBG;
                //GUI.Box(r, "", styleProgressBar);
                GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill);
                GUI.color = colorFG;
                float w = value * (r.width - 2);
                if ((int)w >= 2)
                {
                    //GUI.Box(new Rect(r.x + 1, r.y + 1, w, r.height - 2), "", styleProgressBar);
                    GUI.DrawTexture(new Rect(r.x + 1, r.y + 1, w, r.height - 2), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                }

                GUI.color = cl;
                if (!string.IsNullOrEmpty(text))
                {
                    TextAnchor ta = GUI.skin.label.alignment;
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(r, text);
                    GUI.skin.label.alignment = ta;
                }
            }
        }

        public static void image(Texture2D tex, ScaleMode mode, params GUILayoutOption[] options)
        {
            GUILayout.Box("", new GUIStyle(), options);
            if (Event.current.type == EventType.Repaint)
            {
                Rect r = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(r, tex, mode);
            }
        }

        public static void image(Texture2D tex, ScaleMode mode, string tooltip, params GUILayoutOption[] options)
        {
            GUILayout.Box(new GUIContent("", tooltip), new GUIStyle(), options);
            if (Event.current.type == EventType.Repaint)
            {
                Rect r = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(r, tex, mode);
            }
        }

        public static bool data_category(bool show, string title, Action content)
        {
            if (show)
                v(() =>
                {
                    h(() =>
                    {
                        show = toggle(show, width(16));
                        TextAnchor ta = GUI.skin.label.alignment;
                        FontStyle fs = GUI.skin.label.fontStyle;
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.skin.label.fontStyle = FontStyle.Bold;                        
                        lb(title);
                        GUI.skin.label.alignment = ta;
                        GUI.skin.label.fontStyle = fs;
                        space(20);
                    });
                    content?.Invoke();
                }, GUI.skin.box);
            else
                v(() =>
                {
                    h(() =>
                    {
                        show = toggle(show, width(16));
                        TextAnchor ta = GUI.skin.label.alignment;
                        FontStyle fs = GUI.skin.label.fontStyle;
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.skin.label.fontStyle = FontStyle.Bold;
                        lb(title);
                        GUI.skin.label.alignment = ta;
                        GUI.skin.label.fontStyle = fs;
                        space(20);
                    });
                }, GUI.skin.box, height(20));
            return show;
        }

        public static bool data_category_cpy<T>(bool show, string title, Action content, Func<T> get, Action<T> set)
        {
            if (show)
                v(() =>
                {
                    h(() =>
                    {
                        show = toggle(show, width(16));
                        space(30);
                        TextAnchor ta = GUI.skin.label.alignment;
                        FontStyle fs = GUI.skin.label.fontStyle;
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.skin.label.fontStyle = FontStyle.Bold;
                        lb(title);
                        GUI.skin.label.alignment = ta;
                        GUI.skin.label.fontStyle = fs;
                        clipboard<T>(get, set);
                    });
                    space(4);
                    if (content != null) content.Invoke();
                }, GUI.skin.box);
            else
                v(() =>
                {
                    h(() =>
                    {
                        show = toggle(show, width(16));
                        space(30);
                        TextAnchor ta = GUI.skin.label.alignment;
                        FontStyle fs = GUI.skin.label.fontStyle;
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        GUI.skin.label.fontStyle = FontStyle.Bold;
                        lb(title);
                        GUI.skin.label.alignment = ta;
                        GUI.skin.label.fontStyle = fs;
                        clipboard(get, set);
                    });
                }, GUI.skin.box, height(20));
            return show;
        }

        public static void data_subcategory(string title, Action content = null, params GUILayoutOption[] options)
        {
            v(() =>
            {
                h(() =>
                {
                    TextAnchor ta = GUI.skin.label.alignment;
                    FontStyle fs = GUI.skin.label.fontStyle;
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUI.skin.label.fontStyle = FontStyle.Bold;
                    lb(title);
                    GUI.skin.label.alignment = ta;
                    GUI.skin.label.fontStyle = fs;
                });
                space(4);
                if (content != null)
                {
                    content();
                    space(2);
                }
            }, GUI.skin.box, options);
        }

        public static void data_subcategory_cpy<T>(string title, Action content, Func<T> get, Action<T> set, Func<T, T> clone = null) where T : new()
        {
            v(() =>
            {
                h(() =>
                {
                    space(44);
                    TextAnchor ta = GUI.skin.label.alignment;
                    FontStyle fs = GUI.skin.label.fontStyle;
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUI.skin.label.fontStyle = FontStyle.Bold;
                    lb(title);
                    GUI.skin.label.alignment = ta;
                    GUI.skin.label.fontStyle = fs;
                    clipboard(get, set, clone);
                });
                if (content == null)
                    space(2);
                else content();
            }, GUI.skin.box);
        }

        #region Value Chooser

        public static void value_chooser(string val, Action<string> set, Action lookup)
        {
            //toolbar()
        }

        #endregion

        #region ClipBoard 

        static Texture2D[] clipboardTexs = null;
        static Type clipboardType = null;
        static object clipboardObjectT = null;

        public static void clipboard<T>(Func<T> get, Action<T> set, Func<T, T> clone = null)
        {
            T temp = get();

            bool pasted = false;
            if (clipboardTexs == null)
            {
                clipboardTexs = new Texture2D[]
                {
                    EditorGUIUtility.Load("arrow_black_down.png") as Texture2D,
                    EditorGUIUtility.Load("x_black.png") as Texture2D
                };
            }
            GUI.enabled = temp != null;
            h(() =>
            {
                if (clipboardType == null || !clipboardType.Equals(temp.GetType()))
                {
                    lb("", width(15));
                    if (bt(EditorGUIUtility.Load("arrow_black_up.png") as Texture2D, width(20), height(20)))//copy
                    {
                        clipboardType = typeof(T);
                        clipboardObjectT = clone == null ? temp.Clone() : clone(temp);
                    }
                }
                else
                {
                    int sel = toolbar(-1, clipboardTexs, width(40), height(19));
                    if (sel == 0)//paste
                    {
                        pasted = true;
                        temp = (T)clipboardObjectT;
                        clipboardType = null;
                    }
                    if (sel == 1)//cancel
                    {
                        clipboardType = null;
                    }
                }
            }, width(40), height(20));
            GUI.enabled = true;
            if (pasted) set(temp);
        }

        #endregion

        #endregion

        #region GUILayoutOptions

        public static GUILayoutOption width(float width)
        {
            return GUILayout.Width(width);
        }

        public static GUILayoutOption height(float height)
        {
            return GUILayout.Height(height);
        }

        public static GUILayoutOption expand_width(bool exp)
        {
            return GUILayout.ExpandWidth(exp);
        }

        public static GUILayoutOption expand_height(bool exp)
        {
            return GUILayout.ExpandHeight(exp);
        }

        public static GUILayoutOption min_height(float height)
        {
            return GUILayout.MinHeight(height);
        }

        public static GUILayoutOption min_width(float width)
        {
            return GUILayout.MinWidth(width);
        }

        public static GUILayoutOption max_height(float height)
        {
            return GUILayout.MaxHeight(height);
        }

        public static GUILayoutOption max_width(float width)
        {
            return GUILayout.MaxWidth(width);
        }

        #endregion

        #region Styles

        static GUIStyle _ta_style;
        public static GUIStyle ta_style
        {
            get
            {
                if (_ta_style == null)
                {
                    _ta_style = new GUIStyle(GUI.skin.textArea);
                    _ta_style.alignment = TextAnchor.UpperLeft;
                    _ta_style.wordWrap = true;
                }
                return _ta_style;
            }
        }

        #endregion

        public static T load<T>(string fname) where T : UnityEngine.Object
        {
            return EditorGUIUtility.Load(fname) as T;
        }

        #region Utils

        public static void unfocus()
        {
            GUI.FocusControl("");
        }

        #endregion

        public enum pbdir
        {
            LeftToRight,
            RightToLeft,
            TopToBottom,
            BottomToTop,
        }

        public static T Clone<T>(this T source)
        {
            throw new Exception("Clone Unimplemented! JSON parser missing.");
            //return JSON.Deserializer.Deserialize<T>(JSON.Serializer.Serialize(source));
        }
    }
}
