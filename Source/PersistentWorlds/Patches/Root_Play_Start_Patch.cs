using System;
using System.IO;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PersistentWorlds.Patches
{
    [HarmonyPatch(typeof(Root_Play), "Start")]
    public static class Root_Play_Start_Patch
    {
        [HarmonyPrefix]
        public static bool Start_Prefix(Root_Play __instance)
        {
            //Log.ResetMessageCount();
            Base_Start(__instance);

            try
			{
				__instance.musicManagerPlay = new MusicManagerPlay();
				var checkedAutostartSaveFile = AccessTools.Field(typeof(Root), "checkedAutostartSaveFile");
				FileInfo autostart = (!(bool)checkedAutostartSaveFile.GetValue(null)) ? SaveGameFilesUtility.GetAutostartSaveFile() : null;
				//Root.checkedAutostartSaveFile = true;
				checkedAutostartSaveFile.SetValue(null, true);
				if (autostart != null)
				{
					Log.Message("Possibility 1");
					
					Action action = delegate()
					{
						SavedGameLoaderNow.LoadGameFromSaveFileNow(Path.GetFileNameWithoutExtension(autostart.Name));
					};
					string textKey = "LoadingLongEvent";
					bool doAsynchronously = true;
					//if (Root_Play.<>f__mg$cache0 == null)
					//{
					//	Root_Play.<>f__mg$cache0 = new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileLoadingGame);
					//}

					var exception = new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileLoadingGame);
					LongEventHandler.QueueLongEvent(action, textKey, doAsynchronously, exception);
				}
				else if (Find.GameInitData != null && !Find.GameInitData.gameToLoad.NullOrEmpty())
				{
					Log.Message("Possibility 2");
					
					Action action2 = delegate()
					{
						SavedGameLoaderNow.LoadGameFromSaveFileNow(Find.GameInitData.gameToLoad);
					};
					string textKey2 = "LoadingLongEvent";
					bool doAsynchronously2 = true;
					var exception = new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileLoadingGame);
					LongEventHandler.QueueLongEvent(action2, textKey2, doAsynchronously2, exception);
				}
				else
				{
					Log.Message("Possibility 3");
					
					Action action3 = delegate()
					{
						if (Current.Game == null)
						{
							//Root_Play.SetupForQuickTestPlay();
							AccessTools.Method(typeof(Root_Play), "SetupForQuickTestPlay").Invoke(null, new object[0]);
						}
						Current.Game.InitNewGame();
					};
					string textKey3 = "GeneratingMap";
					bool doAsynchronously3 = true;
					var exception = new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileLoadingGame);
					LongEventHandler.QueueLongEvent(action3, textKey3, doAsynchronously3, exception);
				}
				LongEventHandler.QueueLongEvent(delegate()
				{
					ScreenFader.SetColor(Color.black);
					ScreenFader.StartFade(Color.clear, 0.5f);
				}, null, false, null);
			}
			catch (Exception arg)
			{
				Log.Error("Critical error in root Start(): " + arg, false);
			}
            
            return false;
        }

        private static void Base_Start(Root_Play __instance)
        {
            try
            {
                CultureInfoUtility.EnsureEnglish();
                Current.Notify_LoadedSceneChanged();
                //Root.CheckGlobalInit();
                AccessTools.Method(typeof(Root), "CheckGlobalInit").Invoke(null, new object[0]);
                Action action = delegate()
                {
                    __instance.soundRoot = new SoundRoot();
                    if (GenScene.InPlayScene)
                    {
                        __instance.uiRoot = new UIRoot_Play();
                    }
                    else if (GenScene.InEntryScene)
                    {
                        __instance.uiRoot = new UIRoot_Entry();
                    }
                    __instance.uiRoot.Init();
                    Messages.Notify_LoadedLevelChanged();
                    if (Current.SubcameraDriver != null)
                    {
                        Current.SubcameraDriver.Init();
                    }
                };
                if (!PlayDataLoader.Loaded)
                {
                    LongEventHandler.QueueLongEvent(delegate()
                    {
                        PlayDataLoader.LoadAllPlayData(false);
                    }, null, true, null);
                    LongEventHandler.QueueLongEvent(action, "InitializingInterface", false, null);
                }
                else
                {
                    action();
                }
            }
            catch (Exception arg)
            {
                Log.Error("Critical error in root Start(): " + arg, false);
            }
        }
    }

	[HarmonyPatch(typeof(WorldGrid), "ExposeData")]
	public static class WorldGrid_ExposeData_Temp_Patch
	{
		[HarmonyPrefix]
		public static void Patch(WorldGrid __instance)
		{
			Log.Message("Deflate Mdoe??? " + Scribe.mode.ToString());
		}
	}
}