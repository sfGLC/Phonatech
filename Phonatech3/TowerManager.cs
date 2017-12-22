using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phonatech3
{
    public class TowerManager
    {
        private IWorkspace _workspace;

        public TowerManager(IWorkspace pWorkspace)
        {
            _workspace = pWorkspace;
        }

        protected Tower GetTower(IFeature pFeature)
        {
            Tower tower = new Tower();
            tower.ID = pFeature.get_Value(pFeature.Fields.FindField("TOWERID")); ;
            tower.networkBand = pFeature.get_Value(pFeature.Fields.FindField("NETWORKBAND"));
            tower.towerType = pFeature.get_Value(pFeature.Fields.FindField("TOWERTYPE"));
            tower.towerLocation = (IPoint)pFeature.Shape;

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
            ITopologicalOperator pTopo = (ITopologicalOperator) pPoint;
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

            return  GetTower(pFeature);;
        }
    }
}
