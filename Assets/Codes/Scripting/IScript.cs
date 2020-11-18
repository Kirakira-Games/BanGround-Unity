using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanGround.Scripting
{
    public interface IScript
    {
        bool HasOnUpdate { get; }
        bool HasOnJudge { get; }
        bool HasOnBeat { get; }

        void Init(int sid, Difficulty difficulty);
        void OnUpdate(int audioTime);
        void OnJudge(JudgeResult result);
        void OnBeat(float beat);
    }
}
