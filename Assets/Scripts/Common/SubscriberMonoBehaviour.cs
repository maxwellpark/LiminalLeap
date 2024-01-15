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
    }

    private void Unsubcribe()
    {
        if (EventTypes.Contains(EventType.Death))
        {
            GameManager.EventService.Remove<OnDeathEvent>(OnDeath);
        }
    }

    protected virtual void OnDeath(OnDeathEvent evt)
    {
        throw new NotImplementedException();
    }
}