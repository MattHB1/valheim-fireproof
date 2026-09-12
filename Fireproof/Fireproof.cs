using BepInEx;
using HarmonyLib;

namespace Fireproof;

[BepInPlugin(GUID, NAME, VERSION)]
public class Fireproof : BaseUnityPlugin
{
  public const string GUID = "matthb1.fireproof";
  public const string NAME = "Fireproof";
  public const string VERSION = "1.0.1";

  private void Awake()
  {
    Logger.LogInfo($"{NAME} {VERSION} loaded - blocking fire damage and Burning on local player.");
    new Harmony(GUID).PatchAll();
  }

  // Clears effects already on the character (e.g. saved while on fire / smoked).
  private void Update()
  {
    var player = Player.m_localPlayer;
    if (!player) return;
    var seman = player.GetSEMan();
    if (seman == null) return;
    if (seman.HaveStatusEffect(SEMan.s_statusEffectBurning))
      seman.RemoveStatusEffect(SEMan.s_statusEffectBurning, true);
    if (seman.HaveStatusEffect(SEMan.s_statusEffectSmoked))
      seman.RemoveStatusEffect(SEMan.s_statusEffectSmoked, true);
  }
}
