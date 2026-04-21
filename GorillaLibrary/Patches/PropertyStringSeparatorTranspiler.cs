using GorillaGameModes;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GorillaLibrary.Patches;

[HarmonyPatch(typeof(GameModeString), "FromString")]
internal class PropertyStringSeparatorTranspiler
{
    public static string Separator;

    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        if (string.IsNullOrEmpty(Separator) || string.IsNullOrWhiteSpace(Separator))
        {
            CodeInstruction[] instructionArray = [.. instructions];

            for (int i = 0; i < instructionArray.Length; i++)
            {
                if (instructionArray[i].opcode == OpCodes.Ldstr)
                {
                    string separator = instructionArray[i].operand.ToString();
                    if (separator != null && separator.Length == 1)
                    {
                        Separator = separator;
                        break;
                    }
                }
            }
        }

        return instructions;
    }
}
