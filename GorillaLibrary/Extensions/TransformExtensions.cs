using UnityEngine;

namespace GorillaLibrary.Extensions;

public static class TransformExtensions
{
    public static Transform FindChildRecursive(this Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name.Contains(name)) return child;
            Transform transform = child.FindChildRecursive(name);
            if (transform.IsObjectExistent()) return transform;
        }

        return null;
    }
}
