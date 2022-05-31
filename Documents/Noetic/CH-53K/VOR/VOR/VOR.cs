using BaseARINCFormLibrary.Interfaces;
using BaseARINCFormLibrary.SessionManagment;

namespace VOR
{
    public class VOR : IARINCDevice
    {
        private VorARINCForm frmVORARINC = null;
        private readonly string _displayName;

        public ARINCSession Session
        {
            get
            {
                if (frmVORARINC != null)
                {
                    return frmVORARINC.Session;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (frmVORARINC != null)
                {
                    frmVORARINC.Session = value;
                }
            }
        }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        public bool DeviceEnabled
        {
            get
            {
                return frmVORARINC.DeviceEnabled;
            }

            set
            {
                if (frmVORARINC != null)
                {
                    frmVORARINC.DeviceEnabled = value;
                }
            }
        }

        public IMainForm MainForm
        {
            set
            {
                if (frmVORARINC != null)
                {
                    frmVORARINC.MainForm = value;
                }
            }
        }

        public bool SimulationEnabled
        {
            get
            {
                return frmVORARINC.SimulationEnabled;
            }

            set
            {
                if (frmVORARINC != null)
                {
                    frmVORARINC.SimulationEnabled = value;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title"></param>
        /// <param name="rtAddress"></param>
        /// <param name="handles"></param>
        /// <param name="ControllerBusIndex"></param>
        /// <param name="TotalBusCount"></param>
        public VOR(string title)
        {
            _displayName = title;
            frmVORARINC = new VorARINCForm(title);
        }
        ~VOR()
        {

        }

        public void Close()
        {
            frmVORARINC.Close();
        }

        public void ShowARINCWindow()
        {
            if (frmVORARINC != null)
            {
                frmVORARINC.Show();
                frmVORARINC.WindowState = System.Windows.Forms.FormWindowState.Normal;
                frmVORARINC.BringToFront();
            }
        }

        public void SetDeviceEnabled(bool enabled)
        {
            if (frmVORARINC != null)
            {
                frmVORARINC.DeviceEnabled = enabled;
            }
        }

        public void SetSimEnabled(bool enabled)
        {
            if (frmVORARINC != null)
            {
                frmVORARINC.SimulationEnabled = enabled;
            }
        }

        public void SetARINCSession(ARINCSession session)
        {
            if (frmVORARINC != null)
            {
                frmVORARINC.SetARINCSession(session);
            }
        }

        public bool GetSimEnabled()
        {
            if (frmVORARINC != null)
            {
                return frmVORARINC.SimulationEnabled;
            }
            else
            {
                return false;
            }
        }

        public bool GetDeviceEnabled()
        {
            return frmVORARINC.DeviceEnabled;
        }
    }
}
