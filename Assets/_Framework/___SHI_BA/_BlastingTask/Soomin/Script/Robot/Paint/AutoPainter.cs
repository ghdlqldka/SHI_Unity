using PaintIn3D;
using UnityEngine;
using System.Collections;
using System;
public class AutoPainter : MonoBehaviour
{
    [Header("페인팅 분사 파티클")]
    public GameObject sprayParticles;

    [Header("페인팅 분사 스크립트")]
    private CwPaintSphere paintSphere;

    [Header("타겟")]
    public Transform targetTrans; // 분사위치
    public float paintInterval = 0.0001f; // 페인트 간격

    [Header("싱글톤 캐싱")]
    private SystemController robotController; // 로봇 시스템 컨트롤러

    private Coroutine paintCoroutine; //코루틴 저장용

    private void Start()
    {
        robotController = SystemController.Instance;
        paintSphere = gameObject.GetComponent<CwPaintSphere>();

    }
    /// <summary>
    /// 분사 시작
    /// </summary>
    public void ShootPaint(bool isbool)
    {
        sprayParticles.SetActive(isbool);
    }

    /// <summary>
    /// 분사 닿은곳 페인팅 처리 코루틴
    /// </summary>
    private IEnumerator PaintRoutine()
    {
        sprayParticles.SetActive(true);
        while (SystemController.Instance.currentState == RobotState.Working)
        {
            Vector3 origin = targetTrans.position;
            Vector3 direction = targetTrans.right;
            Ray ray = new Ray(origin, direction);

            Debug.DrawRay(origin, direction * 1f, Color.yellow, paintInterval);

            if (Physics.Raycast(ray, out RaycastHit hit, 1f))
            {
                Quaternion rotation = Quaternion.LookRotation(hit.normal);

                paintSphere.HandleHitPoint(
                    preview: false,
                    priority: 0,
                    pressure: 1.0f,
                    seed: int.MaxValue,
                    position: hit.point,
                    rotation: rotation
                );
            }

            // 분사 간격을 paintInterval 초로 늘림
            yield return null;
        }
        sprayParticles.SetActive(false);
    }
}
