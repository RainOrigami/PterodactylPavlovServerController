using PterodactylPavlovServerController.Models;

namespace PterodactylPavlovServerController.Exceptions
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
