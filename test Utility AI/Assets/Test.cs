using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIConsiderationSO<T> : ScriptableObject
{
    public string Name;

    private float score;
    public float Score
    {
        get { return score; }
        set { this.score = Mathf.Clamp01(value); }
    }

    public virtual void Awake()
    {
        score = 0;
    }

    public abstract float ConsiderationScore(T agent);
}

public class BotCloseToWinConsiderationsSO : AIConsiderationSO<PlayerController>
{
    public float ManipulateDiceMaxProbability;
    public float ManipulateDiceMinProbability;

    public override float ConsiderationScore(PlayerController bot)
    {
        Score = (2 < 2.3f) ? ManipulateDiceMaxProbability : ManipulateDiceMinProbability;
        return Score;
    }
}


   