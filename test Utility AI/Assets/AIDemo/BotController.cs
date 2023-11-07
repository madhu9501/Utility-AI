// using System.Collections;
// using UnityEngine;
// // using UnityEngine.Events;


// public class BotController : MonoBehaviour
// {
//     public NpcController npcController;

//     Vector3 initPos;

//     private bool _playerTurnSkip;

//     void Awake()
//     {
//         npcController = GetComponent<NpcController>();
//     }

//     private void Start()
//     {
//         GameManager.Instance.BotTurnStartEvent += StartBotTurn;
//         GameManager.Instance.BotTurnEnded();
//         initPos = transform.position;
//         _playerTurnSkip = false;

//     }

//     public void StartBotTurn()
//     {

//         StartCoroutine(BotMovement());
//     }

//     private IEnumerator BotMovement()
//     {
//         yield return new WaitForSeconds(1f); 

//         npcController.PickPowerAction();

//         if(!npcController.DiceRollCompleted )
//             npcController.PlayDice();


       
//         MoveFwd(npcController.DiceValue);
//         Debug.Log("Bot Dice Value: " + npcController.DiceValue);
        
//         GameManager.Instance.BotTurnEndedEvent?.Invoke();
//     }



//     public void MoveFwd(int diceValue)
//     {   
//         var newPos = new Vector3(transform.position.x, transform.position.y, transform.position.z + diceValue * 1.1f);
//         var canMove = GameManager.Instance.CheckBotCanMove(newPos.z);
//         if(canMove)
//         {
//             transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + diceValue * 1.1f);
//             GameManager.Instance.CheckBotWinCondition(newPos.z);
//         }
//     }

//     public void Moveback( bool backToBase = false, int value = 0)
//     {
//         if(backToBase)
//         {
//             transform.position = initPos;
//         }

//         transform.position = new Vector3(transform.position.x , transform.position.y, transform.position.z - value * 1.1f);
//     }

//     public void SetPlayerTurnSkip(bool doSkip)
//     {
//         _playerTurnSkip = doSkip;
//     }

//     public bool GetPlayerTurnSkip()
//     {
//         return _playerTurnSkip;
//     }

    
//     // private void OnEnable()
//     // {
//     //     GameManager.Instance.BotTurnStartEvent += StartBotTurn;
//     //     Debug.Log(GameManager.Instance);
//     // }

//     private void OnDisable()
//     {
//         GameManager.Instance.BotTurnStartEvent -= StartBotTurn;
//     }
// }

