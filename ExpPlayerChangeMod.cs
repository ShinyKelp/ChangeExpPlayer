using System;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;


#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]


namespace ExpPlayerChange
{
    [BepInPlugin("ShinyKelp.ExpPlayer1Change", "ExpPlayer1Change", "1.2.1")]
    public class ExpPlayerChangeMod : BaseUnityPlugin
    {
        private void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
        }

        private bool isExpedition = false;

        private bool IsInit;
        private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (IsInit) return;

                //Your hooks go here
                //IL.JollyCoop.JollyMenu.JollyPlayerSelector.Update += JollyPlayerSelector_Update;
                On.JollyCoop.JollyMenu.JollyPlayerSelector.Update += JollyPlayerSelector_Update1;
                IL.Menu.UnlockDialog.TogglePerk += UnlockDialog_TogglePerk;
                IL.Menu.CharacterSelectPage.UpdateSelectedSlugcat += CharacterSelectPage_UpdateSelectedSlugcat;
                On.JollyCoop.JollyMenu.JollyPlayerOptions.ClassAllowsChangingPlayerOne += JollyPlayerOptions_ClassAllowsChangingPlayerOne;
                On.JollyCoop.JollyMenu.JollyPlayerSelector.ctor += JollyPlayerSelector_ctor;
                IsInit = true;
                UnityEngine.Debug.Log("Expedition Player 1 Change hooks finished successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }

        private void JollyPlayerSelector_ctor(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_ctor orig, JollyCoop.JollyMenu.JollyPlayerSelector self, JollyCoop.JollyMenu.JollySetupDialog menu, Menu.MenuObject owner, UnityEngine.Vector2 pos, int index)
        {
            isExpedition = menu.manager.rainWorld.ExpeditionMode;
            orig(self, menu, owner, pos, index);
        }

        private bool JollyPlayerOptions_ClassAllowsChangingPlayerOne(On.JollyCoop.JollyMenu.JollyPlayerOptions.orig_ClassAllowsChangingPlayerOne orig, SlugcatStats.Name name)
        {
            if (ModManager.Expedition && isExpedition)
                return true;
            else return orig(name);
        }

        private void JollyPlayerSelector_Update1(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_Update orig, JollyCoop.JollyMenu.JollyPlayerSelector self)
        {
            orig(self);
            if (self.index == 0 && ModManager.Expedition && self.menu.manager.rainWorld.ExpeditionMode)
            {
                self.classButton.GetButtonBehavior.greyedOut = false;
            }
        }

        private void CharacterSelectPage_UpdateSelectedSlugcat(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld<ModManager>("JollyCoop"));
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_I4_0);
        }

        private void UnlockDialog_TogglePerk(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(0),
                x => x.MatchLdsfld<MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName>("Artificer"));
            c.Index++;
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);

            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(0),
                x => x.MatchLdsfld<SlugcatStats.Name>("Red"));
            c.Index++;
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);
            
            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(0),
                x => x.MatchLdsfld<MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName>("Spear"));
            c.Index++;
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);

            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(0),
                x => x.MatchLdsfld<MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName>("Rivulet"));
            c.Index++;
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);

        }

        // Obsolete: Does not work as for Watcher 1.5
        // Reason unknown.

        // Code and explanation by forthbridge
        // Skip the 'if (this.index == 0 && ModManager.Expedition && this.menu.manager.rainWorld.ExpeditionMode)' condition
        private static void JollyPlayerSelector_Update(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            // Navigate to the place we want to add instructions
            // We do this by matching existing IL instructions
            c.GotoNext(MoveType.After,
                x => x.MatchStfld<Menu.ButtonBehavior>(nameof(Menu.ButtonBehavior.greyedOut)),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<JollyCoop.JollyMenu.JollyPlayerSelector>(nameof(JollyCoop.JollyMenu.JollyPlayerSelector.index)));

            // The following will effectively turn the statement into '(false) && ModManager.Expedition && this.menu.manager.rainWorld.ExpeditionMode'

            c.Emit(OpCodes.Pop);    // We remove the index value
            c.Emit(OpCodes.Ldc_I4_1); // Push 1 (true) onto the stack
            
            // The reason we emit true is because the following IL instruction:
            // Brtrue.s
            // Will branch to the target instruction if the current value on the stack is true
            // The target here is after the if statement, so because we always emit true, the if statement is always skipped
        }

    }
}
