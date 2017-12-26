using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geodatabase;

namespace Phonatech3
{
    public class AddDevice : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        public AddDevice()
        {
        }

        protected override void OnUpdate()
        {

        }

        protected override void OnMouseUp(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
            int x = arg.X;
            int y = arg.Y;

            IMxDocument pMxdoc = (IMxDocument)ArcMap.Application.Document;
            ////IWorkspace pWorkspace

            IFeatureLayer pFeatureLayer = (IFeatureLayer)pMxdoc.ActiveView.FocusMap.Layer[0];
            IDataset pDS = (IDataset)pFeatureLayer.FeatureClass;

            IPoint pPoint = pMxdoc.ActivatedView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
           
            DeviceManager dm = new DeviceManager(pDS.Workspace);
            dm.AddDevice("D01", pPoint);
            
            pMxdoc.ActivatedView.Refresh();            
        }
    }

}
