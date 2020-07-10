using UnityEngine;
using System.Collections;

public static class ChartVersion
{
    const int VERSION = 1;

    public static bool CanRead(int version)
    {
        return version == VERSION;
    }

    public static bool CanConvert(int version)
    {
        return version == 1;
    }

    public static bool Process(cHeader header, Chart _)
    {
        if (!CanRead(header.version))
        {
            if (!CanConvert(header.version))
            {
                return false;
            }
            // TODO: Convert
        }
        return true;
    }
}
