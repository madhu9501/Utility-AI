using UnityEngine;

// Abstract class for Ai Action scriptable object
public abstract class AIActionSO<T> : ScriptableObject
{
    public string Name;
    public AIConsideratinSO<NpcController>[] consideration;

    public virtual bool CanRemove {get; set;} = true;
    private float score;
    public float Score{
        get{ return score; }
        set{ score = Mathf.Clamp01(value); }
    }


    public virtual void Awake()
    {

        score = 0;
    }

    public abstract void ExecuteAction(T npc);
}
