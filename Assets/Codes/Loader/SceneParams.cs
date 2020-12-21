using BanGround.Game.Mods;
using Newtonsoft.Json;

namespace BanGround.Scene.Params
{
    public abstract class SceneParams
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class MappingParams : SceneParams
    {
        public int sid;
        public Difficulty difficulty;
        public BGEditor.IEditorInfo editor = new BGEditor.EditorInfo();
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
    }
}