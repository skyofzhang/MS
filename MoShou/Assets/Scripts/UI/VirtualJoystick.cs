using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Components")]
    public RectTransform background;
    public RectTransform handle;
    
    [Header("Settings")]
    public float handleRange = 50f;
    public bool snapToCenter = true;
    
    private Vector2 inputVector;
    private Canvas canvas;
    private Camera cam;
    
    // 输出给PlayerController使用
    public Vector2 InputDirection => inputVector;
    
    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            cam = canvas.worldCamera;
        
        // 初始化位置
        if (handle != null)
            handle.anchoredPosition = Vector2.zero;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, 
            eventData.position, 
            cam, 
            out position
        );
        
        // 计算输入方向
        position = position / (background.sizeDelta / 2);
        inputVector = new Vector2(position.x, position.y);
        inputVector = Vector2.ClampMagnitude(inputVector, 1f);
        
        // 移动摇杆手柄
        if (handle != null)
        {
            handle.anchoredPosition = inputVector * handleRange;
        }
        
        // 传递给玩家控制器
        UpdatePlayerInput();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        
        if (snapToCenter && handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
        
        UpdatePlayerInput();
    }
    
    void UpdatePlayerInput()
    {
        // 找到玩家并更新输入
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.JoystickInput = inputVector;
        }
    }
    
    // 编辑器中创建默认摇杆UI
    [ContextMenu("Setup Default Joystick")]
    void SetupDefaultJoystick()
    {
        // 背景
        if (background == null)
        {
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(transform);
            background = bgGO.AddComponent<RectTransform>();
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(1, 1, 1, 0.3f);
            background.sizeDelta = new Vector2(150, 150);
            background.anchoredPosition = Vector2.zero;
        }
        
        // 手柄
        if (handle == null)
        {
            var handleGO = new GameObject("Handle");
            handleGO.transform.SetParent(background);
            handle = handleGO.AddComponent<RectTransform>();
            var handleImage = handleGO.AddComponent<Image>();
            handleImage.color = new Color(1, 1, 1, 0.8f);
            handle.sizeDelta = new Vector2(60, 60);
            handle.anchoredPosition = Vector2.zero;
        }
    }
}
