using System.Collections.Generic;

namespace BGEditor
{
    public class CmdGroup : IEditorCmd
    {
        public LinkedList<IEditorCmd> cmds = new LinkedList<IEditorCmd>();

        public virtual bool Commit(IChartCore core)
        {
            for (var i = cmds.First; i != null; i = i.Next)
            {
                if (!i.Value.Commit(core))
                {
                    i = i.Previous;
                    while (i != null)
                    {
                        i.Value.Rollback(core);
                        i = i.Previous;
                    }
                    return false;
                }
            }
            return true;
        }

        public virtual bool Rollback(IChartCore core)
        {
            for (var i = cmds.Last; i != null; i = i.Previous)
            {
                if (!i.Value.Rollback(core))
                {
                    i = i.Next;
                    while (i != null)
                    {
                        i.Value.Commit(core);
                        i = i.Next;
                    }
                    return false;
                }
            }
            return true;
        }

        public void Add(IEditorCmd cmd)
        {
            cmds.AddLast(cmd);
        }
    }
}
