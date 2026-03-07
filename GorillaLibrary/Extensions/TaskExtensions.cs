using GorillaLocomotion;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaLibrary.Extensions;

public static class TaskExtensions
{
    private static MonoBehaviour Behaviour => GTPlayer.Instance;

    public static async Task AsAwaitable(this YieldInstruction instruction)
    {
        var completionSource = new TaskCompletionSource<YieldInstruction>();
        IEnumerator coroutine = AwaitableRoutine(instruction, completionSource);
        Behaviour.StartCoroutine(coroutine);
        await completionSource.Task;
        Behaviour.StopCoroutine(coroutine);
    }

    public static async Task AsAwaitable(this CustomYieldInstruction instruction)
    {
        var completionSource = new TaskCompletionSource<CustomYieldInstruction>();
        IEnumerator coroutine = AwaitableRoutine(instruction, completionSource);
        Behaviour.StartCoroutine(coroutine);
        await completionSource.Task;
        Behaviour.StopCoroutine(coroutine);
    }

    private static IEnumerator AwaitableRoutine(YieldInstruction instruction, TaskCompletionSource<YieldInstruction> completionSource)
    {
        yield return instruction;
        completionSource.SetResult(instruction);
    }

    private static IEnumerator AwaitableRoutine(CustomYieldInstruction instruction, TaskCompletionSource<CustomYieldInstruction> completionSource)
    {
        yield return instruction;
        completionSource.SetResult(instruction);
    }
}
