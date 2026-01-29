using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStats : Stats
{
    private Coroutine healingCoroutine;
    private Coroutine healthRegenCoroutine;
    private Coroutine manaRegenCoroutine;
    private Coroutine guardRegenCoroutine;
    private Coroutine bleedCoroutine;
    void Start()
    {
        for (int i = 0; i < attributes.Length; i++)
        {
            attributes[i].SetParent(this);

            if (transform.gameObject.GetComponent<Stats>().attributes[i].type == Attributes.Health)
            {
                currentHealth = maxHealth;

            }
            if (transform.gameObject.GetComponent<Stats>().attributes[i].type == Attributes.Mana)
            {
                currentMana = maxMana;
            }
            if (transform.gameObject.GetComponent<Stats>().attributes[i].type == Attributes.Guard)
            {
                currentGuard = maxGuard;
            }
            if (transform.gameObject.GetComponent<Stats>().attributes[i].type == Attributes.PhysicalDamage)
            {
                //attributes[i].value.BaseValue = 5;
                //physicalDamage = transform.gameObject.GetComponent<Stats>().attributes[i].value.ModifiedValue;
            }
            if (transform.gameObject.GetComponent<Stats>().attributes[i].type == Attributes.Armor)
            {
                //attributes[i].value.BaseValue = 0;
                //armor = transform.gameObject.GetComponent<Stats>().attributes[i].value.ModifiedValue;
            }
            if (transform.gameObject.GetComponent<Stats>().attributes[i].type == Attributes.AttackSpeed)
            {
                //attributes[i].value.BaseValue = 1;
                //attackSpeed = transform.gameObject.GetComponent<Stats>().attributes[i].value.ModifiedValue;

            }
            if (transform.gameObject.GetComponent<Stats>().attributes[i].type == Attributes.HealthRegen)
            {
                //attributes[i].value.BaseValue = 1;
                //healthRegen = transform.gameObject.GetComponent<Stats>().attributes[i].value.ModifiedValue;

            }
            if (transform.gameObject.GetComponent<Stats>().attributes[i].type == Attributes.ManaRegen)
            {
                //attributes[i].value.BaseValue = 1;
                //manaRegen = transform.gameObject.GetComponent<Stats>().attributes[i].value.ModifiedValue;

            }
        }
        
        healthRegenCoroutine = StartCoroutine(RegenerateHealth());
        manaRegenCoroutine = StartCoroutine(RegenerateMana());
        guardRegenCoroutine = StartCoroutine(RegenerateGuard());
    }

    private IEnumerator RegenerateHealth()
    {
        yield return new WaitForSeconds(10f);
        while (currentHealth > 0)
        {
            ModifyHealth(healthRegen);
            yield return new WaitForSeconds(10f);
        }
    }

    private IEnumerator RegenerateMana()
    {
        yield return new WaitForSeconds(10f);
        while (currentMana >= 0)
        {
            ModifyMana(manaRegen);
            yield return new WaitForSeconds(10f);
        }
    }

    private IEnumerator RegenerateGuard()
    {
        yield return new WaitUntil(() => maxGuard > 0);

        // Guard implementation
        while (currentHealth > 0)
        {
            yield return new WaitForSeconds(guardRegenInterval);

            if (currentGuard < maxGuard)
            {
                ModifyGuard(solidGuardAmount);
            }
        }

    }
    private void Update()
    {
        
    }
    public override void Die()
    {
        base.Die();

        if (healthRegenCoroutine != null)
            StopCoroutine(healthRegenCoroutine);

        //Add ragdoll effect / death animation

        //Loot
        
        foreach(var co in activeBurns.Values) StopCoroutine(co);
        foreach (var co in activeBleeds.Values) StopCoroutine(co);
        activeBurns.Clear();
        activeBleeds.Clear();


        var ui = GetComponent<EnemyHealthUI>();
        if(ui != null)
        {
            ui.enabled = false;
            Destroy(ui.gameObject);
        }
        Destroy(gameObject);
    }
}
