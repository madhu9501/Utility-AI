// using UnityEngine;

// [CreateAssetMenu(fileName = "BotDistanceToWinConsideration", menuName = "ScriptableObject/AI/Consideration/BotDistanceToWinConsideration")]

// public class BotDistanceToWinConsiderationSO : AIConsideratinSO
// {
//     [SerializeField] private AnimationCurve responseCurve;
//     public override float ConsiderationScore(NpcController npc)
//     {
//         Score = responseCurve.Evaluate(Mathf.Clamp01( 1 - (npc.GetBotDistToWin() / npc.GetMaxDist()) ));
//         return Score;
//     }
// }
