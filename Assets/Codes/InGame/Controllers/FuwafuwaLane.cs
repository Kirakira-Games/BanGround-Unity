using UnityEngine;
using System.Collections;
using Zenject;

public interface IInitable
{
    void Init();
}

public class FuwafuwaLane : MonoBehaviour, IInitable
{
    public Animator animator;
    private int Show;
    private int Hide;
    [Inject]
    private INoteController noteController;

    private void Awake()
    {
        Show = Animator.StringToHash("Show");
        Hide = Animator.StringToHash("Hide");
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private bool IsShowing()
    {
        var state = animator.GetCurrentAnimatorStateInfo(0);
        return state.shortNameHash == Show;
    }

    public void Init()
    {
        var pos = NoteController.mainCamera.WorldToScreenPoint(transform.position);
        pos.x = 0;
        transform.position = NoteController.mainCamera.ScreenToWorldPoint(pos);
    }

    void Update()
    {
        if (noteController.hasFuwafuwaNote)
        {
            if (!IsShowing())
            {
                animator.Play(Show, 0);
            }
        }
        else
        {
            if (IsShowing())
            {
                animator.Play(Hide, 0);
            }
        }
    }
}
