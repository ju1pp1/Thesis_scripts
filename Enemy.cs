using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Stats))]
public class Enemy : Interactable
{
    PlayerManager playerManager;
    public Stats myStats;

    void Start()
    {

        playerManager = PlayerManager.instance;
        myStats = GetComponent<Stats>();
    }
    public override void Interact()
    {
        base.Interact();

        if(myStats.faction == Stats.Faction.Enemy)
        {
            //Attack the enemy
            CharacterCombat playerCombat = playerManager.player.GetComponent<CharacterCombat>();

            if (playerCombat != null)
            {
                playerCombat.Attack(myStats);
            }
        }
    }
}
