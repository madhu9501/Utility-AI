// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// [CreateAssetMenu(fileName = "PlayerDistToWinConsideration", menuName ="ScriptableObject/AI/Consideration/PlayerDistToWinConsideration")]

// public class PlayerDistToWinConsiderationSO : AIConsideratinSO
// {
//     [SerializeField] private AnimationCurve responseCurve;
//     public override float ConsiderationScore(NpcController npc)
//     {
//         Score = responseCurve.Evaluate(Mathf.Clamp01( 1 - (npc.GetPlayerDistToWin() / npc.GetMaxDist()) ));

//         return Score;
//     }
// }
