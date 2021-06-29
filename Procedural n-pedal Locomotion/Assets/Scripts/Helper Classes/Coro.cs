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

    public static void Perp(Transform obj, Vector3 targetPos, float duration, Action callback)
    {
        CoroutineBox box = new CoroutineBox();

        IEnumerator coro = _instance.Perp(box, obj, targetPos, duration, callback);
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
    private IEnumerator Perp(CoroutineBox box, Transform obj, Vector3 targetPos, float duration, Action callback)
    {
        yield return new WaitForEndOfFrame();

        Coroutine self = box.coroutine;
        _runningCoroutines.Add(self);

        Vector3 startingPos = obj.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float step = elapsedTime / duration;

            float xLerp = Mathf.Lerp(startingPos.x, targetPos.x, step);
            float zLerp = Mathf.Lerp(startingPos.z, targetPos.z, step);
            float yParabola = MathParabolic.Parabola(startingPos.y, targetPos.y, step);

            obj.position = new Vector3(xLerp, yParabola, zLerp);

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