using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.ArcMap;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;

namespace Phonatech3
{
    public class DeviceManager
    {
        private IWorkspace _workspace;

        public DeviceManager(IWorkspace pWorkspace)
        {
            this._workspace = pWorkspace;
        }

        public void AddDevice(string pDeviceID, IPoint pDeviceLocation)
        {
            Device pDevice = new Device(_workspace);
            pDevice.deviceID = pDeviceID;
            pDevice.deviceLocation = pDeviceLocation;
            pDevice.RecalculateSignal();
            pDevice.Store();

            MessageBox.Show(pDevice.bars + Environment.NewLine + pDevice.connectedTower.ID);
        }

        public Device GetDevice(string pDeviceID)
        {
            return new Device(_workspace);
        }

    }
}
