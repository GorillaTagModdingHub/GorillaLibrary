using MelonLoader;
using System;
using System.Threading;

namespace GorillaLibrary.Utilities;

public static class ThreadUtility
{
    public static void StartSyncMethod(Action action)
    {
        Mod._unityAction = (Action)Delegate.Combine(Mod._unityAction, action);
    }

    public static void StartAsyncMethod(Action action)
    {
        if (!ThreadPool.QueueUserWorkItem(Method)) return;

        void Method(object _)
        {
            try
            {
                action.Invoke();
            }
            catch(Exception ex)
            {
                Melon<Mod>.Logger.Error(ex);
            }
        }
    }
}
