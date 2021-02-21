using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace PersistentWorlds.Logic
{
    public sealed class PersistentColonyLeader : IExposable
    {
        public Pawn Reference;
        
        public string UniqueId;
        public Name Name;

        private Vector2 textureSize;
        private string textureBase64;
        
        public Texture Texture;
        public bool LoadingTexture;

        public bool Set => !UniqueId.NullOrEmpty() && Name != null;

        // Due to the nature of IExposable.
        public PersistentColonyLeader()
        {
        }
        
        public PersistentColonyLeader(Pawn pawn)
        {
            this.Reference = pawn;
            
            this.UniqueId = pawn.GetUniqueLoadID();
            this.Name = pawn.Name;
        }
        
        public void ExposeData()
        {
            Scribe_Values.Look(ref UniqueId, "uniqueID");
            
            Scribe_Deep.Look(ref Name, "name");
            
            this.ExposeTextureData();
        }

        private void ExposeTextureData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && (object) Texture != null)
            {
                switch (Texture)
                {
                    case Texture2D texture2D:
                        textureBase64 = Convert.ToBase64String(texture2D.GetRawTextureData());

                        break;
                    case RenderTexture renderTexture:
                        RenderTexture.active = renderTexture;
                        
                        var texture2DRender = new Texture2D(renderTexture.width, renderTexture.height);
                        texture2DRender.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                        texture2DRender.Apply();
                        
                        textureBase64 = Convert.ToBase64String(texture2DRender.GetRawTextureData());
                        
                        break;
                    default:
                        Log.Error($"{nameof(ExposeTextureData)} Unknown Texture type: {Texture.GetType().FullName}");

                        break;
                }
                
                textureSize = new Vector2(Texture.width, Texture.height);
            }
            
            Scribe_Values.Look(ref textureSize, "textureSize");
            Scribe_Values.Look(ref textureBase64, "texture");
            
            if (Scribe.mode != LoadSaveMode.LoadingVars || textureBase64.NullOrEmpty()) return;

            {
                LoadingTexture = true;
                
                LongEventHandler.QueueLongEvent(delegate
                {
                    var texture2D = new Texture2D((int) textureSize.x, (int) textureSize.y);
                    texture2D.LoadRawTextureData(Convert.FromBase64String(textureBase64));
                    texture2D.Apply();
                    
                    this.Texture = texture2D;

                    LoadingTexture = false;
                }, "", false, null);
            }
        }
    }
}