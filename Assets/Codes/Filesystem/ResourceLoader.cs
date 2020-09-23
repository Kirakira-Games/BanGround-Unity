﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BanGround
{
    public static class ResourceLoader
    {
        public static string ReadAsString(this IFile file)
        {
            return Encoding.UTF8.GetString(file.ReadToEnd());
        }

        public static Texture2D ReadAsTexture(this IFile file)
        {
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(file.ReadToEnd());
            tex.wrapMode = TextureWrapMode.Mirror;

            return tex;
        }
    }
}