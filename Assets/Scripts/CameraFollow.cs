using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private Vector3 offset;

    [SerializeField] private Camera cam;

    [Header("Zoom")]
    [SerializeField] private float defaultSize = 5f;
    [SerializeField] private float aimSize = 4f;
    [SerializeField] private float zoomSpeed = 8f;

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    public void SetAiming(bool aiming)
    {
        StopAllCoroutines();
        StartCoroutine(ZoomRoutine(aiming ? aimSize : defaultSize));
    }

    private System.Collections.IEnumerator ZoomRoutine(float targetSize)
    {
        while (Mathf.Abs(cam.orthographicSize - targetSize) > 0.01f)
        {
            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                targetSize,
                zoomSpeed * Time.deltaTime
            );

            yield return null;
        }

        cam.orthographicSize = targetSize;
        Debug.Log("yup");
    }   
}