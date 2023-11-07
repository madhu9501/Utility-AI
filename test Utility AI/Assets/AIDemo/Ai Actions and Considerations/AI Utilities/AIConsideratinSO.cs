using UnityEngine;


// Abstract class for Ai Consideration scriptable object
public abstract class AIConsideratinSO<T> : ScriptableObject
{
    public string Name;
    private float score;
    public float Score{
        get{ return score; }
        set{ this.score = Mathf.Clamp01(value); }
    }

    public virtual void Awake()
    {
        score = 0;
    }

    public abstract float ConsiderationScore(T npc);
}
