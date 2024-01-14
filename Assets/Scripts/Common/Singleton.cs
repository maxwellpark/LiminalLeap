using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected Singleton() { }

    protected static T instance;

    public static T Instance => instance;

    public static T GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<T>();
            if (instance == null)
            {
                var singleton = new GameObject();
                instance = singleton.AddComponent<T>();
                singleton.name = "[singleton] " + typeof(T).ToString();
            }
        }
        return instance;
    }

    protected virtual void Awake()
    {
        if (instance != null && instance.gameObject != gameObject)
        {
            Destroy(gameObject);
            return;
        }

        instance = (T)(object)this;
        Init();
    }

    public virtual void Init() { }
}
