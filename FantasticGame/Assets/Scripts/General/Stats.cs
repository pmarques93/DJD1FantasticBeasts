﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats
{
    public float    MaxMana              { get; set; }
    public float    CurrentMana          { get; set; }
    public float    AttackManaCost       { get; set; }
    public float    ManaRegen            { get; set; }

    public float    MaxHP                { get; set; }
    public float    CurrentHP            { get; set; }

    public bool     IsAlive              { get; set; }

    public float    RangedDamage         { get; set; }
    public bool     CanRangeAttack       { get; set; }
    public bool     RangedAttacking      { get; set; }
    public float    RangedAttackDelay    { get; set; }
    public float    RangedAttackCounter  { get; set; }

    public float    MeleeDamage          { get; set; } 
    public bool     CanMeleeAttack       { get; set; }
    public float    MeleeAttackRange     { get; set; }
    public float    MeleeAttackDelay     { get; set; }
    public float    MeleeAttackCounter   { get; set; }


    public void TakeDamage(float damage)
    {
        CurrentHP -= damage;
        if (CurrentHP < 1)
            IsAlive = false;
    }

    public bool IsMaxHP()
    {
        bool isMaxHP = false;
        if (CurrentHP >= MaxHP)
            isMaxHP = true;
        return isMaxHP;
    }

    public void HealHP(float heal)
    {
        if (CurrentHP + heal > MaxHP)
            CurrentHP = MaxHP;
        else
            CurrentHP += heal;
    }

    public bool IsMaxMana()
    {
        bool isMaxMana = false;
        if (CurrentMana >= MaxMana)
            isMaxMana = true;
        return isMaxMana;
    }

    public void HealMana(float heal)
    {
        if (CurrentMana + heal > MaxMana)
            CurrentMana = MaxMana;
        else
            CurrentMana += heal;
    }

    public bool CanUseSpell()
    {
        bool useSpell = false;
        if (CurrentMana - AttackManaCost > 0) useSpell = true;
        return useSpell;
    }

    public void RegenMana()
    {
        if (CurrentMana < MaxMana)
            CurrentMana += Time.deltaTime * ManaRegen;
    }

    public void SpendMana()
    {
        CurrentMana -= AttackManaCost;
    }
}







