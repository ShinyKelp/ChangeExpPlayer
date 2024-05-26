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
    [BepInPlugin("ShinyKelp.ExpPlayer1Change", "ExpPlayer1Change", "1.2.0")]
    public class ExpPlayerChangeMod : BaseUnityPlugin
    {
        private void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
        }

        private bool IsInit;
        private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (IsInit) return;

                //Your hooks go here
                IL.JollyCoop.JollyMenu.JollyPlayerSelector.Update += JollyPlayerSelector_Update;
                IL.Menu.UnlockDialog.TogglePerk += UnlockDialog_TogglePerk;
                IL.Menu.CharacterSelectPage.UpdateSelectedSlugcat += CharacterSelectPage_UpdateSelectedSlugcat; 
                IsInit = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
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

            // The following will effectively turn the statement into '(index == 0 && false) && ModManager.Expedition && this.menu.manager.rainWorld.ExpeditionMode'

            c.Emit(OpCodes.Ldc_I4_1); // Push 1 (true) onto the stack
            c.Emit(OpCodes.Or); // Check if either of the last 2 values on the stack are true (the one we just pushed will always be true)

            // The reason we emit true is because the following IL instruction:
            // Brtrue.s
            // Will branch to the target instruction if the current value on the stack is true
            // The target here is after the if statement, so because we always emit true, the if statement is always skipped
        }

    }
}
