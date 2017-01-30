﻿// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using SpriterDotNet.Providers;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SpriterDotNet.MonoGame
{
    public class SpriterContentLoader
    {
        public Spriter Spriter { get; private set; }

        private readonly ContentManager content;
        private readonly string scmlPath;
        private readonly string rootPath;

        private Dictionary<int, SpriterAtlas> atlases;
        private Dictionary<SpriterAtlas, Dictionary<string, ImageInfo>> infos;

        public SpriterContentLoader(ContentManager content, string scmlPath)
        {
            this.content = content;
            this.scmlPath = scmlPath;
            rootPath = scmlPath.Substring(0, scmlPath.LastIndexOf("/"));
            Load();
        }

        public void Fill(DefaultProviderFactory<IDrawable, SoundEffect> factory)
        {
            foreach (SpriterFolder folder in Spriter.Folders)
            {
                if (atlases != null && atlases.Count > 0) AddAtlasFolder(folder, factory);
                else AddRegularFolder(folder, factory);
            }
        }

        private void AddRegularFolder(SpriterFolder folder, DefaultProviderFactory<IDrawable, SoundEffect> factory)
        {
            foreach (SpriterFile file in folder.Files)
            {
                string path = FormatPath(file.Name, folder.Name);

                if (file.Type == SpriterFileType.Sound)
                {
                    SoundEffect sound = LoadContent<SoundEffect>(path);
                    factory.SetSound(Spriter, folder, file, sound);
                }
                else
                {
                    Texture2D texture = LoadContent<Texture2D>(path);
                    TextureDrawable sprite = new TextureDrawable(texture);
                    factory.SetSprite(Spriter, folder, file, sprite);
                }

            }
        }

        private void AddAtlasFolder(SpriterFolder folder, DefaultProviderFactory<IDrawable, SoundEffect> factory)
        {
            int id = folder.AtlasId;
            if (id < 0) id = 0;
            SpriterAtlas atlas = atlases[id];
            Texture2D texture = content.Load<Texture2D>(FormatPath(atlas.Meta.Image));
            Dictionary<string, ImageInfo> imageInfos = infos[atlas];

            foreach (SpriterFile file in folder.Files)
            {
                ImageInfo info = imageInfos[file.Name];

                // "x", "y" = location in spritesheet, "w", "h" = trimmed unrotated image size
                Size frame = info.Frame;

                // "w", "h" = original image size﻿
                Size source = info.SourceSize;

                // "x", "y" = trimmed offset - pixels trimmed from the top and left
                Size spriteSource = info.SpriteSourceSize;

                Rectangle sourceRectangle;
                bool rotated = false;

                if (info.Rotated)
                {
                    sourceRectangle = new Rectangle(frame.X, frame.Y, frame.H, frame.W);
                    rotated = true;
                }
                else
                {
                    sourceRectangle = new Rectangle(frame.X, frame.Y, frame.W, frame.H);
                }

                float trimLeft = spriteSource.X;
                float trimRight = source.W - frame.W - spriteSource.X;
                float trimTop = spriteSource.Y;
                float trimBottom = source.H - frame.H - spriteSource.Y;

                int width = source.W;
                int height = source.H;

                TexturePackerDrawable sprite = new TexturePackerDrawable(texture, sourceRectangle, width, height, rotated, trimLeft, trimRight, trimTop, trimBottom);

                factory.SetSprite(Spriter, folder, file, sprite);
            }
        }

        private void Load()
        {
            Spriter = LoadContent<Spriter>(scmlPath);
            if (Spriter.Atlases == null || Spriter.Atlases.Length == 0) return;
            atlases = new Dictionary<int, SpriterAtlas>();
            infos = new Dictionary<SpriterAtlas, Dictionary<string, ImageInfo>>();

            foreach (var atlasRef in Spriter.Atlases)
            {
                String path = FormatPath(atlasRef.Name);
                SpriterAtlas atlas = content.Load<SpriterAtlas>(path);
                atlases[atlasRef.Id] = atlas;

                Dictionary<string, ImageInfo> imageInfos = new Dictionary<string, ImageInfo>();
                infos[atlas] = imageInfos;

                foreach (ImageInfo info in atlas.ImageInfos) imageInfos[info.Name] = info;
            }
        }

        private string FormatPath(string fileName, string folderName = null)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            if (string.IsNullOrEmpty(folderName)) return string.Format("{0}/{1}", rootPath, fileName);
            return string.Format("{0}/{1}/{2}", rootPath, folderName, fileName);
        }

        private T LoadContent<T>(string path)
        {
            T asset = default(T);
            try
            {
                asset = content.Load<T>(path);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Missing Asset: " + path);
            }

            return asset;
        }
    }
}

