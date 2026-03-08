using UnityEngine;

namespace GorillaLibrary.Extensions;

public static class ObjectExtensions
{
    public static bool IsObjectExistent(this Object obj) => obj != null && obj;

    public static bool IsObjectNull(this Object obj) => !obj.IsObjectExistent();
}