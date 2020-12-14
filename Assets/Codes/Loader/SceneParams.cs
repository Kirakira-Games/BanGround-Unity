namespace BanGround.Scene.Params
{
    public class MappingParams
    {
        public BGEditor.IEditorInfo editor = new BGEditor.EditorInfo();
    }

    public class InGameParams
    {
        public string replayPath;
        public ulong mods = 0;
        public float seekPosition = 0f;
    }
}