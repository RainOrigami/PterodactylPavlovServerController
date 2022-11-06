using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerDomain.Exceptions
{
    public class InvalidMapsException : Exception
    {
        public InvalidMapsException(MapRowModel[] invalidMaps)
        {
            this.InvalidMaps = invalidMaps;
        }

        public MapRowModel[] InvalidMaps { get; }
    }
}
