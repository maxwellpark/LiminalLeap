using Events;
using System;
using UnityEngine;
using EventType = Events.EventType;

public abstract class SubscriberMonoBehaviour : MonoBehaviour
{
    protected virtual EventType[] EventTypes { get; } = new EventType[0];

    // Overrides usually build the array inline, so read it once rather than per lookup.
    private EventType[] types;
    private EventType[] Types => types ??= EventTypes;

    protected virtual void OnEnable()
    {
        Subscribe();
    }

    protected virtual void OnDisable()
    {
        Unsubcribe();
    }

    private bool Handles(EventType type)
    {
        return Array.IndexOf(Types, type) >= 0;
    }

    private void Subscribe()
    {
        if (Handles(EventType.Death))
        {
            GameManager.EventService.Add<OnDeathEvent>(OnDeath);
        }
        if (Handles(EventType.Spawn))
        {
            GameManager.EventService.Add<OnSpawnEvent>(OnSpawn);
        }
        if (Handles(EventType.DataUpdated))
        {
            GameManager.EventService.Add<OnDataUpdatedEvent>(OnDataUpdated);
        }
    }

    private void Unsubcribe()
    {
        if (Handles(EventType.Death))
        {
            GameManager.EventService.Remove<OnDeathEvent>(OnDeath);
        }
        if (Handles(EventType.Spawn))
        {
            GameManager.EventService.Remove<OnSpawnEvent>(OnSpawn);
        }
        if (Handles(EventType.DataUpdated))
        {
            GameManager.EventService.Remove<OnDataUpdatedEvent>(OnDataUpdated);
        }
    }

    protected virtual void OnDeath(OnDeathEvent evt)
    {
        throw new NotImplementedException();
    }

    protected virtual void OnSpawn()
    {
        throw new NotImplementedException();
    }

    protected virtual void OnDataUpdated()
    {
        throw new NotImplementedException();
    }
}
