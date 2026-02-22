using UnityEngine;
using Enemies.BaseEnemy;
using System;
using Unity.VisualScripting;
public class BaseEnemyAudio : MonoBehaviour
{
    [SerializeField] 
    BaseEnemyAgent enemyBase;
    [SerializeField]
    private AK.Wwise.Event _akEnemy1GetHit;
    [SerializeField]
    private AK.Wwise.Event _akEnemy1HitGround;


    void OnEnable()
    {
        enemyBase.onAttackHit.AddListener(() => OnAttackHit(enemyBase));
        enemyBase.onAttackFinish.AddListener(() => OnAttackFinish(enemyBase));
        enemyBase.onBeingAttacked.AddListener(() => OnbeingAttacked(enemyBase));
        enemyBase.onSpinningVerticalImpulseEnded.AddListener(() => OnEnemyHitGround(enemyBase));
    }

    private void OnEnemyHitGround(BaseEnemyAgent enemyBase)
    {
        _akEnemy1HitGround.Post(this.gameObject);
    }

    private void OnbeingAttacked(BaseEnemyAgent enemyBase)
    {
        Debug.Log("AUDIO: Enemy Being Hit");
        _akEnemy1GetHit.Post(this.gameObject);
    }

    void OnDisable()
    {
        enemyBase.onAttackHit.RemoveListener(() => OnAttackHit(enemyBase));
        enemyBase.onAttackFinish.RemoveListener(() => OnAttackFinish(enemyBase));
    }

    private void OnAttackFinish(BaseEnemyAgent enemy)
    {
        //Debug.Log("AUDIO: Enemy Attacked");
    }

    private void OnAttackHit(BaseEnemyAgent enemy)
    {
        //Debug.Log("AUDIO: Enemy Attackig");
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
