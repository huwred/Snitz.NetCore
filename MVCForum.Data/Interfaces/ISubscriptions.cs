namespace SnitzCore.Data.Interfaces
{
    public interface ISubscriptions
    {
        void Topic(int id);
        void Reply(int id);
    }
}
