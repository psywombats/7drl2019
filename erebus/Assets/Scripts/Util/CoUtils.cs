using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;

public class CoUtils {
    
    public static IEnumerator RunAfterDelay(float delayInSeconds, Action toRun) {
        yield return new WaitForSeconds(delayInSeconds);
        toRun();
    }

    public static IEnumerator Delay(float delayInSeconds, IEnumerator toRun) {
        yield return Wait(delayInSeconds);
        yield return toRun;
    }

    public static IEnumerator RunParallel(IEnumerator[] coroutines, MonoBehaviour runner) {
        int running = coroutines.Length;
        foreach (IEnumerator coroutine in coroutines) {
            runner.StartCoroutine(RunWithCallback(coroutine, () => {
                running -= 1;
            }));
        }
        while (running > 0) {
            yield return null;
        }
    }

    public static IEnumerator RunSequence(IEnumerator[] coroutines) {
        foreach (IEnumerator routine in coroutines) {
            yield return routine;
        }
    }

    public static IEnumerator RunWithCallback(IEnumerator coroutine, Action toRun) {
        yield return coroutine;
        toRun?.Invoke();
    }

    public static IEnumerator Wait(float seconds) {
        yield return new WaitForSeconds(seconds);
    }

    public static IEnumerator RunTween(Tweener tween) {
        bool done = false;
        tween.Play().onComplete = () => {
            done = true;
        };
        while (!done) {
            yield return null;
        }
    }
}
