using UnityEngine;
using System.Collections.Generic;

namespace Web
{
    public enum ChartSource
    {
        Local,
        BanGround,
        Burrito,
        None
    }

    public static class IDRouterUtil
    {
        /// <summary>
        /// Number of lower bits to use as the chart ID bit.
        /// </summary>
        private const int ID_BIT = 20;
        private const int ID_MASK = (1 << ID_BIT) - 1;

        /// <summary>
        /// Number of chart sources.
        /// </summary>
        private const int SOURCE_NUM = (int)ChartSource.None;

        public static bool IsFromUnixTimestamp(int id)
        {
            return id >= 1000000000;
        }

        /// <summary>
        /// Find the source and original ID of a song or chart.
        /// </summary>
        /// <param name="id">Raw ID.</param>
        /// <param name="originalId">Original ID in the corresponsind server.</param>
        /// <returns></returns>
        public static ChartSource GetSource(int id, out int originalId)
        {
            originalId = id;
            if (IsFromUnixTimestamp(id))
                return ChartSource.Local;
            int index = id >> ID_BIT;
            originalId = id & ID_MASK;
            return index >= SOURCE_NUM ? ChartSource.None : (ChartSource)index;
        }

        /// <summary>
        /// Convert an ID from a specific source to the file system ID. 
        /// </summary>
        /// <param name="source">The source of the file.</param>
        /// <param name="id">The ID from the specified source.</param>
        /// <returns></returns>
        public static int ToFileId(ChartSource source, int id)
        {
            if (source == ChartSource.None || source == ChartSource.Local)
                return id;
            return ((int)source << ID_BIT) | id;
        }
    }
}