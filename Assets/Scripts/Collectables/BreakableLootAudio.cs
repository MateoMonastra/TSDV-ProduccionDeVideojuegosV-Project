using System;
using Coins;
using UnityEngine;
using Event = AK.Wwise.Event;

public class BreakableLootAudio : MonoBehaviour
{
    [SerializeField] private BreakableLoot breakableLoot;
    [SerializeField] private Event BreakSFX;
    private void OnEnable()
    {
        breakableLoot._breakSfx += OnBreakSFX;
    }

    private void OnDisable()
    {
        breakableLoot._breakSfx -= OnBreakSFX;
    }

    private void OnBreakSFX()
    {
        BreakSFX?.Post(gameObject);
    }
}
