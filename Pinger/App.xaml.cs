﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Forms;
using System.Drawing;
using System.Windows.Threading;
using System.Net.NetworkInformation;

namespace Pinger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static Icon CreateIcon(string iconName)
        {
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
    "Pinger.res." + iconName))
            {
                return new Icon(stream);
            }
        }

        static Icon ICON_GOOD = CreateIcon("accept.ico");
        static Icon ICON_BAD = CreateIcon("cross.ico");

        NotifyIcon notifyIcon = new NotifyIcon();
        DispatcherTimer dispatcherTimer = new DispatcherTimer();

        System.DateTime lastSuccess = System.DateTime.Now;

        Ping ping = new Ping();

        public App()
        {
            notifyIcon.Icon = ICON_GOOD;
            notifyIcon.Visible = true;

            var app = this;

            notifyIcon.DoubleClick += (s, a) => { ShowDiagnostics(); };
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                new MenuItem("Show Diagnostics", new EventHandler((o, e) => { ShowDiagnostics(); })),
                new MenuItem("-"),
                new MenuItem("Exit", new EventHandler((o, e) => {app.Shutdown();})),
            });

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void ShowDiagnostics()
        {
            try
            {
                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.AppStarting;
                System.Diagnostics.Process.Start("http://10.0.0.138/cgi/b/events/?be=0&l0=1&l1=2");
            }
            catch (Exception)
            {
            }
            finally
            {
                System.Windows.Input.Mouse.OverrideCursor = null;
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // Create a buffer of 32 bytes of data to be transmitted. 
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);
            int timeout = 500;

            System.Console.Write("Pinging... ");
            try
            {
                // ping 216.58.220.142 (google.com)
                PingReply reply = ping.Send("216.58.220.142", timeout, buffer, new PingOptions());
                System.Console.WriteLine(reply.Status.ToString() + " " + reply.RoundtripTime + "ms (" + (reply.Address == null ? "null" : reply.Address.ToString()) + ")");

                if (reply.Status == IPStatus.Success)
                {
                    notifyIcon.Text = "connection ok (" + reply.RoundtripTime + "ms)";
                    lastSuccess = System.DateTime.Now;

                    if (notifyIcon.Icon != ICON_GOOD)
                    {
                        notifyIcon.Icon = ICON_GOOD;
                        notifyIcon.Visible = true;
                    }
                }
                else
                {
                    var diff = System.DateTime.Now - lastSuccess;
                    if (diff > TimeSpan.FromSeconds(2.0))
                    {
                        var text = "connection out for " + (System.DateTime.Now - lastSuccess).Seconds + " seconds";

                        notifyIcon.BalloonTipText = text;
                        notifyIcon.Text = text;

                        if (notifyIcon.Icon != ICON_BAD)
                        {
                            notifyIcon.Icon = ICON_BAD;
                            notifyIcon.Visible = true;

                            notifyIcon.ShowBalloonTip(5000); // Shows BalloonTip 
                        }
                    }
                }
            } catch (PingException ex)
            {
                var text = ex.Message;
                if (ex.InnerException != null) text = ex.InnerException.Message;

                text = text.Substring(0, text.Length < 63 ? text.Length : 63);
                notifyIcon.BalloonTipText = text;
                notifyIcon.Text = text;

                notifyIcon.ShowBalloonTip(1000); // Shows BalloonTip 
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            dispatcherTimer.Stop();
            notifyIcon.Icon = null;
        }
    }
}
