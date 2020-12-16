using BanGround.Game.Mods;

namespace BanGround.Scene.Params
{
    public class MappingParams
    {
        public BGEditor.IEditorInfo editor = new BGEditor.EditorInfo();
    }

    public class InGameParams
    {
        public string replayPath; // TODO(GEEKiDoS): Implement
        public bool saveReplay = true; // TODO(GEEKiDoS): Implement
        public bool saveRecord = true; // TODO(GEEKiDoS): Implement
        public ModFlag mods = ModFlag.None;
        public float seekPosition = 0f;
    }

    public class ResultParams : InGameParams
    {
        public float scoreMultiplier = 1f;
        
        public ResultParams() { }

        public ResultParams(InGameParams game)
        {
            replayPath = game.replayPath;
            saveRecord = game.saveRecord;
            saveReplay = game.saveReplay;
            mods = game.mods;
            seekPosition = game.seekPosition;
        }
    }
}