using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void CustomDelegate(); 

public class UtilityRoutines : SingletonNew<UtilityRoutines>
{
    public Coroutine DelayedCall(float delay, CustomDelegate customDelegate = null)
    {
        if (Mathf.Abs(delay) <= Mathf.Epsilon)
        {
            customDelegate?.Invoke();
            return null;
        }
        return StartCoroutine(DelayedCallRoutine(delay, customDelegate, false));
    }
    
    public Coroutine DelayedCallRealtime(float delay, CustomDelegate customDelegate = null)
    {
        if (Mathf.Abs(delay) <= Mathf.Epsilon)
        {
            customDelegate?.Invoke();
            return null;
        }
        return StartCoroutine(DelayedCallRoutine(delay, customDelegate, true));
    }

    public Coroutine WaitUntil(Func<bool> untilCondition, CustomDelegate customDelegate = null)
    {
        return StartCoroutine(UntilRoutine(untilCondition, customDelegate));
    }
    
    public void StopAll()
    {
        StopAllCoroutines();
    }

    private IEnumerator UntilRoutine(Func<bool> pred, CustomDelegate customDelegate)
    {
        yield return new WaitUntil(pred);
        customDelegate?.Invoke();
    }
    
    private IEnumerator DelayedCallRoutine(float delay, CustomDelegate customDelegate, bool realtime)
    {
        if (realtime)
        {
            yield return new WaitForSecondsRealtime(delay);
        }
        else
        {
            yield return new WaitForSeconds(delay);
        }
        customDelegate?.Invoke();
    }
}