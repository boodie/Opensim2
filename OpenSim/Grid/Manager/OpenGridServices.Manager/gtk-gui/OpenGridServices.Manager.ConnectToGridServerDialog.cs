/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.42
//
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace OpenGridServices.Manager
{
    public partial class ConnectToGridServerDialog
    {
        private Gtk.VBox vbox2;
        private Gtk.VBox vbox3;
        private Gtk.HBox hbox1;
        private Gtk.Label label1;
        private Gtk.Entry entry1;
        private Gtk.HBox hbox2;
        private Gtk.Label label2;
        private Gtk.Entry entry2;
        private Gtk.HBox hbox3;
        private Gtk.Label label3;
        private Gtk.Entry entry3;
        private Gtk.Button button2;
        private Gtk.Button button8;

        protected virtual void Build()
        {
            Stetic.Gui.Initialize();
            // Widget OpenGridServices.Manager.ConnectToGridServerDialog
            this.Events = ((Gdk.EventMask)(256));
            this.Name = "OpenGridServices.Manager.ConnectToGridServerDialog";
            this.Title = Mono.Unix.Catalog.GetString("Connect to Grid server");
            this.WindowPosition = ((Gtk.WindowPosition)(4));
            this.HasSeparator = false;
            // Internal child OpenGridServices.Manager.ConnectToGridServerDialog.VBox
            Gtk.VBox w1 = this.VBox;
            w1.Events = ((Gdk.EventMask)(256));
            w1.Name = "dialog_VBox";
            w1.BorderWidth = ((uint)(2));
            // Container child dialog_VBox.Gtk.Box+BoxChild
            this.vbox2 = new Gtk.VBox();
            this.vbox2.Name = "vbox2";
            // Container child vbox2.Gtk.Box+BoxChild
            this.vbox3 = new Gtk.VBox();
            this.vbox3.Name = "vbox3";
            // Container child vbox3.Gtk.Box+BoxChild
            this.hbox1 = new Gtk.HBox();
            this.hbox1.Name = "hbox1";
            // Container child hbox1.Gtk.Box+BoxChild
            this.label1 = new Gtk.Label();
            this.label1.Name = "label1";
            this.label1.Xalign = 1F;
            this.label1.LabelProp = Mono.Unix.Catalog.GetString("Grid server URL: ");
            this.label1.Justify = ((Gtk.Justification)(1));
            this.hbox1.Add(this.label1);
            Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(this.hbox1[this.label1]));
            w2.Position = 0;
            // Container child hbox1.Gtk.Box+BoxChild
            this.entry1 = new Gtk.Entry();
            this.entry1.CanFocus = true;
            this.entry1.Name = "entry1";
            this.entry1.Text = Mono.Unix.Catalog.GetString("http://gridserver:8001");
            this.entry1.IsEditable = true;
            this.entry1.MaxLength = 255;
            this.entry1.InvisibleChar = '•';
            this.hbox1.Add(this.entry1);
            Gtk.Box.BoxChild w3 = ((Gtk.Box.BoxChild)(this.hbox1[this.entry1]));
            w3.Position = 1;
            this.vbox3.Add(this.hbox1);
            Gtk.Box.BoxChild w4 = ((Gtk.Box.BoxChild)(this.vbox3[this.hbox1]));
            w4.Position = 0;
            w4.Expand = false;
            w4.Fill = false;
            // Container child vbox3.Gtk.Box+BoxChild
            this.hbox2 = new Gtk.HBox();
            this.hbox2.Name = "hbox2";
            // Container child hbox2.Gtk.Box+BoxChild
            this.label2 = new Gtk.Label();
            this.label2.Name = "label2";
            this.label2.Xalign = 1F;
            this.label2.LabelProp = Mono.Unix.Catalog.GetString("Username:");
            this.label2.Justify = ((Gtk.Justification)(1));
            this.hbox2.Add(this.label2);
            Gtk.Box.BoxChild w5 = ((Gtk.Box.BoxChild)(this.hbox2[this.label2]));
            w5.Position = 0;
            // Container child hbox2.Gtk.Box+BoxChild
            this.entry2 = new Gtk.Entry();
            this.entry2.CanFocus = true;
            this.entry2.Name = "entry2";
            this.entry2.IsEditable = true;
            this.entry2.InvisibleChar = '•';
            this.hbox2.Add(this.entry2);
            Gtk.Box.BoxChild w6 = ((Gtk.Box.BoxChild)(this.hbox2[this.entry2]));
            w6.Position = 1;
            this.vbox3.Add(this.hbox2);
            Gtk.Box.BoxChild w7 = ((Gtk.Box.BoxChild)(this.vbox3[this.hbox2]));
            w7.Position = 1;
            w7.Expand = false;
            w7.Fill = false;
            // Container child vbox3.Gtk.Box+BoxChild
            this.hbox3 = new Gtk.HBox();
            this.hbox3.Name = "hbox3";
            // Container child hbox3.Gtk.Box+BoxChild
            this.label3 = new Gtk.Label();
            this.label3.Name = "label3";
            this.label3.Xalign = 1F;
            this.label3.LabelProp = Mono.Unix.Catalog.GetString("Password:");
            this.label3.Justify = ((Gtk.Justification)(1));
            this.hbox3.Add(this.label3);
            Gtk.Box.BoxChild w8 = ((Gtk.Box.BoxChild)(this.hbox3[this.label3]));
            w8.Position = 0;
            // Container child hbox3.Gtk.Box+BoxChild
            this.entry3 = new Gtk.Entry();
            this.entry3.CanFocus = true;
            this.entry3.Name = "entry3";
            this.entry3.IsEditable = true;
            this.entry3.InvisibleChar = '•';
            this.hbox3.Add(this.entry3);
            Gtk.Box.BoxChild w9 = ((Gtk.Box.BoxChild)(this.hbox3[this.entry3]));
            w9.Position = 1;
            this.vbox3.Add(this.hbox3);
            Gtk.Box.BoxChild w10 = ((Gtk.Box.BoxChild)(this.vbox3[this.hbox3]));
            w10.Position = 2;
            w10.Expand = false;
            w10.Fill = false;
            this.vbox2.Add(this.vbox3);
            Gtk.Box.BoxChild w11 = ((Gtk.Box.BoxChild)(this.vbox2[this.vbox3]));
            w11.Position = 2;
            w11.Expand = false;
            w11.Fill = false;
            w1.Add(this.vbox2);
            Gtk.Box.BoxChild w12 = ((Gtk.Box.BoxChild)(w1[this.vbox2]));
            w12.Position = 0;
            // Internal child OpenGridServices.Manager.ConnectToGridServerDialog.ActionArea
            Gtk.HButtonBox w13 = this.ActionArea;
            w13.Events = ((Gdk.EventMask)(256));
            w13.Name = "OpenGridServices.Manager.ConnectToGridServerDialog_ActionArea";
            w13.Spacing = 6;
            w13.BorderWidth = ((uint)(5));
            w13.LayoutStyle = ((Gtk.ButtonBoxStyle)(4));
            // Container child OpenGridServices.Manager.ConnectToGridServerDialog_ActionArea.Gtk.ButtonBox+ButtonBoxChild
            this.button2 = new Gtk.Button();
            this.button2.CanDefault = true;
            this.button2.CanFocus = true;
            this.button2.Name = "button2";
            this.button2.UseUnderline = true;
            // Container child button2.Gtk.Container+ContainerChild
            Gtk.Alignment w14 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
            w14.Name = "GtkAlignment";
            // Container child GtkAlignment.Gtk.Container+ContainerChild
            Gtk.HBox w15 = new Gtk.HBox();
            w15.Name = "GtkHBox";
            w15.Spacing = 2;
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Image w16 = new Gtk.Image();
            w16.Name = "image1";
            w16.Pixbuf = Gtk.IconTheme.Default.LoadIcon("gtk-apply", 16, 0);
            w15.Add(w16);
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Label w18 = new Gtk.Label();
            w18.Name = "GtkLabel";
            w18.LabelProp = Mono.Unix.Catalog.GetString("Connect");
            w18.UseUnderline = true;
            w15.Add(w18);
            w14.Add(w15);
            this.button2.Add(w14);
            this.AddActionWidget(this.button2, -5);
            Gtk.ButtonBox.ButtonBoxChild w22 = ((Gtk.ButtonBox.ButtonBoxChild)(w13[this.button2]));
            w22.Expand = false;
            w22.Fill = false;
            // Container child OpenGridServices.Manager.ConnectToGridServerDialog_ActionArea.Gtk.ButtonBox+ButtonBoxChild
            this.button8 = new Gtk.Button();
            this.button8.CanDefault = true;
            this.button8.CanFocus = true;
            this.button8.Name = "button8";
            this.button8.UseUnderline = true;
            // Container child button8.Gtk.Container+ContainerChild
            Gtk.Alignment w23 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
            w23.Name = "GtkAlignment1";
            // Container child GtkAlignment1.Gtk.Container+ContainerChild
            Gtk.HBox w24 = new Gtk.HBox();
            w24.Name = "GtkHBox1";
            w24.Spacing = 2;
            // Container child GtkHBox1.Gtk.Container+ContainerChild
            Gtk.Image w25 = new Gtk.Image();
            w25.Name = "image2";
            w25.Pixbuf = Gtk.IconTheme.Default.LoadIcon("gtk-cancel", 16, 0);
            w24.Add(w25);
            // Container child GtkHBox1.Gtk.Container+ContainerChild
            Gtk.Label w27 = new Gtk.Label();
            w27.Name = "GtkLabel1";
            w27.LabelProp = Mono.Unix.Catalog.GetString("Cancel");
            w27.UseUnderline = true;
            w24.Add(w27);
            w23.Add(w24);
            this.button8.Add(w23);
            this.AddActionWidget(this.button8, -6);
            Gtk.ButtonBox.ButtonBoxChild w31 = ((Gtk.ButtonBox.ButtonBoxChild)(w13[this.button8]));
            w31.Position = 1;
            w31.Expand = false;
            w31.Fill = false;
            if (this.Child != null)
            {
                this.Child.ShowAll();
            }
            this.DefaultWidth = 476;
            this.DefaultHeight = 137;
            this.Show();
            this.Response += new Gtk.ResponseHandler(this.OnResponse);
        }
    }
}