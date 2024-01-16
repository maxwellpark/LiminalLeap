using Events;
using System;
using System.Linq;
using UnityEngine;
using EventType = Events.EventType;

public abstract class SubscriberMonoBehaviour : MonoBehaviour
{
    protected virtual EventType[] EventTypes { get; } = new EventType[0];

    protected virtual void OnEnable()
    {
        Subscribe();
    }

    protected virtual void OnDisable()
    {
        Unsubcribe();
    }

    private void Subscribe()
    {
        if (EventTypes.Contains(EventType.Death))
        {
            GameManager.EventService.Add<OnDeathEvent>(OnDeath);
        }
        if (EventTypes.Contains(EventType.Spawn))
        {
            GameManager.EventService.Add<OnSpawnEvent>(OnSpawn);
        }
        if (EventTypes.Contains(EventType.DataUpdated))
        {
            GameManager.EventService.Add<OnDataUpdatedEvent>(OnDataUpdated);
        }
    }

    private void Unsubcribe()
    {
        if (EventTypes.Contains(EventType.Death))
        {
            GameManager.EventService.Remove<OnDeathEvent>(OnDeath);
        }
        if (EventTypes.Contains(EventType.Spawn))
        {
            GameManager.EventService.Remove<OnSpawnEvent>(OnSpawn);
        }
        if (EventTypes.Contains(EventType.DataUpdated))
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