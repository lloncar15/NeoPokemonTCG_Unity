using System;
using System.Collections;
using System.Collections.Generic;
using GimGim.AspectContainer;
using GimGim.EventSystem;

namespace GimGim.ActionSystem {
    public interface IGameAction {
        int Priority { get; set; }
        int OrderOfPlay { get; set; }
        bool IsCanceled { get; }
        List<GameActionPhase> Phases { get; }

        void Cancel();
        GameActionPhase GetPhase(GameActionPhaseType phaseType);
    }
}