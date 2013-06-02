/*
 * Copyright (c) 2013, Carl Burch.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Carl Burch can be contacted at cburch@cburch.com or by post at
 * Hendrix College, 1600 Washington Ave, Conway AR 72032, USA.
 */
using System;
using Gtk;

using Toves.GuiGeneric.LayoutCanvas;
using Toves.GuiGeneric.Toolbox;
using Toves.GuiImpl.GtkCanvas;

namespace Toves.GuiImpl.GtkMain {
    public class GtkMain : Gtk.Window {
        public static string Version = "0.0.1";
        public static string CopyrightYear = "2013";

        public static void Main(string[] args)
        {
            Application.Init();
            GtkMain win = new GtkMain();
            win.Show();
            Application.Run();
        }

        private GtkCanvas.GtkCanvas canvas;

        public GtkMain () : base("Toves")
        {
            LayoutCanvasModel canvasModel = new LayoutCanvasModel();
            ToolboxModel toolboxModel = new ToolboxModel(canvasModel);

            canvas = new GtkCanvas.GtkCanvas();
            canvas.CanvasModel = canvasModel;
            GtkToolbox toolbox = new GtkToolbox(this, toolboxModel);

            VBox vbox = new VBox(false, 0);
            this.Add(vbox);
            vbox.PackStart(toolbox, false, false, 0);
            vbox.PackEnd(canvas, true, true, 0);
            this.SetDefaultSize(980, 600);
            this.ShowAll();

            canvas.GrabFocus();
        }

        protected override bool OnDeleteEvent(Gdk.Event evnt)
        {
            canvas.CanvasModel.Disable();
            Application.Quit();
            return true;
        }
    }
}
