using PterodactylPavlovServerDomain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PterodactylPavlovServerDomain;
public static class Utils
{
    public static MapRotationModel? FindMatchingRotation(ServerMapModel[] currentRotation, MapRotationModel[] allRotations)
    {
        var comparer = new ServerMapModelComparer();

        foreach (var rotation in allRotations)
        {
            if (rotation.MapsInRotation == null)
            {
                continue;
            }

            if (rotation.MapsInRotation.Count != currentRotation.Length)
            {
                continue;
            }

            var concatenatedRotation = new List<ServerMapModel>(rotation.MapsInRotation.OrderBy(m => m.Order).Select(m => new ServerMapModel() { GameMode = m.GameMode, MapLabel = m.MapLabel }));
            concatenatedRotation.AddRange(concatenatedRotation);

            for (int i = 0; i < concatenatedRotation.Count - currentRotation.Length + 1; i++)
            {
                var subRotation = concatenatedRotation.GetRange(i, currentRotation.Length);
                if (subRotation.SequenceEqual(currentRotation, comparer))
                    return rotation;
            }
        }

        return null;
    }
}

public class ServerMapModelComparer : IEqualityComparer<ServerMapModel>
{

    public bool Equals(ServerMapModel? x, ServerMapModel? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            return false;

        return x.MapLabel == y.MapLabel && x.GameMode == y.GameMode;
    }

    public int GetHashCode(ServerMapModel obj)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + (obj.MapLabel?.GetHashCode() ?? 0);
            hash = hash * 23 + (obj.GameMode?.GetHashCode() ?? 0);
            return hash;
        }
    }
}
