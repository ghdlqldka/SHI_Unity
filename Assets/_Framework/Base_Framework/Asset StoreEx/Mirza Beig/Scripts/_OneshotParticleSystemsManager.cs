

using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;


namespace MirzaBeig
{
    public class _OneshotParticleSystemsManager : ParticleSystems.Demos.OneshotParticleSystemsManager
    {
        [Header("Camera")]
        [SerializeField]
        protected Camera _camera;

        public override void InstantiateParticlePrefab(Vector2 mousePosition, float maxDistance)
        {
            // base.InstantiateParticlePrefab(mousePosition, maxDistance);

            if (spawnedPrefabs != null)
            {
                if (disableSpawn == false)
                {
                    Vector3 position = mousePosition;

                    position.z = maxDistance;
                    Vector3 worldMousePosition = _camera.ScreenToWorldPoint(position);

                    Vector3 directionToWorldMouse = worldMousePosition - _camera.transform.position;

                    RaycastHit rayHit;

                    // Start the raycast a little bit ahead of the camera because the camera starts right where a cube's edge is
                    // and that causes the raycast to hit... spawning a prefab right at the camera position. It's fixed by moving the camera,
                    // or I can just add this forward to prevent it from happening at all.

                    Physics.Raycast(_camera.transform.position + _camera.transform.forward * 0.01f, directionToWorldMouse, out rayHit, maxDistance);

                    Vector3 spawnPosition;

                    if (rayHit.collider)
                    {
                        spawnPosition = rayHit.point;
                    }
                    else
                    {
                        spawnPosition = worldMousePosition;
                    }

                    ParticleSystem[] prefab = particlePrefabs[currentParticlePrefabIndex];
                    ParticleSystem newParticlePrefab = Instantiate(prefab[0], spawnPosition, prefab[0].transform.rotation);

                    newParticlePrefab.gameObject.SetActive(true);

                    // Parent to spawner.

                    newParticlePrefab.transform.parent = transform;

                    spawnedPrefabs.Add(newParticlePrefab.GetComponentsInChildren<ParticleSystem>());
                }
            }
        } // end-of-function
    }
}
