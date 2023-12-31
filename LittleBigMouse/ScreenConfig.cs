﻿/*
  MouseControl - Mouse Managment in multi DPI monitors environment
  Copyright (c) 2015 Mathieu GRENET.  All right reserved.

  This file is part of MouseControl.

    ArduixPL is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ArduixPL is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MouseControl.  If not, see <http://www.gnu.org/licenses/>.

	  mailto:mathieu@mgth.fr
	  http://www.mgth.fr
*/

using LittleBigMouseGeo;
using Microsoft.Win32;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;

namespace LittleBigMouse
{
    public class ScreenConfig : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ScreenConfig()
        {
            _allScreens = new List<Screen>();
            _MouseHookManager = new MouseHookListener(new GlobalHooker());
            Load();
            MouseLocation = new PixelPoint(this, 0, 0);
        }

        public event EventHandler RegistryChanged;

        private List<Screen> _allScreens;
        private readonly MouseHookListener _MouseHookManager;

        private PixelPoint _oldPoint;
        private bool _enabled;
        private bool _loadAtStartup;
        private bool _adjustPointer;
        private bool _adjustSpeed;
        private Rect _configLocation;
        private bool _allowToJump;

        public void Start()
        {
            if (Enabled)
            {
                _MouseHookManager.MouseMoveExt += _MouseHookManager_MouseMoveExt;
                _MouseHookManager.Enabled = true;
            }
        }
        public void Stop()
        {
            _MouseHookManager.MouseMoveExt -= _MouseHookManager_MouseMoveExt;
            _MouseHookManager.Enabled = false;
        }

        public PixelPoint MouseLocation { get; private set; }

        private void _MouseHookManager_MouseMoveExt(object sender, MouseEventExtArgs e)
        {
            PixelPoint pIn;
            if (_oldPoint == null)
            {
                _oldPoint = new PixelPoint(this, e.X, e.Y);
                Debug.Print("New:" + (_oldPoint.Screen?.DeviceName??"null"));
                return;
            }

            pIn = new PixelPoint(_oldPoint.Screen, e.X, e.Y);

            MouseLocation = pIn; Changed("MouseLocation");


            if (pIn.TargetScreen == _oldPoint.Screen)
            {
                _oldPoint = pIn;
                return;
            }

            Screen screenOut = pIn.Physical.TargetScreen;

            Debug.Print("From:" + _oldPoint.Screen.DeviceName);
            Debug.Print(" To:" + (screenOut?.DeviceName??"null") + "\n");


            //
            // Allow To Jump
            //
            if (screenOut==null && AllowToJump)
            {
                double dist = 100.0; // double.PositiveInfinity;
                Segment seg = new Segment(_oldPoint.Physical.Point, pIn.Physical.Point);
                foreach (Screen s in AllScreens)
                {
                    if (s!=_oldPoint.Screen)
                    {
                        foreach (Point p in seg.Line.Intersect(s.PhysicalBounds))
                        {
                            Segment travel = new Segment(_oldPoint.Physical.Point, p);
                            if (travel.Rect.Contains(pIn.Physical.Point))
                            {
                                if (travel.Size < dist)
                                {
                                    dist = travel.Size;
                                    pIn = (new PhysicalPoint(this, p.X, p.Y)).Pixel;
                                    screenOut = s;
                                }
                            }
                        }
                    }
                }
            }

            // if new position is within another screen
            if (screenOut != null)
            {
                double factor = screenOut.ScaleFactor;

                Debug.Print(factor.ToString(CultureInfo.InvariantCulture));

                Point p = pIn.Physical.Pixel.DpiAware.Point;


                Mouse.CursorPos = p;
                //Mouse.CursorPos = new Point(
                //    screenOut.PixelLocation.X + (p.X - screenOut.PixelLocation.X) / (screenOut.DpiX/screenOut.EffectiveDpiX),
                //    screenOut.PixelLocation.Y + (p.Y - screenOut.PixelLocation.Y) / (screenOut.DpiY / screenOut.EffectiveDpiY)
                //     );

                Point p2 = screenOut.ScaledPoint(p);

                Debug.Print((screenOut.DpiY / screenOut.EffectiveDpiY).ToString());


 //               Mouse.CursorPos = p2;

/*                Mouse.CursorPos = new Point(
                    screenOut.PixelLocation.X + (p.X - screenOut.PixelLocation.X) / factor,
                    screenOut.PixelLocation.Y + (p.Y - screenOut.PixelLocation.Y) / factor
                    );
                    

                if (_currentScreen.DeviceNo==2 && screenOut.DeviceNo==1)
                {
                    pOut.Y /= 2;
                }
                if (_currentScreen.DeviceNo == 1 && screenOut.DeviceNo == 2)
                {
                    pOut.Y /= 2;
                }
  */              
                //System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)pOut.X,(int)pOut.Y);
                //_MouseHookManager.Enabled = false;
                //Mouse.SetCursorPos((int)pOut.X, (int)pOut.Y);


                //_MouseHookManager.Enabled = true;


                if (AdjustPointer)
                {
                    if (screenOut.DpiAvg > 110)
                    {
                        if (screenOut.DpiAvg > 138)
                            Mouse.setCursorAero(3);
                        else Mouse.setCursorAero(2);
                    }
                    else Mouse.setCursorAero(1);
                }

                if (AdjustSpeed)
                {
                    Mouse.MouseSpeed = Math.Round((5.0 / 96.0) * screenOut.DpiAvg, 0);
                }

                _oldPoint = new PixelPoint(screenOut,pIn.X,pIn.Y);
                e.Handled = true;
            }

        }


        public List<Screen> AllScreens { get { return _allScreens; } }

        private static String RootKey = "SOFTWARE\\" + System.Windows.Forms.Application.CompanyName + "\\" + Application.ResourceAssembly.GetName().Name;

        public RegistryKey OpenRegKey()
        {
                RegistryKey k = Registry.CurrentUser.CreateSubKey(RootKey);
                return k.CreateSubKey(_id);
        }

        private String _id = "";
        public void Load()
        {
            _id = "";

            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                Screen s = GetScreen(screen);
                _id += ((_id!="")?"." :"") + s.ID;
            }

            LoadAtStartup = App.Scheduled;

            using (RegistryKey k = OpenRegKey())
            {
                Enabled = k.GetValue("Enabled", 0).ToString() == "1";
                AdjustPointer = k.GetValue("AdjustPointer", 0).ToString() == "1";
                AdjustSpeed = k.GetValue("AdjustSpeed", 0).ToString() == "1";
                AllowToJump = k.GetValue("AllowToJump", 0).ToString() == "1";
                foreach(Screen s in AllScreens)
                {
                    s.Load(k);
                }
            }
            UpdatePhysicalBounds();
        }

        public void Save()
        {
            if (LoadAtStartup)
                App.Schedule();
            else
                App.Unschedule();

            using (RegistryKey k = OpenRegKey())
            {
                k.SetValue("Enabled", Enabled ? "1" : "0");
                k.SetValue("AdjustPointer", AdjustPointer ? "1" : "0");
                k.SetValue("AdjustSpeed", AdjustSpeed ? "1" : "0");
                k.SetValue("AllowToJump", AllowToJump ? "1" : "0");

                foreach (Screen s in AllScreens)
                    s.Save(k);
            }

            RegistryChanged?.Invoke(this, new EventArgs());
        }

        private Screen GetScreen(System.Windows.Forms.Screen screen)
        {
            foreach (Screen s in AllScreens.Where(s => s._formScreen.DeviceName == screen.DeviceName))
            {
                return s;
            }

            {
                Screen s = new Screen(this,screen);
                AllScreens.Add(s);
                s.PropertyChanged += Screen_PropertyChanged;
                return s;
            }
        }

        private Rect _physicalBounds = new Rect();

        private void UpdatePhysicalBounds()
        {
            Rect r = new Rect();
            foreach (Screen s in AllScreens)
            {
                if (r.Width == 0)
                    r = s.PhysicalBounds;
                else
                    r.Union(s.PhysicalBounds);
            }
            if (_physicalBounds != r)
            {
                _physicalBounds = r;
                Changed("PhysicalBounds");
            }
        }

        private void Screen_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "PhysicalBounds":
                    UpdatePhysicalBounds();
                    break;
            }
        }

        public Screen FromPixelPoint(AbsolutePoint point)
        {
            return AllScreens.FirstOrDefault(s => s.Bounds.Contains(point.Pixel));
        }

        public Screen FromWpfPoint(AbsolutePoint point)
        {
            return FromPixelPoint( point.Wpf );
        }

        public Screen PrimaryScreen => GetScreen(System.Windows.Forms.Screen.PrimaryScreen);

// Original windows locations
        public Rect OverallBounds
        {
            get
            {
                Rect r = new Rect();
                foreach (var s in AllScreens)
                {
                    if (r.Width == 0)
                        r = new Rect(new Point(s.PixelLocation.X,s.PixelLocation.Y),s.PixelSize);
                    else
                        r.Union(new Rect(new Point(s.PixelLocation.X, s.PixelLocation.Y), s.PixelSize));
                }
                return r;
            }
        }

        // Physical Locations
        public Rect PhysicalOverallBounds => _physicalBounds;

        public bool Enabled
        {
            get { return _enabled; }
            set {
                _enabled = value;
                Changed("Enabled");
            }
        }

        public bool LoadAtStartup
        {
            get { return _loadAtStartup;  }
            set
            {
                _loadAtStartup = value;
                Changed("LoadAtStartup");
            }
        }

        public bool AdjustPointer
        {
            get { return _adjustPointer; }
            set {
                _adjustPointer = value;
                Changed("AdjustPointer");
            }
        }

        public bool AdjustSpeed
        {
            get { return _adjustSpeed; }
            set {
                _adjustSpeed = value;
                Changed("AdjustSpeed");
            }
        }
        public bool AllowToJump
        {
            get { return _allowToJump; }
            set {
                _allowToJump = value;
                Changed("AllowToJump");
            }
        }

        public Rect ConfigLocation
        {
            get { return _configLocation; }
            set {
                _configLocation = value;
                Changed("ConfigLocation");
            }
        }


        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _MouseHookManager.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ScreenConfig() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
