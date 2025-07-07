using PaintIn3D;
using UnityEngine;
using System.Collections;
using System;
public class AutoPainter : MonoBehaviour
{
    [Header("������ �л� ��ƼŬ")]
    public GameObject sprayParticles;

    [Header("������ �л� ��ũ��Ʈ")]
    private CwPaintSphere paintSphere;

    [Header("Ÿ��")]
    public Transform targetTrans; // �л���ġ
    public float paintInterval = 0.0001f; // ����Ʈ ����

    [Header("�̱��� ĳ��")]
    private SystemController robotController; // �κ� �ý��� ��Ʈ�ѷ�

    private Coroutine paintCoroutine; //�ڷ�ƾ �����

    private void Start()
    {
        robotController = SystemController.Instance;
        paintSphere = gameObject.GetComponent<CwPaintSphere>();

    }
    /// <summary>
    /// �л� ����
    /// </summary>
    public void ShootPaint(bool isbool)
    {
        sprayParticles.SetActive(isbool);
    }

    /// <summary>
    /// �л� ������ ������ ó�� �ڷ�ƾ
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

            // �л� ������ paintInterval �ʷ� �ø�
            yield return null;
        }
        sprayParticles.SetActive(false);
    }
}
