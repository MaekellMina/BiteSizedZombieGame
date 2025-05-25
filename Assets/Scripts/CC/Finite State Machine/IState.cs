
namespace cc.FiniteStateMachine
{
    public interface IState
    {
        void Tic();
        void OnEnter();
        void OnExit();

    }

    public interface IPlayerState:IState
    {
        void OnPhysicsTic();
    }
}
