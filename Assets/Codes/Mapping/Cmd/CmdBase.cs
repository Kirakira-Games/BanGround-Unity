namespace BGEditor
{
    public interface IEditorCmd
    {
        bool Commit();
        bool Rollback();
    }
}
