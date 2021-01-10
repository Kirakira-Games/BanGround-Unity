using BanGround.Game.Mods;
using Newtonsoft.Json;
using System;

namespace BanGround.Scene.Params
{
    public abstract class SceneParams
    {
        public override abstract string ToString();
    }

    public class MappingParams : SceneParams
    {
        public int sid;
        public Difficulty difficulty;
        public BGEditor.IEditorInfo editor = new BGEditor.EditorInfo();

        public override string ToString()
        {
            return $"sid = {sid}\ndifficulty = {difficulty}\n";
        }
    }

    public class InGameParams : SceneParams
    {
        public int sid;
        public Difficulty difficulty;
        public string replayPath;
        public bool saveReplay = true;
        public bool saveRecord = true;
        public bool isOffsetGuide = false;
        public ModFlag mods = ModFlag.None;
        public float seekPosition = 0f;

        public bool ShouldSaveReplay => saveReplay && string.IsNullOrEmpty(replayPath);
        public bool ShouldSaveRecord => saveRecord && !mods.HasFlag(ModFlag.AutoPlay) && string.IsNullOrEmpty(replayPath);

        public override string ToString()
        {
            return $"sid = {sid}\n" +
                $"difficulty = {difficulty}\n" +
                $"replayPath = {replayPath}\n" +
                $"saveReplay = {saveReplay}\n" +
                $"saveRecord = {saveRecord}\n" +
                $"isOffsetGuide = {isOffsetGuide}\n" +
                $"mods = {Convert.ToString((long)mods, 2)}\n" +
                $"seekPosition = {seekPosition}\n";
        }
    }

    public class ResultParams : InGameParams
    {
        public float scoreMultiplier = 1f;
        
        public ResultParams() { }

        public ResultParams(InGameParams game)
        {
            sid = game.sid;
            difficulty = game.difficulty;
            replayPath = game.replayPath;
            saveRecord = game.saveRecord;
            saveReplay = game.saveReplay;
            isOffsetGuide = game.isOffsetGuide;
            mods = game.mods;
            seekPosition = game.seekPosition;
        }

        public InGameParams ToInGameParams()
        {
            return new InGameParams
            {
                sid = sid,
                difficulty = difficulty,
                replayPath = replayPath,
                saveRecord = saveRecord,
                saveReplay = saveReplay,
                isOffsetGuide = isOffsetGuide,
                mods = mods,
                seekPosition = seekPosition
            };
        }

        public override string ToString()
        {
            return base.ToString() + $"scoreMultiplier = {scoreMultiplier}\n";
        }
    }
}