using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;


namespace Phonatech3
{
    public class GenerateTowerRanges : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public GenerateTowerRanges()
        {
        }

        protected override void OnClick()
        {
            IMxDocument pMxdoc = (IMxDocument)ArcMap.Application.Document;
            ////IWorkspace pWorkspace
            IFeatureLayer pFeatureLayer = (IFeatureLayer)pMxdoc.ActiveView.FocusMap.Layer[0];
            IDataset pDS = (IDataset)pFeatureLayer.FeatureClass;

            TowerManager tm = new TowerManager(pDS.Workspace);

            Tower pTower = tm.GetTowerByID("T04");
            
            int towerRange = 100;
            ITopologicalOperator pTopo = (ITopologicalOperator)pTower.towerLocation;
            IPolygon range3Bars = (IPolygon)pTopo.Buffer(towerRange / 3);
            
            IPolygon range2BarsWhole = (IPolygon)pTopo.Buffer(towerRange * 2 / 3);
            ITopologicalOperator p2BarTopo = (ITopologicalOperator)range2BarsWhole;
            IPolygon range2BarsDonut = (IPolygon) p2BarTopo.Difference(range3Bars);

            IPolygon range1BarWhole = (IPolygon)pTopo.Buffer(towerRange);
            ITopologicalOperator p1BarTopo = (ITopologicalOperator)range1BarWhole;
            IPolygon range1BarDonut = (IPolygon)p1BarTopo.Difference(range2BarsWhole);

            IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)pDS.Workspace;
            pWorkspaceEdit.StartEditing(true);
            pWorkspaceEdit.StartEditOperation();

            IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)pWorkspaceEdit;
            IFeatureClass pTowerRangeFC = pFWorkspace.OpenFeatureClass("TowerRanges");

            IFeature pFeature = pTowerRangeFC.CreateFeature();
            pFeature.set_Value(pFeature.Fields.FindField("TOWERID"),"T04");
            pFeature.set_Value(pFeature.Fields.FindField("RANGE"), 3);
            pFeature.Shape = range3Bars;
            pFeature.Store();

            IFeature p2BarsFeature = pTowerRangeFC.CreateFeature();
            p2BarsFeature.set_Value(pFeature.Fields.FindField("TOWERID"), "T04");
            p2BarsFeature.set_Value(pFeature.Fields.FindField("RANGE"), 2);
            p2BarsFeature.Shape = range2BarsDonut;
            p2BarsFeature.Store();

            IFeature p1BarsFeature = pTowerRangeFC.CreateFeature();
            p1BarsFeature.set_Value(pFeature.Fields.FindField("TOWERID"), "T04");
            p1BarsFeature.set_Value(pFeature.Fields.FindField("RANGE"), 1);
            p1BarsFeature.Shape = range1BarDonut;
            p1BarsFeature.Store();

            pWorkspaceEdit.StopEditOperation();
            pWorkspaceEdit.StopEditing(true);
        }

        protected override void OnUpdate()
        {
        }
    }
}
