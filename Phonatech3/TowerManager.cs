using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Phonatech3
{
    public class TowerManager
    {
        private IWorkspace _workspace;
        private DataTable _towerDetails;

        public TowerManager(IWorkspace pWorkspace)
        {
            _workspace = pWorkspace;

            _towerDetails = new DataTable();
            _towerDetails.Columns.Add("TowerType");
            _towerDetails.Columns.Add("TowerCost");
            _towerDetails.Columns.Add("TowerCoverage");
            _towerDetails.Columns.Add("TowerHeight");
            _towerDetails.Columns.Add("TowerBaseArea");

            IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)_workspace;
            ITable pTable = (ITable) pFeatureWorkspace.OpenTable("TowerDetails");
            ICursor pCursor = pTable.Search(null, false);
            IRow pRow = pCursor.NextRow();

            while(pRow != null)
            {
                DataRow dtRow = _towerDetails.NewRow();
                dtRow["TowerType"] = pRow.get_Value(pRow.Fields.FindField("TowerType"));
                dtRow["TowerCost"] = pRow.get_Value(pRow.Fields.FindField("TowerCost"));
                dtRow["TowerCoverage"] = pRow.get_Value(pRow.Fields.FindField("TowerCoverage"));
                dtRow["TowerHeight"] = pRow.get_Value(pRow.Fields.FindField("TowerHeight"));
                dtRow["TowerBaseArea"] = pRow.get_Value(pRow.Fields.FindField("TowerBaseArea"));

                _towerDetails.Rows.Add(dtRow);
                _towerDetails.AcceptChanges();

                pRow = pCursor.NextRow();
            }
        }

        protected Tower GetTower(IFeature pFeature)
        {
            Tower tower = new Tower();
            tower.ID = pFeature.get_Value(pFeature.Fields.FindField("TOWERID")); ;
            tower.networkBand = pFeature.get_Value(pFeature.Fields.FindField("NETWORKBAND"));
            tower.towerType = pFeature.get_Value(pFeature.Fields.FindField("TOWERTYPE"));
            tower.towerLocation = (IPoint)pFeature.Shape;

            DataRow[] dtRows = _towerDetails.Select("TowerType = '" + tower.towerType + "'");
            foreach (DataRow r in dtRows)
            {
                tower.towerCoverage = double.Parse( r["TowerCoverage"].ToString());
                tower.towerCost = double.Parse( r["TowerCost"].ToString());
                tower.towerHeight = double.Parse( r["TowerHeight"].ToString());
                tower.towerBaseArea = double.Parse( r["TowerBaseArea"].ToString());
            }
            return tower;
        }

        public Tower GetTowerByID(string towerID)
        {
            // Query the geodabatase 
            IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)_workspace;
            IFeatureClass fcTower = pFeatureWorkspace.OpenFeatureClass("Towers");

            // Get the tower feature by ID
            IQueryFilter pQFilter = new QueryFilter();
            pQFilter.WhereClause = "TOWERID = '" + towerID + "'";
            IFeatureCursor pFCursor = fcTower.Search(pQFilter, true);
            IFeature pTowerFeature = pFCursor.NextFeature();

            if (pTowerFeature == null)
            {
                return null;
            }

            Tower tower = GetTower(pTowerFeature);

            // Get the tower type, and then query the tower detail table


            return tower;
        }

        public Tower GetNearestTower(IPoint pPoint)
        {
            //spatial query
            ITopologicalOperator pTopo = (ITopologicalOperator)pPoint;
            IGeometry pBufferedPoint = pTopo.Buffer(50);

            ISpatialFilter pSFilter = new SpatialFilter();
            pSFilter.Geometry = pBufferedPoint;
            pSFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            // query gdb
            IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)_workspace;
            IFeatureClass pFC = pFWorkspace.OpenFeatureClass("Towers");
            IFeatureCursor pFCursor = pFC.Search(pSFilter, true);
            IFeature pFeature = pFCursor.NextFeature();

            if (pFeature == null)
            {
                return null;
            }

            return GetTower(pFeature); ;
        }

        public Towers GetTowers()
        {
            IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)_workspace;
            IFeatureClass pTowersFC = pFeatureWorkspace.OpenFeatureClass("Towers");

            Towers pTowers = new Towers();
            IFeatureCursor pFCursor = pTowersFC.Search(null, false);
            IFeature pFeature = pFCursor.NextFeature();
            while (pFeature != null)
            {
                pTowers.Items.Add(GetTower(pFeature));
                pFeature = pFCursor.NextFeature();
            }
            // for each feature in pTowersFC, create a Tower object and add to Towers
            return pTowers;
        }

        public void GetTowerCoverage(Towers pTowers)
        {

            IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)_workspace;
            IFeatureClass pRangeFC = pFeatureWorkspace.OpenFeatureClass("TowerRanges");

            IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)_workspace;
            try
            {
                pWorkspaceEdit.StartEditing(true);
                pWorkspaceEdit.StartEditOperation();

                IFeatureCursor pRangeCursor = pRangeFC.Update(null, false);
                //IFeature pFeature = pRangeCursor.NextFeature();
                while (pRangeCursor.NextFeature() != null)
                {
                    //pFeature.Delete();
                    //pFeature = pRangeCursor.NextFeature();
                    pRangeCursor.DeleteFeature();
                }

                foreach (Tower pTower in pTowers.Items)
                {
                    //Tower pTower = tm.GetTowerByID("T04");

                    double towerRange = pTower.towerCoverage;
                    ITopologicalOperator pTopo = (ITopologicalOperator)pTower.towerLocation;
                    IPolygon range3Bars = (IPolygon)pTopo.Buffer(towerRange / 3);

                    IPolygon range2BarsWhole = (IPolygon)pTopo.Buffer(towerRange * 2 / 3);
                    ITopologicalOperator p2BarTopo = (ITopologicalOperator)range2BarsWhole;
                    IPolygon range2BarsDonut = (IPolygon)p2BarTopo.Difference(range3Bars);

                    IPolygon range1BarWhole = (IPolygon)pTopo.Buffer(towerRange);
                    ITopologicalOperator p1BarTopo = (ITopologicalOperator)range1BarWhole;
                    IPolygon range1BarDonut = (IPolygon)p1BarTopo.Difference(range2BarsWhole);

                    //IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)pWorkspaceEdit;
                    //IFeatureClass pTowerRangeFC = pFWorkspace.OpenFeatureClass("TowerRanges");

                    IFeature pFeature = pRangeFC.CreateFeature();
                    pFeature.set_Value(pFeature.Fields.FindField("TOWERID"), pTower.ID);
                    pFeature.set_Value(pFeature.Fields.FindField("RANGE"), 3);
                    pFeature.Shape = range3Bars;
                    pFeature.Store();

                    IFeature p2BarsFeature = pRangeFC.CreateFeature();
                    p2BarsFeature.set_Value(pFeature.Fields.FindField("TOWERID"), pTower.ID);
                    p2BarsFeature.set_Value(pFeature.Fields.FindField("RANGE"), 2);
                    p2BarsFeature.Shape = range2BarsDonut;
                    p2BarsFeature.Store();

                    IFeature p1BarsFeature = pRangeFC.CreateFeature();
                    p1BarsFeature.set_Value(pFeature.Fields.FindField("TOWERID"), pTower.ID);
                    p1BarsFeature.set_Value(pFeature.Fields.FindField("RANGE"), 1);
                    p1BarsFeature.Shape = range1BarDonut;
                    p1BarsFeature.Store();
                }

                pWorkspaceEdit.StopEditOperation();
                pWorkspaceEdit.StopEditing(true);
            }
            catch (Exception ex)
            {
                pWorkspaceEdit.AbortEditOperation();
                MessageBox.Show(ex.ToString());
            }
        }

        public void GenerateDeadArea()
        {
            try
            {
                IGeometry pCoverageGeom = null;

                IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)_workspace;
                IWorkspaceEdit pWorspaceEdit = (IWorkspaceEdit)pFeatureWorkspace;  
              
                ServiceTerritory pST = new ServiceTerritory(_workspace, "Main");

                if(pST.ServiceTerritoryFeature != null)
                {
                    if(pCoverageGeom == null)
                        pCoverageGeom = pST.ServiceTerritoryFeature.Shape;
                    else
                    {
                        ITopologicalOperator pTopoUnion = (ITopologicalOperator)pCoverageGeom;
                        pCoverageGeom = pTopoUnion.Union(pST.ServiceTerritoryFeature.Shape);
                    }                    
                }

                IFeatureClass pServiceFC = pFeatureWorkspace.OpenFeatureClass("ServiceTerritory");
                IFeatureCursor pServiceCursor = pServiceFC.Search(null, false);
                IFeature pServiceFeature = pServiceCursor.NextFeature();
                               
                ITopologicalOperator pTopoClip = (ITopologicalOperator)pServiceFeature.Shape;
                IGeometry pDeadArea = pTopoClip.Difference(pCoverageGeom);

                IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)_workspace;
                pWorkspaceEdit.StartEditing(true);
                pWorkspaceEdit.StartEditOperation();
                IFeatureClass pDeadFC = pFeatureWorkspace.OpenFeatureClass("DeadArea");
                IFeatureCursor pDeleteCursor = pDeadFC.Search(null, false);
                IFeature pExistingFeature = pDeleteCursor.NextFeature();
                while(pExistingFeature != null)
                {
                    pExistingFeature.Delete();
                    pExistingFeature = pDeleteCursor.NextFeature();
                }

                       

                IFeature pDeadFeature = pDeadFC.CreateFeature();
                pDeadFeature.Shape = pDeadArea;
                //pDeadFeature.set_Value(pDeadFeature.Fields.FindField("COVERAGE_PCT"), dead_pct);
                pDeadFeature.Store();
                pWorkspaceEdit.StopEditOperation();
                pWorkspaceEdit.StopEditing(true);

                int dead_pct = (int)(100 * ((IArea)pDeadFeature.Shape).Area / ((IArea)pServiceFeature.Shape).Area);
                int reception_pct = 100 - dead_pct;         
                //pST.UpdateCoveragePct(dead_pct, reception_pct);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
