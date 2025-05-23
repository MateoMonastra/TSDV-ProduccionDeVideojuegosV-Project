﻿using Enemies.BaseEnemy;
using UnityEngine;

namespace Enemies.RangeEnemy
{
    [CreateAssetMenu(fileName = "RangedEnemyModel", menuName = "Models/RangedEnemy")]
    public class RangedEnemyModel : BaseEnemyModel
    {
        
        [Header("Idle Values")] 
        [SerializeField] private int attacksCountToSpecialAttack;
        [SerializeField] private int cooldownBetweenAttacks;

        [Header("Attack Values")] 
        
        [SerializeField] private bool mustFaceTargetToFire;
        [SerializeField] private int totalShots;
        [SerializeField] private int shotsSeries;
        [SerializeField] private float height;
        [SerializeField] private float shotSpeed;
        [SerializeField] private float timeBetweenShots;
        [SerializeField] private float cooldownBetweenShots;
        [SerializeField] private float rotateVelocity;
        
        [Header("SpecialAttack Values")]
        [SerializeField] private float attackAreaRadius;
        [SerializeField] private float maxRayDistance;
        [SerializeField] private int specialAttacksCount;
        [SerializeField] private float projectileFallTime;
        [SerializeField] private float specialAttackCooldown;
        [SerializeField] private float damageRadius = 2.0f;
        [SerializeField] private float damageAmount = 1;

        
        //Idle
        public int AttacksCountToSpecialAttack
        {
            get => attacksCountToSpecialAttack;
            set => attacksCountToSpecialAttack = value;
        }
        public int CooldownBetweenAttacks
        {
            get => cooldownBetweenAttacks;
            set => cooldownBetweenAttacks = value;
        }
        
        //Attack
        public bool MustFaceTargetToFire
        {
            get => mustFaceTargetToFire;
            set => mustFaceTargetToFire = value;
        }
        public int TotalShots
        {
            get => totalShots;
            set => totalShots = value;
        }
        
        public int ShotsSeries
        {
            get => shotsSeries;
            set => shotsSeries = value;
        }

        public float ShotSpeed
        {
            get => shotSpeed;
            set => shotSpeed = value;
        }
        public float Height
        {
            get => height;
            set => height = value;
        }

        public float TimeBetweenShots
        {
            get => timeBetweenShots;
            set => timeBetweenShots = value;
        }

        public float CooldownBetweenShots
        {
            get => cooldownBetweenShots;
            set => cooldownBetweenShots = value;
        } 
        public float RotateVelocity
        {
            get => rotateVelocity;
            set => rotateVelocity = value;
        }
        
        //SpecialAttack
        public float AttackAreaRadius
        {
            get => attackAreaRadius;
            set => attackAreaRadius = value;
        }
        public float MaxRayDistance
        {
            get => maxRayDistance;
            set => maxRayDistance = value;
        }

        public int AttacksCount
        {
            get => specialAttacksCount;
            set => specialAttacksCount = value;
        }

        public float ProjectileFallTime
        {
            get => projectileFallTime;
            set => projectileFallTime = value;
        }

        public float SpecialAttackCooldown
        {
            get => specialAttackCooldown;
            set => specialAttackCooldown = value;
        }

        public float DamageRadius
        {
            get => damageRadius;
            set => damageRadius = value;
        }

        public float DamageAmount
        {
            get => damageAmount;
            set => damageAmount = value;
        }
    }
}