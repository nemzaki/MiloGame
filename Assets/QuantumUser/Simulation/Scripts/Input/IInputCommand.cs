namespace Quantum
{
    internal interface IInputCommand
    {
        void Process(Frame frame, EntityRef entity, ref InputDesires inputDesires);
    }
}