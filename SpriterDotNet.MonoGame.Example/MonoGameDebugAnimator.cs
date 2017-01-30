﻿// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace SpriterDotNet.MonoGame.Example
{
    public class MonoGameDebugAnimator : MonoGameAnimator
    {
		private IDictionary<string, IDrawable> boxTextures = new Dictionary<string, IDrawable>();
		private IDrawable pointTexture;

        public MonoGameDebugAnimator(SpriterEntity entity, GraphicsDevice graphicsDevice, IProviderFactory<IDrawable, SoundEffect> providerFactory = null) : base(entity, providerFactory)
        {
			pointTexture = new TextureDrawable(TextureUtil.CreateCircle(graphicsDevice, 5, Color.Cyan));

            if (entity.ObjectInfos != null)
            {
                foreach (SpriterObjectInfo objInfo in entity.ObjectInfos)
                {
                    if (objInfo.ObjectType != SpriterObjectType.Box) continue;
					boxTextures[objInfo.Name] = new TextureDrawable(TextureUtil.CreateRectangle(graphicsDevice, (int)objInfo.Width, (int)objInfo.Height, Color.Cyan));
                }
            }
        }

        protected override void ApplyPointTransform(string name, SpriterObject info)
        {
            if (pointTexture == null) return;
            ApplySpriteTransform(pointTexture, info);
        }

        protected override void ApplyBoxTransform(SpriterObjectInfo objInfo, SpriterObject info)
        {
            ApplySpriteTransform(boxTextures[objInfo.Name], info);
        }
    }
}
