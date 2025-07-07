using UnityEngine;
using System;
using System.Linq;
using WorldSpaceTransitions.Examples;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WorldSpaceTransitions
{
    [ExecuteInEditMode]

    public class RectGizmo : MonoBehaviour
    {

        private float _b = 0.1f;
        private float _w = 1.0f;
        private float _h = 1.0f;
        private Planar_xyzClippingSection.ConstrainedAxis _axis = Planar_xyzClippingSection.ConstrainedAxis.X;
        public Planar_xyzClippingSection.ConstrainedAxis Axis
        {
            get
            {
                return _axis;
            }
            set
            {
                _axis = value;
                CreateSlicedMesh();
            }
        }


        private float _m = 0.4f;
        public float Margin
        {
            get
            {
                return _m;
            }
            set
            {
                _m = value;
                CreateSlicedMesh();
            }
        }

        bool border = true;
        public Material borderMaterial;
        //public bool fill = true;
        public Material fillMaterial;
        #if UNITY_EDITOR
        void OnValidate()
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            mr.hideFlags = HideFlags.HideInInspector;
            EditorUtility.SetDirty(this);
        }
        #endif
        public void SetSizedGizmo(Vector3 size, Planar_xyzClippingSection.ConstrainedAxis axis, bool drawBorder)
        {
            border = drawBorder || fillMaterial == null;
            MeshRenderer mr = GetComponent<MeshRenderer>();
            //mr.enabled = drawBorder || fillMaterial;
            float a0 = 1.0f; //gizmo proportions 
            float a1 = 0.02f; //border proportions 
            _axis = axis;
            switch (_axis)
            {
                case Planar_xyzClippingSection.ConstrainedAxis.X:
                    _w = a0 * size.y; _h = a0 * size.z;
                    break;
                case Planar_xyzClippingSection.ConstrainedAxis.Y:
                    _w = a0 * size.x; _h = a0 * size.z;
                    break;
                case Planar_xyzClippingSection.ConstrainedAxis.Z:
                    _w = a0 * size.x; _h = a0 * size.y;
                    break;
            }
            _b = a1 * (_w + _h);
            CreateSlicedMesh();
        }

        void CreateSlicedMesh()
        {
            Mesh mesh = new Mesh();
            MeshFilter mf = GetComponent<MeshFilter>();
            if(mf) mf.mesh = mesh;

            MeshCollider mc = GetComponent<MeshCollider>();
            if (mc) Destroy(mc);
            mesh.subMeshCount = 2;
            Vector3[] borderVert = new Vector3[0];
            Vector3[] fillVert = new Vector3[0];

            switch (_axis)
            {
                case Planar_xyzClippingSection.ConstrainedAxis.X:
                    if (border) borderVert = new Vector3[] {
                    new Vector3(0,-_w/2 - _b, -_h/2 - _b), new Vector3(0,-_w/2, -_h/2 - _b), new Vector3(0,_w/2,-_h/2 - _b), new Vector3(0,_w/2 + _b,-_h/2 - _b),
                    new Vector3(0,-_w/2 - _b, -_h/2), new Vector3(0,-_w/2, -_h/2), new Vector3(0,_w/2, -_h/2), new Vector3(0,_w/2 +_b, -_h/2),
                    new Vector3(0,-_w/2 -_b, _h/2), new Vector3(0,-_w/2, _h/2), new Vector3(0, _w/2, _h/2), new Vector3(0, _w/2 +_b, _h/2),
                    new Vector3(0,-_w/2 - _b, _h/2 + _b), new Vector3(0,-_w/2, _h/2 + _b), new Vector3(0,_w/2, _h/2 + _b), new Vector3(0,_w/2 +_b, _h/2 +_b)
                    };
                    if (fillMaterial) fillVert = new Vector3[] {
                        new Vector3(0,-_w/2, -_h/2), new Vector3(0,_w/2,-_h/2), new Vector3(0,-_w/2, _h/2), new Vector3(0,_w/2,_h/2 )
                    };
                    break;
                case Planar_xyzClippingSection.ConstrainedAxis.Y:
                    if (border) borderVert = new Vector3[] {
                    new Vector3(-_w/2 - _b, 0, -_h/2 - _b), new Vector3(-_w/2, 0, -_h/2 - _b), new Vector3(_w/2, 0, -_h/2 - _b), new Vector3(_w/2 + _b, 0, -_h/2 - _b),
                    new Vector3(-_w/2 - _b, 0, -_h/2), new Vector3(-_w/2,  0, -_h/2), new Vector3(_w/2 ,  0, -_h/2), new Vector3(_w/2 +_b,  0, -_h/2),
                    new Vector3(-_w/2 -_b, 0, _h/2), new Vector3(-_w/2, 0, _h/2), new Vector3(_w/2, 0, _h/2), new Vector3(_w/2 +_b, 0, _h/2),
                    new Vector3(-_w/2 - _b, 0, _h/2 + _b), new Vector3(-_w/2, 0, _h/2 + _b), new Vector3(_w/2, 0, _h/2 + _b), new Vector3(_w/2 +_b, 0, _h/2 +_b)
                    };
                    if (fillMaterial) fillVert = new Vector3[] {
                        new Vector3(-_w/2,0,-_h/2), new Vector3(_w/2,0,-_h/2), new Vector3(-_w/2,0, _h/2), new Vector3(_w/2,0,_h/2 )
                    };
                    break;
                case Planar_xyzClippingSection.ConstrainedAxis.Z:
                    if (border) borderVert = new Vector3[] {
                    new Vector3(-_w/2 - _b, -_h/2 - _b, 0), new Vector3(-_w/2, -_h/2 - _b, 0), new Vector3(_w/2, -_h/2 - _b, 0), new Vector3(_w/2 + _b, -_h/2 - _b, 0),
                    new Vector3(-_w/2 - _b, -_h/2, 0), new Vector3(-_w/2, -_h/2, 0), new Vector3(_w/2, -_h/2, 0), new Vector3(_w/2 +_b, -_h/2, 0),
                    new Vector3(-_w/2 -_b, _h/2, 0), new Vector3(-_w/2, _h/2, 0), new Vector3(_w/2, _h/2, 0), new Vector3(_w/2 +_b, _h/2, 0),
                    new Vector3(-_w/2 - _b, _h/2 + _b, 0), new Vector3(-_w/2, _h/2 + _b, 0), new Vector3(_w/2, _h/2 + _b, 0), new Vector3(_w/2 +_b, _h/2 +_b, 0)
                    };
                    if (fillMaterial) fillVert = new Vector3[] {
                    new Vector3(-_w/2, -_h/2,0), new Vector3(_w/2,-_h/2,0), new Vector3(-_w/2, _h/2,0), new Vector3(_w/2,_h/2,0 )
                    };
                    break;
            }
            mesh.vertices = borderVert.Concat(fillVert).ToArray();
            Vector2[] borderUV = new Vector2[0];
            Vector2[] fillUV = new Vector2[0];
            if (border) borderUV = new Vector2[] {
            new Vector2(0, 0), new Vector2(_m, 0), new Vector2(1-_m, 0), new Vector2(1, 0),
            new Vector2(0, _m), new Vector2(_m, _m), new Vector2(1-_m, _m), new Vector2(1, _m),
            new Vector2(0, 1-_m), new Vector2(_m, 1-_m), new Vector2(1-_m, 1-_m), new Vector2(1, 1-_m),
            new Vector2(0, 1), new Vector2(_m, 1), new Vector2(1-_m, 1), new Vector2(1, 1)
            };
            if (fillMaterial) fillUV = new Vector2[] {
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1)
            };
            mesh.uv = borderUV.Concat(fillUV).ToArray();

            //mesh.vertices = borderVert.Concat(fillVert).ToArray();
            int[] borderTris = new int[0];
            if (border) borderTris = new int[]
            {
            0, 4, 5,
            0, 5, 1,
            1, 5, 6,
            1, 6, 2,
            2, 6, 7,
            2, 7, 3,
            4, 8, 9,
            4, 9, 5, 
            //5, 9, 10,
            //5, 10, 6,
            6, 10, 11,
            6, 11, 7,
            8, 12, 13,
            8, 13, 9,
            9, 13, 14,
            9, 14, 10,
            10, 14, 15,
            10, 15, 11
            };
            int[] fillTris = new int[0];
            Debug.Log(borderVert.Length.ToString());
            if (fillMaterial) fillTris = new int[]
            {
            borderVert.Length,borderVert.Length +2,borderVert.Length +1,
            borderVert.Length +2,borderVert.Length +3,borderVert.Length +1
            };
            if(_axis!= Planar_xyzClippingSection.ConstrainedAxis.Y)
            {
                Array.Reverse(borderTris);
                Array.Reverse(fillTris);
            }


            if (border) mesh.SetTriangles(borderTris, 0);
            if (fillMaterial) mesh.SetTriangles(fillTris, 1);
            mesh.RecalculateNormals();
            //if (fill&&border) mesh.SetTriangles(fillTris, 1);
            Renderer renderer = GetComponent<Renderer>();
            Material[] materials = new Material[2];
            if (borderMaterial)
            {
                materials[0] = borderMaterial;
                if (fillMaterial)
                {
                    materials[1] = fillMaterial;
                }
                renderer.materials = materials;
            }
            gameObject.AddComponent<MeshCollider>();
        }
    }
}

