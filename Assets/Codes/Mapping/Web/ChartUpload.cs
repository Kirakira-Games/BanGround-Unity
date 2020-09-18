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

        [Inject]
        IKiraWebRequest web;

        public void UploadChart(int cid)
        {
            var source = IDRouterUtil.GetSource(cid, out int id);
            if (source == ChartSource.Local)
            {

            }
        }
    }
}