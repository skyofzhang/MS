// Unity API Stubs for compile verification
// This file provides minimal type definitions to verify C# syntax without Unity

using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public class Object
    {
        public string name;
        public string tag;
        public static void Destroy(Object obj) { }
        public static void Destroy(Object obj, float t) { }
        public static void DontDestroyOnLoad(Object obj) { }
        public static T Instantiate<T>(T original) where T : Object => default;
        public static T Instantiate<T>(T original, Transform parent) where T : Object => default;
        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object => default;
    }

    public class MonoBehaviour : Behaviour
    {
        public new T GetComponent<T>() => default;
        public T GetComponentInParent<T>() => default;
        public T GetComponentInChildren<T>() => default;
        public T[] GetComponentsInChildren<T>() => default;
        public static T FindObjectOfType<T>() => default;
        public void Invoke(string method, float time) { }
        public void StartCoroutine(IEnumerator routine) { }
        public void StopCoroutine(IEnumerator routine) { }
        public void StopAllCoroutines() { }
        protected static T Instantiate<T>(T original) where T : Object => default;
        protected static T Instantiate<T>(T original, Transform parent) where T : Object => default;
        protected static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object => default;
        protected static GameObject Instantiate(GameObject original) => null;
        protected static GameObject Instantiate(GameObject original, Transform parent) => null;
    }

    public class Behaviour : Component { public bool enabled; }

    public class Component : Object
    {
        public GameObject gameObject;
        public Transform transform;
        public T GetComponent<T>() => default;
    }

    public class GameObject : Object
    {
        public Transform transform;
        public bool activeSelf;
        public bool activeInHierarchy;

        public GameObject() { }
        public GameObject(string name) { }

        public void SetActive(bool value) { }
        public T GetComponent<T>() => default;
        public T AddComponent<T>() where T : Component => default;

        public static GameObject Find(string name) => null;
        public static GameObject FindWithTag(string tag) => null;
        public static GameObject FindGameObjectWithTag(string tag) => null;
        public static GameObject[] FindGameObjectsWithTag(string tag) => new GameObject[0];
        public static GameObject CreatePrimitive(PrimitiveType type) => null;

        public static GameObject Instantiate(GameObject original) => null;
        public static GameObject Instantiate(GameObject original, Transform parent) => null;
    }

    public enum PrimitiveType { Sphere, Capsule, Cylinder, Cube, Plane, Quad }
    public enum CameraClearFlags { Skybox, SolidColor, Depth, Nothing }

    public class Transform : Component, IEnumerable
    {
        public Vector3 position;
        public Vector3 localPosition;
        public Vector3 localScale;
        public Quaternion rotation;
        public Quaternion localRotation;
        public Vector3 forward;
        public Vector3 right;
        public Vector3 up;
        public Transform parent;
        public int childCount;
        public void SetParent(Transform parent) { }
        public void SetParent(Transform parent, bool worldPositionStays) { }
        public void LookAt(Transform target) { }
        public void LookAt(Vector3 worldPosition) { }
        public void Rotate(float x, float y, float z) { }
        public void Translate(Vector3 translation) { }
        public Transform GetChild(int index) => null;
        public IEnumerator GetEnumerator() => null;
    }

    public struct Vector2
    {
        public float x, y;
        public float magnitude;
        public Vector2 normalized { get { return this; } }
        public Vector2(float x, float y) { this.x = x; this.y = y; this.magnitude = 0; }
        public static Vector2 zero = new Vector2(0, 0);
        public static Vector2 one = new Vector2(1, 1);
        public static Vector2 ClampMagnitude(Vector2 v, float max) => v;
        public static float Distance(Vector2 a, Vector2 b) => 0;
        public static implicit operator Vector3(Vector2 v) => new Vector3(v.x, v.y, 0);

        public static Vector2 operator +(Vector2 a, Vector2 b) => a;
        public static Vector2 operator -(Vector2 a, Vector2 b) => a;
        public static Vector2 operator *(Vector2 a, float b) => a;
        public static Vector2 operator *(float a, Vector2 b) => b;
        public static Vector2 operator /(Vector2 a, float b) => a;
        public static Vector2 operator /(Vector2 a, int b) => a;
        public static Vector2 operator /(Vector2 a, Vector2 b) => a;
        public static bool operator ==(Vector2 a, Vector2 b) => true;
        public static bool operator !=(Vector2 a, Vector2 b) => false;
        public override bool Equals(object obj) => true;
        public override int GetHashCode() => 0;
    }

    public struct Vector3
    {
        public float x, y, z;
        public float magnitude;
        public Vector3 normalized { get { return this; } }
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; this.magnitude = 0; }
        public static Vector3 zero = new Vector3(0, 0, 0);
        public static Vector3 one = new Vector3(1, 1, 1);
        public static Vector3 up = new Vector3(0, 1, 0);
        public static Vector3 down = new Vector3(0, -1, 0);
        public static Vector3 left = new Vector3(-1, 0, 0);
        public static Vector3 right = new Vector3(1, 0, 0);
        public static Vector3 forward = new Vector3(0, 0, 1);
        public static Vector3 back = new Vector3(0, 0, -1);
        public static float Distance(Vector3 a, Vector3 b) => 0;
        public static Vector3 MoveTowards(Vector3 a, Vector3 b, float max) => a;
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a;
        public static float Angle(Vector3 from, Vector3 to) => 0;
        public static float Dot(Vector3 a, Vector3 b) => 0;
        public static Vector3 Cross(Vector3 a, Vector3 b) => zero;
        public static Vector3 Project(Vector3 vector, Vector3 onNormal) => zero;

        public static Vector3 operator +(Vector3 a, Vector3 b) => a;
        public static Vector3 operator -(Vector3 a, Vector3 b) => a;
        public static Vector3 operator *(Vector3 a, float b) => a;
        public static Vector3 operator *(float a, Vector3 b) => b;
        public static bool operator ==(Vector3 a, Vector3 b) => true;
        public static bool operator !=(Vector3 a, Vector3 b) => false;
        public override bool Equals(object obj) => true;
        public override int GetHashCode() => 0;
    }

    public struct Quaternion
    {
        public static Quaternion identity = new Quaternion();
        public static Quaternion Euler(float x, float y, float z) => identity;
        public static Quaternion LookRotation(Vector3 forward) => identity;
        public static Quaternion Slerp(Quaternion a, Quaternion b, float t) => identity;
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; this.a = 1; }
        public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color white = new Color(1, 1, 1);
        public static Color black = new Color(0, 0, 0);
        public static Color red = new Color(1, 0, 0);
        public static Color green = new Color(0, 1, 0);
        public static Color blue = new Color(0, 0, 1);
        public static Color yellow = new Color(1, 1, 0);
        public static Color gray = new Color(0.5f, 0.5f, 0.5f);
        public static Color cyan = new Color(0, 1, 1);
        public static Color magenta = new Color(1, 0, 1);
        public static Color clear = new Color(0, 0, 0, 0);
    }

    public struct Rect
    {
        public float x, y, width, height;
        public Rect(float x, float y, float w, float h) { this.x=x; this.y=y; width=w; height=h; }
    }

    public class Camera : Behaviour
    {
        public static Camera main;
        public new Transform transform;
        public CameraClearFlags clearFlags;
        public Color backgroundColor;
        public Vector3 WorldToScreenPoint(Vector3 pos) => Vector3.zero;
    }

    public class Collider : Component { public bool enabled; }
    public class SphereCollider : Collider { public float radius; public Vector3 center; public bool isTrigger; }
    public class BoxCollider : Collider { public Vector3 size; public Vector3 center; }
    public class CapsuleCollider : Collider { public float radius; public float height; }

    public class CharacterController : Collider
    {
        public bool isGrounded;
        public Vector3 velocity;
        public float height;
        public float radius;
        public Vector3 center;
        public CollisionFlags Move(Vector3 motion) => CollisionFlags.None;
        public bool SimpleMove(Vector3 speed) => true;
    }

    public enum CollisionFlags { None, Sides, Above, Below }

    public class Mesh : Object { public Vector3[] vertices; public int[] triangles; }
    public class MeshFilter : Component { public Mesh mesh; public Mesh sharedMesh; }
    public class MeshRenderer : Renderer { }

    public class Rigidbody : Component
    {
        public Vector3 velocity;
        public void AddForce(Vector3 force) { }
    }

    public class Renderer : Component { public Material material; public Material[] materials; }

    public class Shader : Object
    {
        public static Shader Find(string name) => null;
    }

    public class Material : Object
    {
        public Color color;
        public Material(Shader shader) { }
        public Material(Material source) { }
    }

    public class Sprite : Object { }
    public class TextAsset : Object { public string text; }

    public class AudioSource : Component
    {
        public AudioClip clip;
        public void Play() { }
        public void Stop() { }
    }
    public class AudioClip : Object { }
    public class AnimationClip : Object { }

    public class Animator : Behaviour
    {
        public void SetTrigger(string name) { }
        public void SetBool(string name, bool value) { }
        public void SetFloat(string name, float value) { }
        public void SetInteger(string name, int value) { }
        public void Play(string name) { }
    }

    public class Animation : Behaviour { public void Play(string name) { } }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject => default;
    }

    public class Resources
    {
        public static T Load<T>(string path) where T : Object => default;
        public static T GetBuiltinResource<T>(string name) where T : Object => default;
    }

    public class Application
    {
        public static string dataPath;
        public static string persistentDataPath;
        public static RuntimePlatform platform;
        public static void Quit() { }
    }

    public enum RuntimePlatform { Android, IPhonePlayer, WindowsPlayer, WebGLPlayer }

    public class Time
    {
        public static float time;
        public static float deltaTime;
        public static float fixedDeltaTime;
        public static float timeScale;
        public static float realtimeSinceStartup;
    }

    public class Input
    {
        public static bool GetKey(KeyCode key) => false;
        public static bool GetKeyDown(KeyCode key) => false;
        public static bool GetMouseButton(int button) => false;
        public static Vector3 mousePosition;
        public static float GetAxis(string name) => 0;
    }

    public enum KeyCode { None, Space, Return, Escape, LeftArrow, RightArrow, UpArrow, DownArrow }

    public class Debug
    {
        public static void Log(object msg) { }
        public static void LogWarning(object msg) { }
        public static void LogError(object msg) { }
    }

    public class PlayerPrefs
    {
        public static void SetInt(string key, int value) { }
        public static void SetFloat(string key, float value) { }
        public static void SetString(string key, string value) { }
        public static int GetInt(string key, int def = 0) => def;
        public static float GetFloat(string key, float def = 0) => def;
        public static string GetString(string key, string def = "") => def;
        public static bool HasKey(string key) => false;
        public static void DeleteKey(string key) { }
        public static void DeleteAll() { }
        public static void Save() { }
    }

    public class JsonUtility
    {
        public static string ToJson(object obj) => "";
        public static string ToJson(object obj, bool prettyPrint) => "";
        public static T FromJson<T>(string json) => default;
        public static void FromJsonOverwrite(string json, object obj) { }
    }

    public class Mathf
    {
        public static float Lerp(float a, float b, float t) => a;
        public static float Clamp(float value, float min, float max) => value;
        public static float Clamp01(float value) => value;
        public static float Max(float a, float b) => a;
        public static float Min(float a, float b) => a;
        public static int Max(int a, int b) => a;
        public static int Min(int a, int b) => a;
        public static float Abs(float v) => v;
        public static int FloorToInt(float v) => 0;
        public static int CeilToInt(float v) => 0;
        public static int RoundToInt(float v) => 0;
        public static float Sin(float f) => 0;
        public static float Cos(float f) => 0;
        public static float Sqrt(float f) => 0;
        public const float Infinity = float.MaxValue;
        public const float PI = 3.14159274f;
    }

    public class Random
    {
        public static float value;
        public static float Range(float min, float max) => min;
        public static int Range(int min, int max) => min;
        public static Vector2 insideUnitCircle => Vector2.zero;
        public static Vector3 insideUnitSphere => Vector3.zero;
    }

    public class Physics
    {
        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance) { hit = default; return false; }
        public static Collider[] OverlapSphere(Vector3 pos, float radius) => new Collider[0];
    }

    public struct RaycastHit { public Vector3 point; public Vector3 normal; public Collider collider; public float distance; }

    public class LayerMask { public static int GetMask(params string[] names) => 0; public int value; }

    public class Gizmos { public static Color color; public static void DrawWireSphere(Vector3 center, float radius) { } }

    public class WaitForSeconds : YieldInstruction { public WaitForSeconds(float seconds) { } }
    public class WaitForSecondsRealtime { public WaitForSecondsRealtime(float seconds) { } }
    public class YieldInstruction { }
    public class Coroutine { }

    public class Font : Object { }

    public class TextMesh : Component
    {
        public string text;
        public Color color;
        public int fontSize;
        public float characterSize;
        public TextAlignment alignment;
        public TextAnchor anchor;
    }

    public enum TextAlignment { Left, Center, Right }
    public enum TextAnchor { UpperLeft, UpperCenter, UpperRight, MiddleLeft, MiddleCenter, MiddleRight, LowerLeft, LowerCenter, LowerRight }

    // Attributes
    public class SerializeField : Attribute { }
    public class HeaderAttribute : Attribute { public HeaderAttribute(string header) { } }
    public class TooltipAttribute : Attribute { public TooltipAttribute(string tooltip) { } }
    public class RangeAttribute : Attribute { public RangeAttribute(float min, float max) { } }
    public class HideInInspector : Attribute { }
    public class RequireComponent : Attribute { public RequireComponent(Type type) { } }
    public class ExecuteInEditMode : Attribute { }
    public class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
    public class ContextMenu : Attribute { public ContextMenu(string name) { } }

    // Canvas
    public class Canvas : Behaviour
    {
        public RenderMode renderMode;
        public int sortingOrder;
        public Camera worldCamera;
    }
    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }
    public class CanvasGroup : Component { public float alpha; public bool blocksRaycasts; public bool interactable; }
    public class RectTransform : Transform
    {
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
    }
    public class RectTransformUtility
    {
        public static bool ScreenPointToLocalPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam, out Vector2 localPoint) { localPoint = Vector2.zero; return true; }
    }

    // GUILayout
    public class GUILayout
    {
        public static void Label(string text) { }
        public static void Label(string text, params GUILayoutOption[] options) { }
        public static bool Button(string text) => false;
        public static bool Button(string text, params GUILayoutOption[] options) => false;
        public static void BeginHorizontal(params GUILayoutOption[] options) { }
        public static void EndHorizontal() { }
        public static void BeginVertical(params GUILayoutOption[] options) { }
        public static void EndVertical() { }
        public static void BeginArea(Rect screenRect) { }
        public static void EndArea() { }
        public static void Box(string text) { }
        public static void Space(float pixels) { }
        public static void FlexibleSpace() { }
        public static string TextField(string text, params GUILayoutOption[] options) => text;
        public static GUILayoutOption Width(float width) => null;
        public static GUILayoutOption Height(float height) => null;
    }

    public class GUILayoutOption { }

    public class GUI
    {
        public static Color color;
        public static Color backgroundColor;
        public static Color contentColor;
    }
}

namespace UnityEngine.UI
{
    public class Image : UnityEngine.Behaviour
    {
        public Sprite sprite;
        public Color color;
        public new UnityEngine.GameObject gameObject;
    }
    public class Text : UnityEngine.Behaviour
    {
        public string text;
        public Color color;
        public Font font;
        public int fontSize;
        public TextAnchor alignment;
        public new UnityEngine.GameObject gameObject;
    }
    public class Button : UnityEngine.Behaviour
    {
        public bool interactable;
        public ButtonClickedEvent onClick;
        public class ButtonClickedEvent
        {
            public void AddListener(UnityEngine.Events.UnityAction action) { }
            public void RemoveListener(UnityEngine.Events.UnityAction action) { }
            public void RemoveAllListeners() { }
        }
    }
    public class Slider : UnityEngine.Behaviour
    {
        public float value;
        public float minValue;
        public float maxValue;
    }
    public class Toggle : UnityEngine.Behaviour { public bool isOn; }
    public class InputField : UnityEngine.Behaviour { public string text; }
    public class Dropdown : UnityEngine.Behaviour { public int value; }
    public class ScrollRect : UnityEngine.Behaviour { }
    public class GridLayoutGroup : UnityEngine.Behaviour { }
    public class HorizontalLayoutGroup : UnityEngine.Behaviour { }
    public class VerticalLayoutGroup : UnityEngine.Behaviour { }
    public class LayoutElement : UnityEngine.Behaviour { }
    public class ContentSizeFitter : UnityEngine.Behaviour { }
    public class Graphic : UnityEngine.Behaviour { public Color color; }
    public class Selectable : UnityEngine.Behaviour { public bool interactable; }
}

namespace UnityEngine.Events
{
    public delegate void UnityAction();
    public delegate void UnityAction<T>(T arg);
    public class UnityEvent { public void AddListener(UnityAction action) { } public void RemoveListener(UnityAction action) { } public void Invoke() { } }
    public class UnityEvent<T> { public void AddListener(UnityAction<T> action) { } public void Invoke(T arg) { } }
}

namespace UnityEngine.EventSystems
{
    public interface IPointerDownHandler { void OnPointerDown(PointerEventData eventData); }
    public interface IPointerUpHandler { void OnPointerUp(PointerEventData eventData); }
    public interface IDragHandler { void OnDrag(PointerEventData eventData); }
    public interface IPointerClickHandler { void OnPointerClick(PointerEventData eventData); }
    public class PointerEventData { public Vector2 position; public Vector2 delta; }
    public class EventSystem : UnityEngine.MonoBehaviour { public static EventSystem current; }
}

namespace UnityEngine.SceneManagement
{
    public class SceneManager
    {
        public static void LoadScene(string name) { }
        public static void LoadScene(int index) { }
        public static AsyncOperation LoadSceneAsync(string name) => null;
    }
    public class AsyncOperation { public bool isDone; public float progress; public bool allowSceneActivation; }
}

namespace UnityEngine.Rendering
{
    public enum GraphicsDeviceType { Null, Direct3D11, OpenGLES3, Vulkan, Metal }
}
