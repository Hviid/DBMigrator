namespace DBMigrator.Middleware
{
    public interface IMiddleware
    {
        void Init(Middleware middleware);
    }
}