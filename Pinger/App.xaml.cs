using System;
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

            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                new MenuItem("Exit", new EventHandler((o, e) => {app.Shutdown();})),
            });

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // Create a buffer of 32 bytes of data to be transmitted. 
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);
            int timeout = 500;

            System.Console.Write("Pinging... ");
            PingReply reply = ping.Send("google.com", timeout, buffer, new PingOptions());
            System.Console.WriteLine(reply.Status.ToString() + " " + reply.RoundtripTime + "ms (" + (reply.Address == null ? "null" : reply.Address.ToString()) + ")");

            if (reply.Status == IPStatus.Success)
            {
                lastSuccess = System.DateTime.Now;

                if (notifyIcon.Icon != ICON_GOOD)
                {
                    notifyIcon.Icon = ICON_GOOD;
                    notifyIcon.Visible = true;
                    notifyIcon.Text = "connection ok";
                }
            }
            else
            {
                var text = "connection out for " + (System.DateTime.Now - lastSuccess).Seconds + " seconds";

                notifyIcon.BalloonTipText = text;
                notifyIcon.Text = text;

                if (notifyIcon.Icon != ICON_BAD)
                {
                    notifyIcon.Icon = ICON_BAD;
                    notifyIcon.Visible = true;

                    var diff = System.DateTime.Now - lastSuccess;
                    if (diff > TimeSpan.FromSeconds(2.0))
                    {
                        notifyIcon.ShowBalloonTip(5000); // Shows BalloonTip 
                    }
                }
            }

        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            dispatcherTimer.Stop();
            notifyIcon.Icon = null;
        }
    }
}
