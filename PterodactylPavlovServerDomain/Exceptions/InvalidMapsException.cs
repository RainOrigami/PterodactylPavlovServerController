using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerDomain.Exceptions;

public class InvalidMapsException : Exception
{
    public InvalidMapsException(GoogleSheetsMapRowModel[] invalidMaps)
    {
        this.InvalidMaps = invalidMaps;
    }

    public GoogleSheetsMapRowModel[] InvalidMaps { get; }
}
