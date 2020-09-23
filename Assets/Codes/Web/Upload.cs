﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UniRx.Async;
using UnityEngine;

namespace Web.Upload
{
    public enum UploadType
    {
        Misc,
        Music,
        Chart,
        Image
    }

    public static class Util
    {
        public static string Hash(byte[] content)
        {
            return Auth.Util.ToHex(SHA256.Create().ComputeHash(content));
        }

        public static string MimetypeOf(string filename)
        {
            string ext = Path.GetExtension(filename).ToLower();
            switch (ext) {
                case ".ogg": return "audio/ogg";
                case ".png": return "image/png";
                case ".jpg": return "image/jpg";
                case ".jpeg": return "image/jpeg";
                default: return "application/octet-stream";
            }
        }
    }

    public class File
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("uploader")]
        public Auth.UserLite Uploader;

        [JsonProperty("size")]
        [JsonConverter(typeof(LongToStringConverter))]
        public long Size;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("hash")]
        public string Hash;

        [JsonProperty("url")]
        public string Url;
    }

    public class FishDelta
    {
        [JsonProperty("fish")]
        [JsonConverter(typeof(LongToStringConverter))]
        public long Fish;

        [JsonProperty("required")]
        [JsonConverter(typeof(LongToStringConverter))]
        public long Required;
    }

    public class CalcResult : FishDelta
    {
        [JsonProperty("duplicate")]
        public List<bool> Duplicates = new List<bool>();
    }

    public class UploadResponse
    {
        [JsonProperty("file")]
        public File File;

        [JsonProperty("fish")]
        public FishDelta Fish;
    }
    public class FileHashSize
    {
        [JsonProperty("hash")]
        public string Hash;

        [JsonProperty("size")]
        public long Size;
    }

    public class CalcUploadCostRequest
    {
        [JsonProperty("files")]
        public List<FileHashSize> files = new List<FileHashSize>();
    }

    public class ClaimFilesRequest
    {
        [JsonProperty("files")]
        public List<string> files = new List<string>();
    }

    public static class Extension
    {
        public static KiraWebRequest.Builder<CalcResult> CalcUploadCost(this IKiraWebRequest web, List<FileHashSize> files)
        {
            return web.New<CalcResult>().SetReq(
                new CalcUploadCostRequest { files = files })
                .UseTokens().Post("upload/calc");
        }

        public static KiraWebRequest.Builder<FishDelta> ClaimFiles(this IKiraWebRequest web, List<string> hashes)
        {
            return web.New<FishDelta>().SetReq(
                new ClaimFilesRequest { files = hashes }
                ).UseTokens().Post("upload/claim");
        }

        public static KiraWebRequest.Builder<UploadResponse> UploadFile(this IKiraWebRequest web, UploadType type, string filename, byte[] content)
        {
            string typeStr = type.ToString().ToLower();
            string mimetype = Util.MimetypeOf(filename);
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", content, filename, mimetype);
            Debug.Log($"[KWR] Upload: {filename} ({mimetype})");
            return web.New<UploadResponse>().SetForm(form).UseTokens().SetTimeout(300).Post("upload/" + typeStr);
        }
    }
}