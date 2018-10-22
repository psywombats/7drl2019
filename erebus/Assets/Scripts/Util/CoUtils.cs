using UnityEngine;
using System.Collections;
using System;

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
            runner.StartCoroutine(RunWithCallback(coroutine, runner, () => {
                running -= 1;
            }));
        }
        while (running > 0) {
            yield return null;
        }
    }

    public static IEnumerator RunWithCallback(IEnumerator coroutine, MonoBehaviour runner, Action toRun) {
        yield return runner.StartCoroutine(coroutine);
        toRun();
    }

    public static IEnumerator Wait(float seconds) {
        yield return new WaitForSeconds(seconds);
    }
}
