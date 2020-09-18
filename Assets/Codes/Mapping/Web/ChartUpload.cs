using UnityEngine;
using System.Collections.Generic;
using Zenject;
using Web;

namespace BGEditor
{
    public class ChartUpload
    {
        [Inject]
        IDataLoader dataLoader;

        public int PreUploadChart(int cid)
        {

        }
    }
}