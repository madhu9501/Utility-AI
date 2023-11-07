using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonBaseClass<T> : SingletonBaseClass where T : SingletonBaseClass<T>
{
    private static T _instance;
    public static T instance { get { return _instance; } }

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
    }
    protected void OnDestroy()
    {
        _instance = null;
    }
}

public abstract class SingletonBaseClass : MonoBehaviour
{
    
}
