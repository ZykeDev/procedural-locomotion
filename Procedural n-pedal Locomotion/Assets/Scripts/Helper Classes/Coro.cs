using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coro : MonoBehaviour
{
    private static Coro _instance;
    public static Coro Instance { get { return _instance; } }
    private List<Coroutine> _runningCoroutines = new List<Coroutine>();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    
    void Update()
    {
        Time.timeScale = t;
    }
    public float t = 1;

    public float timescale = 1f;
    public void Slow() => Time.timeScale = timescale;

    #region Calls

    public static void Delay(float time, Action callback)
    {
        CoroutineBox box = new CoroutineBox();

        IEnumerator coro = _instance.Delay(box, time, callback);
        box.coroutine = _instance.StartCoroutine(coro);
    }

    public static void AfterFrame(Action callback)
    {
        CoroutineBox box = new CoroutineBox();

        IEnumerator coro = _instance.AfterFrame(box, callback);
        box.coroutine = _instance.StartCoroutine(coro);
    }

    public static void Perp(Transform obj, Vector3 targetPos, int axis, float stepHeight, float duration, Action callback)
    {
        CoroutineBox box = new CoroutineBox();

        IEnumerator coro = _instance.Perp(box, obj, targetPos, axis, stepHeight, duration, callback);
        box.coroutine = _instance.StartCoroutine(coro);
    }


    #endregion


    #region Coroutines

    private IEnumerator Delay(CoroutineBox box, float duration, Action callback)
    {
        yield return new WaitForEndOfFrame();

        Coroutine self = box.coroutine;
        _runningCoroutines.Add(self);

        yield return new WaitForSecondsRealtime(duration);

        _runningCoroutines.Remove(self);
        callback?.Invoke();
    }

    private IEnumerator AfterFrame(CoroutineBox box, Action callback)
    {
        yield return new WaitForEndOfFrame();

        Coroutine self = box.coroutine;
        _runningCoroutines.Add(self);

        yield return new WaitForEndOfFrame();

        _runningCoroutines.Remove(self);
        callback?.Invoke();
    }

    /// <summary>
    /// Parabolically Interpolates the obj's position towards the target position.
    /// </summary>
    private IEnumerator Perp(CoroutineBox box, Transform obj, Vector3 targetPos, int axisIndex, float stepHeight, float duration, Action callback)
    {
        yield return new WaitForEndOfFrame();

        Coroutine self = box.coroutine;
        _runningCoroutines.Add(self);

        Vector3 startingPos = obj.position;
        Vector3 newPos = obj.position;
        float elapsedTime = 0;

        
        while (elapsedTime <= duration)
        {
            float step = elapsedTime / duration;

            // Perp only over the given axis (usually Y), based on the direction vector
            float parabolicPoint = Interp.Perp(startingPos, targetPos, axisIndex, step, stepHeight);

            // Adjust the parabolicPoint to align with the general locomotion direction (Terrain Escalation)
            float axisDifference = Mathf.Lerp(startingPos[axisIndex], targetPos[axisIndex], step);

            float adjustedPoint = parabolicPoint + axisDifference;

            // Assign the new position axis value
            newPos[axisIndex] = adjustedPoint;
          
            // Lerp normally the other two axes
            for (int i = 0; i < 3; i++)
            {
                if (i != axisIndex)
                {
                    newPos[i] = Mathf.Lerp(startingPos[i], targetPos[i], step);
                }
            }

            // Finally update the position
            obj.position = newPos;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _runningCoroutines.Remove(self);
        callback?.Invoke();
    }

    #endregion


}

public class CoroutineBox
{
    public Coroutine coroutine;
}