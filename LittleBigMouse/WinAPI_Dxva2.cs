﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinAPI_Dxva2
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct PHYSICAL_MONITOR
    {
        public IntPtr hPhysicalMonitor;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szPhysicalMonitorDescription;
    }
    class Dxva2
    {
        [DllImport("Dxva2.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

        [DllImport("Dxva2.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("Dxva2.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetMonitorBrightness(IntPtr hMonitor, ref uint pdwMinimumBrightness, ref uint pdwCurrentBrightness, ref uint pdwMaximumBrightness);

        [DllImport("Dxva2.dll", CharSet = CharSet.Auto)]
        internal static extern bool SetMonitorBrightness(IntPtr hMonitor, uint dwNewBrightness);

        [DllImport("Dxva2.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetMonitorContrast(IntPtr hMonitor, ref uint pdwMinimumContrast, ref uint pdwCurrentContrast, ref uint pdwMaximumContrast);

        [DllImport("Dxva2.dll", CharSet = CharSet.Auto)]
        internal static extern bool SetMonitorContrast(IntPtr hMonitor, uint dwNewContrast);

        [DllImport("Dxva2.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetMonitorRedGreenOrBlueGain(IntPtr hMonitor, uint component, ref uint pdwMinimumContrast, ref uint pdwCurrentContrast, ref uint pdwMaximumContrast);

        [DllImport("Dxva2.dll", CharSet = CharSet.Auto)]
        internal static extern bool SetMonitorRedGreenOrBlueGain(IntPtr hMonitor, uint component, uint dwNewContrast);

        [DllImport("Dxva2.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetMonitorRedGreenOrBlueDrive(IntPtr hMonitor, uint component, ref uint pdwMinimumContrast, ref uint pdwCurrentContrast, ref uint pdwMaximumContrast);

        [DllImport("Dxva2.dll", CharSet = CharSet.Auto)]
        internal static extern bool SetMonitorRedGreenOrBlueDrive(IntPtr hMonitor, uint component, uint dwNewContrast);
        //GetMonitorRedGreenOrBlueGain

    }
}
