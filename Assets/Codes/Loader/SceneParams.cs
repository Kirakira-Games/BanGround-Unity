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
        public ulong mods = 0;
        public float seekPosition = 0f;
    }
}