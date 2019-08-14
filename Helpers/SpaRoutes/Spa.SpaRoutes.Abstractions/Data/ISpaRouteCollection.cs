namespace Spa.SpaRoutes.Abstractions
{
    public interface ISpaRouteCollection : ISpaRoute
    {
        void Add(ISpaRoute route);
    }
}
