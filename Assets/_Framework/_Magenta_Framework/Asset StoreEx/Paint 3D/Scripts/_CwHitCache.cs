using UnityEngine;
using PaintIn3D;

namespace PaintCore
{
	/// <summary>This class stores lists of IHit__ instances, allowing components like CwHit__ to easily invoke hit events.</summary>
	public class _CwHitCache : CwHitCache
    {
        public override void InvokePoint(GameObject obj, bool preview, int priority, float pressure, Vector3 position, Quaternion rotation)
        {
            // Debug.Log("InvokePoint(), cached : " + cached);

            if (cached == false)
            {
                Cache(obj);
            }

            int seed = Random.Range(int.MinValue, int.MaxValue);

            for (int i = 0; i < hitPoints.Count; i++)
            {
#if DEBUG
                Debug.Log("_CwHitCache => hitPoints.Count : " + hitPoints.Count + ", hitPoints[" + i + "] : " + hitPoints[i]);
#endif
                if (hitPoints[i] is _CwPaintSphere)
                {
                    ((_CwPaintSphere)hitPoints[i]).HandleHitPoint(preview, priority, pressure, seed, position, rotation);
                }
                else if (hitPoints[i] is _CwPaintDecal)
                {
                    ((_CwPaintSphere)hitPoints[i]).HandleHitPoint(preview, priority, pressure, seed, position, rotation);
                }
                else
                {
                    Debug.Log("111111111111111111111111111111111111111111111111111111111111");
                    hitPoints[i].HandleHitPoint(preview, priority, pressure, seed, position, rotation);
                }
            }
        }

        protected override void Cache(GameObject obj)
        {
            cached = true;

            obj.GetComponentsInChildren(hits);

            hitPoints.Clear();
            hitLines.Clear();
            hitTriangles.Clear();
            hitQuads.Clear();
            hitCoords.Clear();

            for (var i = 0; i < hits.Count; i++)
            {
                var hit = hits[i];

                var hitPoint = hit as IHitPoint; if (hitPoint != null) { hitPoints.Add(hitPoint); }

                var hitLine = hit as IHitLine; if (hitLine != null) { hitLines.Add(hitLine); }

                var hitTriangle = hit as IHitTriangle; if (hitTriangle != null) { hitTriangles.Add(hitTriangle); }

                var hitQuad = hit as IHitQuad; if (hitQuad != null) { hitQuads.Add(hitQuad); }

                var hitCoord = hit as IHitCoord; if (hitCoord != null) { hitCoords.Add(hitCoord); }
            }
        }
    }
}