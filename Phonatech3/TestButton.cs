using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;

namespace Phonatech3
{
    public class AddTower : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        public AddTower()
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

            TowerManager tm = new TowerManager(pDS.Workspace);
            Tower tower = tm.GetNearestTower(pPoint);

            if (tower == null)
            {
                MessageBox.Show("No towers were found within the area.");
                return;
            }

            MessageBox.Show("Tower ID: " + tower.ID + Environment.NewLine + "Type: " + tower.towerType + Environment.NewLine + "NetworkBand: " + tower.networkBand);

            //IPoint pPoint = (IPoint) pMxdoc.ActivatedView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

            //MessageBox.Show("Mouse point is x: " + x + Environment.NewLine + "y: " + y + Environment.NewLine + "Mapx: " + pPoint.X + Environment.NewLine + "mapy: " + pPoint.Y);
            ///base.OnMouseUp(arg);
        }
    }

}
