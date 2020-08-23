namespace BGEditor
{
    public interface IEditorCmd
    {
        /// <summary>
        /// Commit a command. DO NOT CALL THIS METHOD DIRECTLY! Use Core.Commit(cmd) instead.
        /// </summary>
        /// <returns>Whether the commit is successful.</returns>
        bool Commit(IChartCore core);

        /// <summary>
        /// Rollback a command. DO NOT CALL THIS METHOD DIRECTLY! Use Core.Commit(cmd) instead.
        /// </summary>
        /// <returns>Whether the rollback is successful.</returns>
        bool Rollback(IChartCore core);
    }

    public class FailCommand : IEditorCmd
    {
        public bool Commit(IChartCore core)
        {
            return false;
        }

        public bool Rollback(IChartCore core)
        {
            return false;
        }
    }
}
