using BaseARINCFormLibrary;
using BaseARINCFormLibrary.Interfaces;
using BaseARINCFormLibrary.SessionManagment;
using C1.Win.C1FlexGrid;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace VOR
{
    public partial class VorARINCForm : BaseARINCForm
    {
        private ARINCSession _session;
        private string _displayName;
        private Int64 _arincEnabled = 0;
        private Int64 _simEnabled = 0;
        private IMainForm _mainForm = null;

        private const double DISCETE_DATA_CONST = -999999.99;

        private List<ARINCRowValue> _rowReceiveItems = new List<ARINCRowValue>(new ARINCRowValue[(int)ReceiveLabels.Count]);
        private List<ARINCRowValue> _rowTransmitItems = new List<ARINCRowValue>(new ARINCRowValue[(int)TransmitLabels.Count]);

        private enum ControlRows
        {
            DeviceEnabled,
            MagneticHeadingSource,
            HybridAltitudeLoop,
            HybridAltitudeClamp,
            AttitudeValid,
            NAVDataUnavailable,
            NAVReady,
            CoarseAlignComplete,
            CoarseLevelComplete,
            DegradedNavReady,
            INSFault,
            AttitudeReady,
            EGIMode,
            LateralAccel,
            LongitudinalAccel,
            NormalAccel,
            Count
        }

        private enum EGIMode
        {
            Auto,
            Init,
            SHAlign,
            Level,
            Test,
            GCAlign,
            IFA,
            AHRS,
            Nav,
            OverflyFix,
            Other,
            Count
        }

        private enum ReceiveLabels
        {
            VorDataLoadRT8,
            VorIlsFrequencyInput,
            Count
        }

        private enum TransmitLabels
        {
            VorIlsFrequencyInput,
            MarkerBeaconAudioPresence,
            EquipmentId,
            LruTopLevelStatus,
            Diagnostics,
            LocalizeDeviation,
            GlideslopeDeviation,
            VorOmnibearing,
            Dataload,
            Count
        }
        public ARINCSession Session
        {
            get
            {
                return _session;
            }

            set
            {
                _session = value;
            }
        }

        public bool DeviceEnabled
        {
            get
            {
                return GetDeviceEnabled();
            }
            internal set
            {
                //SetDeviceEnabled(value);
            }
        }

        public bool SimulationEnabled
        {
            get
            {
                return GetSimulationEnabled();
            }

            internal set
            {
                SetSimulationEnabled(value);
            }
        }

        private bool ShouldPeriodic
        {
            get
            {
                //is device enabled and SIM on
                return GetDeviceEnabled() && GetSimulationEnabled();
            }
        }

        public IMainForm MainForm
        {
            get
            {
                return _mainForm;
            }

            set
            {
                _mainForm = value;
            }
        }

        public VorARINCForm(string displayName) : base((int)ReceiveLabels.Count)
        {
            InitializeComponent();

            _displayName = displayName;

            this.Text = "ARINC " + displayName;

            Initialize();
        }

        private void Initialize()
        {
            SetControlsGridRowCount((int)ControlRows.Count);

            for (int i = 0; i < (int)ReceiveLabels.Count; i++)
            {
                _rowReceiveItems[i] = new ARINCRowValue();
                _rowReceiveItems[i].SSM = 0;
            }

            for (int i = 0; i < (int)TransmitLabels.Count; i++)
            {
                _rowTransmitItems[i] = new ARINCRowValue();
                _rowTransmitItems[i].SSM = 0;
            }

            //SetMainGridRowCount((int)ReceiveLabels.Count);

            _rowReceiveItems[(int)ReceiveLabels.VorDataLoadRT8].SetLabelWithOctalInt(024);
            _rowReceiveItems[(int)ReceiveLabels.VorDataLoadRT8].Name = "VOR Data Load RT8";
            _rowReceiveItems[(int)ReceiveLabels.VorIlsFrequencyInput].SetLabelWithOctalInt(034);
            _rowReceiveItems[(int)ReceiveLabels.VorIlsFrequencyInput].Name = "VOR/ILS Frequency Input";

            _rowTransmitItems[(int)TransmitLabels.VorIlsFrequencyInput].SetLabelWithOctalInt(034);
            _rowTransmitItems[(int)TransmitLabels.VorIlsFrequencyInput].Name = "VOR/ILS Frequency Input";
            _rowTransmitItems[(int)TransmitLabels.MarkerBeaconAudioPresence].SetLabelWithOctalInt(043);
            _rowTransmitItems[(int)TransmitLabels.MarkerBeaconAudioPresence].Name = "Marker Beacon Audio Presence";
            _rowTransmitItems[(int)TransmitLabels.EquipmentId].SetLabelWithOctalInt(056);
            _rowTransmitItems[(int)TransmitLabels.EquipmentId].Name = "Equipment ID";
            _rowTransmitItems[(int)TransmitLabels.LruTopLevelStatus].SetLabelWithOctalInt(057);
            _rowTransmitItems[(int)TransmitLabels.LruTopLevelStatus].Name = "LRY Top Level Status";
            _rowTransmitItems[(int)TransmitLabels.Diagnostics].SetLabelWithOctalInt(067);
            _rowTransmitItems[(int)TransmitLabels.Diagnostics].Name = "Diagnostics";
            _rowTransmitItems[(int)TransmitLabels.LocalizeDeviation].SetLabelWithOctalInt(173);
            _rowTransmitItems[(int)TransmitLabels.LocalizeDeviation].Name = "Localize Deviation";
            _rowTransmitItems[(int)TransmitLabels.GlideslopeDeviation].SetLabelWithOctalInt(174);
            _rowTransmitItems[(int)TransmitLabels.GlideslopeDeviation].Name = "Glideslope Deviation";
            _rowTransmitItems[(int)TransmitLabels.VorOmnibearing].SetLabelWithOctalInt(114);
            _rowTransmitItems[(int)TransmitLabels.VorOmnibearing].Name = "VOR Omnibearing";
            _rowTransmitItems[(int)TransmitLabels.Dataload].SetLabelWithOctalInt(226);
            _rowTransmitItems[(int)TransmitLabels.Dataload].Name = "Dataload";

            List<string> slEnabledDisabled = new List<string>(new string[] { "Enabled", "Disabled" });
            List<string> slNotSetSet = new List<string>(new string[] { "Not set", "Set" });

            List<string> slEGIMode = new List<string>(new string[] {"Auto", "Init", "SH Align", "Level", "Test", "GC Align", "IFA", "AHRS",
            "Nav",
            "Overfly Fix" });

            AddCombo((int)ControlRows.DeviceEnabled, 0, slEnabledDisabled);

            SetRowText((int)ControlRows.DeviceEnabled, "Device Enabled");
            SetRowText((int)ControlRows.EGIMode, "EGI Mode");

            for (int row = (int)ControlRows.MagneticHeadingSource; row < (int)ControlRows.EGIMode; row++)
            {
                AddCombo(row, 0, slNotSetSet);
            }

            SetRowText((int)ControlRows.MagneticHeadingSource, "Magnetic Heading Source");
            SetRowText((int)ControlRows.HybridAltitudeLoop, "Hybrid Altitude Loop");
            SetRowText((int)ControlRows.HybridAltitudeClamp, "Hybrid Altitude Clamp");
            SetRowText((int)ControlRows.AttitudeValid, "Attitude Valid");
            SetRowText((int)ControlRows.NAVDataUnavailable, "NAV Data Unavailable");
            SetRowText((int)ControlRows.NAVReady, "NAV Ready");
            SetRowText((int)ControlRows.CoarseAlignComplete, "Coarse Align Complete");
            SetRowText((int)ControlRows.CoarseLevelComplete, "Coarse Level Complete");
            SetRowText((int)ControlRows.DegradedNavReady, "Degraded Nav Ready");
            SetRowText((int)ControlRows.INSFault, "INS Fault");
            SetRowText((int)ControlRows.AttitudeReady, "Attitude Ready");
            SetRowText((int)ControlRows.EGIMode, "EGI Mode");
            SetRowText((int)ControlRows.LateralAccel, "Lateral Acceleration");
            SetRowText((int)ControlRows.LongitudinalAccel, "Longitudinal Acceleration");
            SetRowText((int)ControlRows.NormalAccel, "Normal Acceleration");

            AddCombo((int)ControlRows.EGIMode, 0, slEGIMode);

            RefreshAllRows();
        }

        private bool GetDeviceEnabled()
        {
            Int64 devEnabled = Interlocked.Read(ref _arincEnabled);

            if (devEnabled == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //private void SetDeviceEnabled(bool onOff)
        //{
        //    if (onOff == true)
        //    {
        //        Interlocked.Exchange(ref _arincEnabled, 1);

        //        if (_mainForm != null && !string.IsNullOrEmpty(_displayName))
        //        {
        //            //enable glyph in main sim
        //            _mainForm.EnableARINCButton(_displayName);
        //        }

        //        Row r = ControlsGrid.Rows[(int)ControlRows.DeviceEnabled];

        //        ComboBox combo = r.Editor as ComboBox;

        //        if (combo != null)
        //        {
        //            //enabled choice
        //            if (combo.SelectedIndex != 0)
        //            {
        //                combo.SelectedIndex = 0;
        //                r[1] = combo.Items[combo.SelectedIndex].ToString();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Interlocked.Exchange(ref _arincEnabled, 0);

        //        if (_mainForm != null && !string.IsNullOrEmpty(_displayName))
        //        {
        //            //enable glyph in main sim
        //            _mainForm.DisableARINCButton(_displayName);
        //        }

        //        Row r = ControlsGrid.Rows[(int)ControlRows.DeviceEnabled];

        //        ComboBox combo = r.Editor as ComboBox;

        //        if (combo != null)
        //        {
        //            //set to disabled choice
        //            if (combo.SelectedIndex != 1)
        //            {
        //                combo.SelectedIndex = 1;
        //                r[1] = combo.Items[combo.SelectedIndex].ToString();
        //            }
        //        }
        //    }
        //}

        private bool GetSimulationEnabled()
        {
            Int64 simEnabled = Interlocked.Read(ref _simEnabled);

            if (simEnabled == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetSimulationEnabled(bool onOff)
        {
            if (onOff == true)
            {
                Interlocked.Exchange(ref _simEnabled, 1);
            }
            else
            {
                Interlocked.Exchange(ref _simEnabled, 0);
            }
        }

        //private int GetRowSelectedIndex(ControlRows row)
        //{
        //    int rowIndex = (int)row;

        //    Row r = ControlsGrid.Rows[rowIndex];

        //    ComboBox combo = r.Editor as ComboBox;

        //    int selectedIndex = 0;

        //    if (combo != null)
        //    {
        //        selectedIndex = combo.SelectedIndex;
        //    }

        //    return selectedIndex;
        //}

        private void RefreshReceiveRow(ReceiveLabels rowEnum)
        {
            int index = (int)rowEnum;
            int row = index + 1;

            //label
            SetCellText(row, LABEL_COL, _rowReceiveItems[index].LabelString);

            //Name
            SetCellText(row, NAME_COL, _rowReceiveItems[index].Name);

            //data
            if (_rowReceiveItems[index].Data == DISCETE_DATA_CONST)
            {
                SetCellText(row, DATA_COL, "Discrete/Not applicable");
            }
            else
            {
                SetCellText(row, DATA_COL, _rowReceiveItems[index].Data.ToString("0.##"));
            }

            //raw data
            SetCellText(row, RAW_DATA_COL, _rowReceiveItems[index].RawDataString);
        }

        private void RefreshTransmitRow(TransmitLabels rowEnum)
        {
            int index = (int)rowEnum;
            int row = index + 1;

            //label
            SetCellText(row, LABEL_COL, _rowTransmitItems[index].LabelString);

            //Name
            SetCellText(row, NAME_COL, _rowTransmitItems[index].Name);

            //data
            if (_rowTransmitItems[index].Data == DISCETE_DATA_CONST)
            {
                SetCellText(row, DATA_COL, "Discrete/Not applicable");
            }
            else
            {
                SetCellText(row, DATA_COL, _rowTransmitItems[index].Data.ToString("0.##"));
            }

            //raw data
            SetCellText(row, RAW_DATA_COL, _rowTransmitItems[index].RawDataString);
        }

        private void RefreshAllRows()
        {
            
            //if ()
            //{
            //    SetMainGridRowCount((int)ReceiveLabels.Count);

            //    for (int index = 0; index < (int)ReceiveLabels.Count; index++)
            //    {
            //        ReceiveLabels row = (ReceiveLabels)index;
            //        RefreshReceiveRow(row);
            //    }
            //}
            //else
            //{
            //    SetMainGridRowCount((int)TransmitLabels.Count);

            //    for (int index = 0; index < (int)TransmitLabels.Count; index++)
            //    {
            //        TransmitLabels row = (TransmitLabels)index;
            //        RefreshTransmitRow(row);
            //    }
            //}
            SetMainGridRowCount((int)ReceiveLabels.Count);
            for (int index = 0; index < (int)ReceiveLabels.Count; index++)
            {
                ReceiveLabels row = (ReceiveLabels)index;
                RefreshReceiveRow(row);
            }
            //SetMainGridRowCount((int)TransmitLabels.Count);
            //for (int index = 0; index < (int)TransmitLabels.Count; index++)
            //{
            //    TransmitLabels row = (TransmitLabels)index;
            //    RefreshTransmitRow(row);
            //}
            //
        }

        internal void SetARINCSession(ARINCSession session)
        {
            _session = session;
        }
    }
}
