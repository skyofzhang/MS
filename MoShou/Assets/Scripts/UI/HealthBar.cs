using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Transform target;
    public Vector3 offset = new Vector3(0, 2f, 0);
    
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (slider == null)
            slider = GetComponent<Slider>();
    }
    
    void LateUpdate()
    {
        if (target != null && mainCamera != null)
        {
            // 跟随目标位置
            transform.position = target.position + offset;
            // 始终面向摄像机
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }
    
    public void SetHealth(float current, float max)
    {
        if (slider != null)
            slider.value = current / max;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
