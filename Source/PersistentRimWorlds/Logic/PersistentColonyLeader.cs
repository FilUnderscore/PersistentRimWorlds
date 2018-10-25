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

        private Vector2 TextureSize;
        private string TextureBase64;
        
        public Texture Texture;

        public bool Set => !UniqueId.NullOrEmpty() && Name != null;

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
            if (Scribe.mode == LoadSaveMode.Saving && Texture != null)
            {
                switch (Texture)
                {
                    case Texture2D texture2D:
                        TextureBase64 = Convert.ToBase64String(texture2D.GetRawTextureData());

                        break;
                    case RenderTexture renderTexture:
                        RenderTexture.active = renderTexture;
                        
                        var texture2DRender = new Texture2D(renderTexture.width, renderTexture.height);
                        texture2DRender.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                        texture2DRender.Apply();
                        
                        TextureBase64 = Convert.ToBase64String(texture2DRender.GetRawTextureData());
                        
                        break;
                    default:
                        Log.Error($"{nameof(ExposeTextureData)} Unknown Texture type: {Texture.GetType().FullName}");

                        break;
                }
                
                TextureSize = new Vector2(Texture.width, Texture.height);
            }
            
            Scribe_Values.Look(ref TextureSize, "textureSize");
            Scribe_Values.Look(ref TextureBase64, "texture");
            
            if (Scribe.mode != LoadSaveMode.LoadingVars || TextureBase64 == null || TextureBase64.NullOrEmpty()) return;

            {
                LongEventHandler.QueueLongEvent(delegate
                {
                    var texture2D = new Texture2D((int) TextureSize.x, (int) TextureSize.y);
                    texture2D.LoadRawTextureData(Convert.FromBase64String(TextureBase64));

                    this.Texture = texture2D;
                }, "", false, null);
            }
        }
    }
}