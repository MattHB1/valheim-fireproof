using HarmonyLib;
using UnityEngine;

namespace Fireproof;

/// <summary>
/// Hearths / campfires still emit HitData (often fire + chop). Blocking the Burning
/// icon is not enough - skip the whole environmental fire hit for the local player
/// so HP, floating numbers, and hit SFX stay quiet.
/// </summary>
[HarmonyPatch(typeof(Character), nameof(Character.ApplyDamage))]
internal static class BlockFireApplyDamagePatch
{
  private static bool Prefix(Character __instance, HitData hit)
  {
    var local = Player.m_localPlayer;
    if (!local || __instance != local || hit == null) return true;

    if (hit.m_hitType == HitData.HitType.Burning
        || hit.m_hitType == HitData.HitType.CinderFire
        || hit.m_hitType == HitData.HitType.Smoke
        || hit.m_hitType == HitData.HitType.AshlandsOcean)
      return false;

    // Fireplace AoEs: no attacker, fire bundled with chop/pickaxe → grey numbers + mining SFX
    if (hit.GetAttacker() == null && hit.m_damage.m_fire > 0f)
      return false;

    hit.m_damage.m_fire = 0f;
    return true;
  }
}

[HarmonyPatch(typeof(Character), "AddFireDamage")]
internal static class BlockAddFireDamagePatch
{
  private static bool Prefix(Character __instance)
  {
    var local = Player.m_localPlayer;
    if (!local || __instance != local) return true;
    return false;
  }
}

/// <summary>
/// AoE.OnHit plays hit effects and damages before Character.ApplyDamage runs.
/// Skip fire AoEs against the local player, and skip the hearth piece hammering
/// itself with fire+chop (TooHard pickaxe ticks while you stand in the flames).
/// </summary>
[HarmonyPatch(typeof(Aoe), "OnHit")]
internal static class BlockFireAoePatch
{
  private static bool Prefix(Aoe __instance, Collider collider)
  {
    if (__instance == null || collider == null) return true;
    if (__instance.m_damage.m_fire <= 0f) return true;

    var go = Projectile.FindHitObject(collider);
    if (!go) return true;

    var local = Player.m_localPlayer;
    if (local)
    {
      var character = go.GetComponent<Character>() ?? go.GetComponentInParent<Character>();
      if (character == local) return false;
    }

    // Flame child AoE repeatedly "TooHard"s its own Fireplace WearNTear (grey 10s + pickaxe).
    if (__instance.m_damage.m_chop > 0f
        && go.GetComponent<WearNTear>() != null
        && __instance.GetComponentInParent<Fireplace>() != null)
      return false;

    return true;
  }
}

/// <summary>
/// Ashlands lava uses a heat meter (not hearth fire HitData). Clear it and skip the
/// damage tick so walking on lava is actually fireproof.
/// </summary>
[HarmonyPatch(typeof(Character), "UpdateLava")]
internal static class BlockLavaUpdatePatch
{
  private static bool Prefix(
    Character __instance,
    ref float ___m_lavaHeatLevel,
    ref float ___m_lavaProximity,
    ref float ___m_lavaHeightFactor)
  {
    var local = Player.m_localPlayer;
    if (!local || __instance != local) return true;

    ___m_lavaHeatLevel = 0f;
    ___m_lavaProximity = 0f;
    ___m_lavaHeightFactor = 0f;
    return false;
  }
}

[HarmonyPatch(typeof(Character), "UpdateAshlandsWater")]
internal static class BlockAshlandsWaterPatch
{
  private static bool Prefix(Character __instance)
  {
    var local = Player.m_localPlayer;
    if (!local || __instance != local) return true;
    return false;
  }
}

[HarmonyPatch(typeof(Character), "UpdateHeatDamage")]
internal static class BlockHeatDamagePatch
{
  private static bool Prefix(Character __instance)
  {
    var local = Player.m_localPlayer;
    if (!local || __instance != local) return true;
    return false;
  }
}
