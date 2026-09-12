using HarmonyLib;

namespace Fireproof;

/// <summary>
/// Reject Burning at the shared SEMan add path so fire / lava / enemies never apply
/// the debuff to the local player. Other players / NPCs are unchanged.
///
/// Important: do not touch SEMan.m_character — it looks public in the publicized
/// compile Libs but is private in the real game DLL (FieldAccessException → soft-lock).
/// </summary>
[HarmonyPatch(typeof(SEMan), nameof(SEMan.Internal_AddStatusEffect))]
internal static class BlockBurningPatch
{
  private static bool Prefix(SEMan __instance, int nameHash)
  {
    if (nameHash != SEMan.s_statusEffectBurning) return true;

    var local = Player.m_localPlayer;
    if (!local) return true;
    if (__instance != local.GetSEMan()) return true;

    return false; // skip original → Burning is never added
  }
}
