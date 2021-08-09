using System.Collections.Generic;

public interface INoteController
{
    int numFuwafuwaNotes { get; set; }
    bool hasFuwafuwaNote { get; }
    bool isFinished { get; }

    Slide CreateSlide(List<GameNoteData> notes);
    void Judge(NoteBase notebase, JudgeResult result, KirakiraTouch touch);
    void OnNoteDestroy(NoteBase note);
    void OnSlideDestroy(Slide slide);
    void OnSyncLineDestroy(NoteSyncLine line);
    void UpdateTouch(KirakiraTouch touch);
}
