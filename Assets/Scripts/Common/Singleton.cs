using UnityEngine;

public class Singleton<T> : SubscriberMonoBehaviour where T : MonoBehaviour
{
    protected Singleton() { }

    protected static T instance;

    public static T Instance => instance;

    public static T GetInstance()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<T>();
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

    protected virtual void OnDestroy()
    {
        if ((object)instance == this)
        {
            instance = null;
        }
    }

    public virtual void Init() { }
}
