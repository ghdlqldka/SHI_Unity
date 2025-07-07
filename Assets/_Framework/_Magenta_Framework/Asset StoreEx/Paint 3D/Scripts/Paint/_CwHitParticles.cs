using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using PaintCore;

namespace PaintIn3D
{
	/// <summary>This component can be added to any ParticleSystem with collisions enabled, and it will fire hits when the particles collide with something.</summary>
	[RequireComponent(typeof(ParticleSystem))]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwHitParticles")]
	// [AddComponentMenu(PaintCore.CwCommon.ComponentHitMenuPrefix + "Hit Particles")]
	public class _CwHitParticles : CwHitParticles
    {
        private static string LOG_FORMAT = "<color=#88D5F6><b>[_CwHitParticles]</b></color> {0}";

        // protected ParticleSystem cachedParticleSystem;
        protected ParticleSystem _ParticleSystem
        {
            get
            {
                return cachedParticleSystem;
            }
        }

        // public Camera Camera { set { _camera = value; } get { return _camera; } }
        protected Camera _Camera { set { _camera = value; } get { return _camera; } }

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + 
                "</b>, pressureMode : <b>" + PressureMode + "</b>, emit : <b>" + Emit + "</b>, root : <b>" + Root + "</b>");

            cachedParticleSystemSet = false; // Not used!!!!!!
            cachedParticleSystem = this.GetComponent<ParticleSystem>();
            Debug.Assert(_ParticleSystem != null);

            hitCache = new _CwHitCache();
        }

        protected override void OnParticleCollision(GameObject hitGameObject)
        {
            /*
            if (cachedParticleSystemSet == false)
            {
                cachedParticleSystem = GetComponent<ParticleSystem>();
                cachedParticleSystemSet = true;
            }
            */

            // Get the collision events array
            int count = _ParticleSystem.GetSafeCollisionEventSize();

            // Expand collisionEvents list to fit all particles
            for (var i = particleCollisionEvents.Count; i < count; i++)
            {
                particleCollisionEvents.Add(new ParticleCollisionEvent());
            }

            count = _ParticleSystem.GetCollisionEvents(hitGameObject, particleCollisionEvents);

            // Calculate up vector ahead of time

            // GameObject finalRoot = root != null ? root : this.gameObject;
            GameObject finalRoot = this.gameObject;
            if (Root != null)
            {
                finalRoot = Root;
            }

            // Paint all locations
            for (int i = 0; i < count; i++)
            {
                ParticleCollisionEvent collisionEvent = particleCollisionEvents[i];

                if (CwHelper.IndexInMask(collisionEvent.colliderComponent.gameObject.layer, Layers) == false)
                {
                    continue;
                }

                if (Skip > 0)
                {
                    if (skipCounter++ > Skip)
                    {
                        skipCounter = 0;
                    }
                    else
                    {
                        continue;
                    }
                }

                float finalPressure = PressureMultiplier;

                CalculateFinalPressure(ref finalPressure, collisionEvent);
                /*
                switch (pressureMode)
                {
                    case PressureType.Constant:
                        {
                            finalPressure *= pressureConstant;
                        }
                        break;

                    case PressureType.Distance:
                        {
                            var distance = Vector3.Distance(this.transform.position, collisionEvent.intersection);

                            finalPressure *= Mathf.InverseLerp(pressureMin, pressureMax, distance);
                        }
                        break;

                    case PressureType.Speed:
                        {
                            var speed = Vector3.SqrMagnitude(collisionEvent.velocity);

                            if (speed > 0.0f)
                            {
                                speed = Mathf.Sqrt(speed);
                            }

                            finalPressure *= Mathf.InverseLerp(pressureMin, pressureMax, speed);
                        }
                        break;
                }
                */

                HandleHit(finalRoot, finalPressure, collisionEvent);
                
            }
        }

        protected void CalculateFinalPressure(ref float finalPressure, ParticleCollisionEvent collisionEvent)
        {
            switch (PressureMode)
            {
                case PressureType.Constant:
                    {
                        finalPressure *= PressureConstant;
                    }
                    break;

                case PressureType.Distance:
                    {
                        var distance = Vector3.Distance(this.transform.position, collisionEvent.intersection);

                        finalPressure *= Mathf.InverseLerp(PressureMin, PressureMax, distance);
                    }
                    break;

                case PressureType.Speed:
                    {
                        var speed = Vector3.SqrMagnitude(collisionEvent.velocity);

                        if (speed > 0.0f)
                        {
                            speed = Mathf.Sqrt(speed);
                        }

                        finalPressure *= Mathf.InverseLerp(PressureMin, PressureMax, speed);
                    }
                    break;
            }
        }

        protected void HandleHit(GameObject finalRoot, float finalPressure, ParticleCollisionEvent collisionEvent)
        {
            // var finalUp = orientation == OrientationType.CameraUp ? PaintCore.CwCommon.GetCameraUp(_camera) : Vector3.up;
            Vector3 finalUp = Vector3.up;
            if (Orientation == OrientationType.CameraUp)
            {
                finalUp = PaintCore.CwCommon.GetCameraUp(_Camera);
            }

            Vector3 finalPosition = collisionEvent.intersection + collisionEvent.normal * Offset;
            // Vector3 finalNormal = normal == NormalType.CollisionNormal ? collisionEvent.normal : -collisionEvent.velocity;
            Vector3 finalNormal = -collisionEvent.velocity;
            if (Normal == NormalType.CollisionNormal)
            {
                finalNormal = collisionEvent.normal;
            }
            // Quaternion finalRotation = finalNormal != Vector3.zero ? Quaternion.LookRotation(-finalNormal, finalUp) : Quaternion.identity;
            Quaternion finalRotation = Quaternion.identity;
            if (finalNormal != Vector3.zero)
            {
                finalRotation = Quaternion.LookRotation(-finalNormal, finalUp);
            }

            if (_ParticleSystem.collision.mode == ParticleSystemCollisionMode.Collision2D)
            {
                finalRotation = Quaternion.LookRotation(Vector3.forward, -finalNormal);
            }

            switch (Emit)
            {
                case EmitType.PointsIn3D:
                    {
                        ((_CwHitCache)hitCache).InvokePoint(finalRoot, Preview, Priority, finalPressure, finalPosition, finalRotation);
                    }
                    break;

                case EmitType.PointsOnUV:
                    {
                        var hit = default(RaycastHit);

                        if (TryGetRaycastHit(collisionEvent, ref hit) == true)
                        {
                            ((_CwHitCache)hitCache).InvokeCoord(finalRoot, Preview, Priority, finalPressure, new CwHit(hit), finalRotation);
                        }
                    }
                    break;

                case EmitType.TrianglesIn3D:
                    {
                        var hit = default(RaycastHit);

                        if (TryGetRaycastHit(collisionEvent, ref hit) == true)
                        {
                            ((_CwHitCache)hitCache).InvokeTriangle(this.gameObject, Preview, Priority, finalPressure, new CwHit(hit), finalRotation);
                        }
                    }
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }
    }
}
