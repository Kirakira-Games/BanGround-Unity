namespace BGEditor
{
    public interface IEditorCmd
    {
        /// <returns>Whether the commit is successful.</returns>
        bool Commit(ChartCore core);

        /// <returns>Whether the rollback is successful.</returns>
        bool Rollback(ChartCore core);
    }

    public class FailCommand : IEditorCmd
    {
        public bool Commit(ChartCore core)
        {
            return false;
        }

        public bool Rollback(ChartCore core)
        {
            return false;
        }
    }
}
