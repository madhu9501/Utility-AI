using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PowerupHandler : MonoBehaviour
{
    [Serializable]
    public struct PowerInterface
    {
        public PowerType type;
        public Button button;
    }
    //public List<PowerInterface> powerupInterface;
    private PlayerController currentPlayer;

    public bool PowerUpSelectionActive = false;

    private bool isPowersDisabled = false;

    private void Start()
    {
        /*
        foreach (var power in powerupInterface)
        {
            PowerType type = power.type;
            power.button.onClick.AddListener(() => { PowerupClicked(type); });
        }
        */
    }

    public void InitPowers(PlayerController _currentPlayer, bool isPlayerOneCycleComplete)
    {
        this.currentPlayer = _currentPlayer;
        List<PowerType> activePowers = new List<PowerType>();

        if (!currentPlayer.isPowerupTurnMove)
        {
            activePowers = currentPlayer.ActivePowerups(isPlayerOneCycleComplete);
        }

        isPowersDisabled = activePowers.Count <= 0;
        currentPlayer.isPowerupTurnMove = false;
        // currentPlayer.playerUI.EnableDisablePowers(activePowers);

        // todo - add bot powerups logic 
    }


    public void PowerupClicked(PowerType power, bool isBot = false)
    {
        // Logger.Log($"{currentPlayer.userId} Power click -> {power}");
        GameController.instance.activeDice.OnRollDisable();

        currentPlayer.PlayerPowerUsed(power, isBot);

        // currentPlayer.playerUI.ResumeTimer();

        if (power == PowerType.Arrows)
        {
            ArrowStormPower();
        }
        else if (power == PowerType.Shield || power == PowerType.Lock)
        {
            currentPlayer.powerCycleCount = currentPlayer.nuberOfTurnsPlayed;
        }
    }

    public void ArrowStormPower()
    {

        // get all the warriors except the player 
        List<PlayerController> otherPlayers = GameController.instance.GetOtherPlayers(currentPlayer.userId);
        List<WarriorController> warriors = new List<WarriorController>();
        foreach (var item in otherPlayers)
        {
            warriors.AddRange(item.GetActiveWarriors());
        }

        //Arrow stro audio
        // AudioController.con.PlayUISFX(GameController.instance.arrowStromAudioClip);


        //Logger.Log($"Total active warriros - {warriors.Count}");
        Sequence seq = DOTween.Sequence();

        foreach (var item in warriors)
        {
            item.WarriorSparkleTrail.SetActive(false);
            item.WarriorSparkleTrail2.SetActive(false);

            seq.Join(item.MoveBackward());
        }

        seq.Play().AppendCallback(() =>
        {
            warriors.ForEach(w => w.OnWarriorMoveComplete(w.currentCell, null));
            DOVirtual.DelayedCall(1.1f, () =>
            {
                currentPlayer.OnWarriorMoveComplete();
                // currentPlayer.playerUI.ScalePowerEffect(PowerType.Arrows, () => { Logger.Log("Arrow Strom Deployed"); }, true);
            });
        });

    }

    public bool CheckLockCastlePower(List<CellHandler> baseCells)
    {
        bool result = false;
        var players = GameController.instance.GetOtherPlayers(null).Where(p => p.IsPowerRunning(PowerType.Lock));

        if (players != null)
        {
            foreach (var item in baseCells)
            {
                var powerPlayer = players.Where(p => p.pillageCell.cellId == item.cellId).ToList();
                if (powerPlayer != null && powerPlayer.Count > 0)
                    result = true;
            }

        }
        return result;
    }

    //    @Madhu - based on certian conditions bot picks a card
    public PowerType PlayerHasPowerup()
    {
        // get number of active warriros 
        var activePlayers = GameController.instance.GetOtherPlayers(null);
        int totalWarriors = activePlayers.Count * 2;
        int numberOfWarriorActive = 0;
        activePlayers.ForEach(p => numberOfWarriorActive += p.GetActiveWarriors().Count);

        // get  no of warrior unlokced
        var _botcurrentPlayer = GameController.instance.GetCurrentPlayer;
        int numberOfLockedWarriors = _botcurrentPlayer.GetLockWarriors().Count;
        int numberOfActiveWarriorsBot = _botcurrentPlayer.GetActiveWarriors().Count;

        var activePowers = _botcurrentPlayer.ActivePowerups(false);

        var pillageCell = _botcurrentPlayer.pillageCell;
        var currentPlayerActiveWarrior = _botcurrentPlayer.GetActiveWarriors();

        // if enemy warrior is close to the base chosses lock power    
        if (activePowers.Contains(PowerType.Lock))
        {
            var otherPlayers = GameController.instance.GetOtherPlayers(_botcurrentPlayer.userId);

            for (int i = 0; i < otherPlayers.Count; i++)
            {
                var otherWarriors = otherPlayers[i].GetActiveWarriors();

                // Logger.Log("Lock castle Other warrior " + otherWarriors.Count);

                for (int j = 0; j < otherWarriors.Count; j++)
                {
                    var diff = 7;

                    var currentPath = otherWarriors[j].currentPath;
                    var currentCell = otherWarriors[j].currentCell;

                    var currentIndex = currentPath.FindIndex(c => c.cellId == currentCell.cellId);

                    if (currentIndex < 0) return PowerType.NA;

                    if ((currentIndex + 7) > currentPath.Count)
                        diff = currentPath.Count - currentIndex;

                    var futurePath = currentPath.GetRange(currentIndex, diff);

                    if (futurePath.Contains(pillageCell))
                    {
                        return PowerType.Lock;
                    }

                }

            }

        }

        // if enemy pawn is close to bot pawn use shield, if in base dont use
        if (activePowers.Contains(PowerType.Shield))
        {
            var otherPlayers = GameController.instance.GetOtherPlayers(_botcurrentPlayer.userId);

            bool shouldBreak = false;
            for (int i = 0; i < otherPlayers.Count; i++)
            {
                var otherWarriors = otherPlayers[i].GetActiveWarriors();

                for (int j = 0; j < otherWarriors.Count; j++)
                {
                    if (shouldBreak == false)
                    {
                        var diff = 7;

                        var currentPath = otherWarriors[j].currentPath;
                        var currentCell = otherWarriors[j].currentCell;

                        var currentIndex = currentPath.FindIndex(c => c.cellId == currentCell.cellId);

                        if (currentIndex < 0) return PowerType.NA;

                        if ((currentIndex + 7) > currentPath.Count)
                            diff = currentPath.Count - currentIndex;

                        var futurePath = currentPath.GetRange(currentIndex, diff);

                        for (int k = 0; k < currentPlayerActiveWarrior.Count; k++)
                        {
                            if (futurePath.Contains(currentPlayerActiveWarrior[k].currentCell) && !shouldBreak)
                            {
                                shouldBreak = true;
                                return PowerType.Shield;
                            }
                        }
                    }

                }

            }

        }

        // use revive if bot pawn is locked in base
        // capture data 
        if (numberOfLockedWarriors > 0 && activePowers.Contains(PowerType.Revive))
        {
            // revive
            var captureWarrior = (_botcurrentPlayer.GetLockWarriors()?.FindAll(w => w.captureData.isCapture)) ?? new List<WarriorController>();
            if (captureWarrior.Count > 0) return PowerType.Revive;
        }

        // use release card if warrior is in base and has already played more than 3 turns
        if (_botcurrentPlayer.nuberOfTurnsPlayed > 1 && numberOfLockedWarriors > 0 && activePowers.Contains(PowerType.Release))
        {
            //unleash 
            return PowerType.Release;
        }

        List<int> randomValues = new List<int>() { 0, 1 };
        int randVal = randomValues[UnityEngine.Random.Range(0, randomValues.Count)];

        // use doubler card if radom value generated is 0 and number of turns played is more than 11 and there are any active warrior in game 
        if (randVal == 0 && numberOfActiveWarriorsBot > 0 && _botcurrentPlayer.nuberOfTurnsPlayed > 11)
        {
            if (activePowers.Contains(PowerType.Doubler)) return PowerType.Doubler;
        }
        // use doubler card if radom value generated is 0 abd number of turns played is more that 11 and there are more tha 3 active warior in game
        if (randVal == 0 && numberOfWarriorActive > 3 && _botcurrentPlayer.nuberOfTurnsPlayed > 11)
        {
            // two moves
            if (activePowers.Contains(PowerType.Twice)) return PowerType.Twice;
        }

        // use arrow card if number ot turns played is gater than 11 and active warrior is game is more than 1  
        if (numberOfWarriorActive > (totalWarriors - 1) || _botcurrentPlayer.nuberOfTurnsPlayed > 8)
        {
            // sheild removed by madhav
            //if (activePowers.Contains(PowerType.Shield_Warrior)) return PowerType.Shield_Warrior;

            // lock castle removed by madhav
            //if (activePowers.Contains(PowerType.Lock_Castle) && _botcurrentPlayer.nuberOfTurnsPlayed > 10) return PowerType.Lock_Castle;

            // arrow
            if (activePowers.Contains(PowerType.Arrows) && _botcurrentPlayer.nuberOfTurnsPlayed > 11
                && numberOfWarriorActive > 1) return PowerType.Arrows;
        }

        return PowerType.NA;
    }

    public void BotPowerUpUsed(PowerType _type)
    {
        var activePowers = currentPlayer.ActivePowerups(false);
        var _randP = _type;
        // currentPlayer.playerUI.BotPowerupUsed(_randP);

    }
}