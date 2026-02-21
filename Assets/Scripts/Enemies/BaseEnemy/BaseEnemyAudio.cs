using UnityEngine;
using Enemies.BaseEnemy;
using System;
public class BaseEnemyAudio : MonoBehaviour
{
    [SerializeField] 
    BaseEnemyAgent enemyBase;
    [SerializeField]
    private AK.Wwise.Event _akEnemy1GetHit;

    void OnEnable()
    {
        enemyBase.onAttackHit.AddListener(() => OnAttackHit(enemyBase));
        enemyBase.onAttackFinish.AddListener(() => OnAttackFinish(enemyBase));
        enemyBase.onBeingAttacked.AddListener(() => OnbeingAttacked(enemyBase));
    }

    private void OnbeingAttacked(BaseEnemyAgent enemyBase)
    {
        Debug.Log("AUDIO: Enemy Attacked");
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
