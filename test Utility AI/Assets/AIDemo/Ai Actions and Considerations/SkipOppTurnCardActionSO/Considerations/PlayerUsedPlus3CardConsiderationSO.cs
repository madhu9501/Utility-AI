// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// [CreateAssetMenu(fileName = "PlayerUsedPlus3CardConsideration", menuName ="ScriptableObject/AI/Consideration/PlayerUsedPlus3CardConsideration")]

// public class PlayerUsedPlus3CardConsiderationSO : AIConsideratinSO
// {
//     [SerializeField] private AnimationCurve responseCurve1;
//     [SerializeField] private AnimationCurve responseCurve2;

//     public override float ConsiderationScore(NpcController npc)
//     {
//         // Score = (npc.PlayerPlus3CardUsed()) ? 0.3f : 0.7f;

//         // if(npc.PlayerPlus3CardUsed())
//         //     Score = responseCurve1.Evaluate(Mathf.Clamp01( 1 - (npc.GetPlayerDistToWin() / npc.GetMaxDist()) ));
//         // else
//         //     Score = responseCurve2.Evaluate(Mathf.Clamp01( 1 - (npc.GetPlayerDistToWin() / npc.GetMaxDist()) ));


//         return Score;
//     }
// }
