using AForge;
using BioGTK;
using Gdk;
using Gtk;
using static BioGTK.Plugin;
namespace BatchPlugin
{
    public class BatchPlugin : BioGTK.Plugin.IPlugin
    {
        public string Name => "Batch-Processing";
        public string MenuPath => "Tools/" + Name + ".dll";
        public bool ContextMenu => false;
        private MainWindow main;
        public void Execute(string[] args)
        {
            main = MainWindow.Create();
            main.Show();
        }
        public void KeyUpEvent(object o, KeyPressEventArgs e)
        {
            
        }
        public void KeyDownEvent(object o, KeyPressEventArgs e)
        {

        }
        public void ScrollEvent(object o, ScrollEventArgs args)
        {

        }
        public void Drawn(object o, DrawnArgs e)
        {

        }
        public void MouseMove(object o, PointD e, MotionNotifyEventArgs buts)
        {

        }
        public void MouseUp(object o, PointD e, ButtonReleaseEventArgs buts)
        {

        }
        public void MouseDown(object o, PointD e, ButtonPressEventArgs buts)
        {

        }
        
        public class MainWindow : Gtk.Window
        {
            #region Properties
            private Builder _builder;
#pragma warning disable 649
            [Builder.Object]
            private Gtk.TextView inputTextBox;
            [Builder.Object]
            private Gtk.TextView outputTextBox;
            [Builder.Object]
            private Gtk.ComboBox comBox;
            [Builder.Object]
            private Gtk.Button runBut;
            [Builder.Object]
            private Gtk.Button outputBut;
            [Builder.Object]
            private Gtk.Button inputBut;
#pragma warning restore 649
            #endregion

            #region Constructors / Destructors

            /// It creates a new instance of the About class.
            /// 
            /// @return A new instance of the About class.
            public static MainWindow Create()
            {
                Builder builder = new Builder(null, "BatchPlugin.MainWindow.glade", null);
                return new MainWindow(builder, builder.GetObject("mainWindow").Handle);
            }

            /* It's the constructor of the class. */
            protected MainWindow(Builder builder, IntPtr handle) : base(handle)
            {
                _builder = builder;
                builder.Autoconnect(this);
                App.ApplyStyles(this);
                inputBut.ButtonPressEvent += InputBut_ButtonPressEvent;
                outputBut.ButtonPressEvent += OutputBut_ButtonPressEvent;
                runBut.ButtonPressEvent += RunBut_ButtonPressEvent;
                var states = new ListStore(typeof(string));
                foreach (ImageJ.Macro.Command item in ImageJ.Macros)
                {
                    //We add button states to buttonState box.
                    states.AppendValues(item.Name);
                }
                foreach (var item in Plugin.Plugins)
                {
                    //We add button states to buttonState box.
                    states.AppendValues(item.Value.Name);
                }
                foreach (var item in BioGTK.Scripting.scripts)
                {
                    //We add button states to buttonState box.
                    states.AppendValues(item.Value.name);
                }
                // Set the model for the ComboBox
                comBox.Model = states;
                // Set the text column to display
                var renderer = new CellRendererText();
                comBox.PackStart(renderer, false);
                comBox.AddAttribute(renderer, "text", 0);
            }

            private void RunBut_ButtonPressEvent(object o, ButtonPressEventArgs args)
            {
                string input = inputTextBox.Buffer.Text;
                foreach (var file in Directory.GetFiles(input))
                {
                    BioImage b = BioImage.OpenFile(file);
                    if(comBox.ActiveId.EndsWith(".dll"))
                    {
                        foreach (var item in Plugin.Plugins)
                        {
                            if (item.Value.Name == comBox.ActiveId)
                                item.Value.Execute(new string[] { file });
                        }
                    }
                    else
                    if (comBox.ActiveId.EndsWith(".cs"))
                    {
                        foreach (var item in BioGTK.Scripting.scripts)
                        {
                            if (item.Value.name == comBox.ActiveId)
                                item.Value.Run();
                        }
                    }
                    else
                    {
                        foreach (ImageJ.Macro.Command item in ImageJ.Macros)
                        {
                            if (item.Name == comBox.ActiveId)
                                ImageJ.RunOnImage(file);
                        }
                    }
                    string s = System.IO.Path.ChangeExtension(file, ".ome.tif");
                    string ot = outputTextBox.Buffer.Text + "/" + System.IO.Path.GetFileName(s);
                    BioImage.SaveOME(b, s);
                }
                
            }

            private void OutputBut_ButtonPressEvent(object o, ButtonPressEventArgs args)
            {
                Gtk.FileChooserDialog filechooser =
                new Gtk.FileChooserDialog("Choose Output Folder",
                this,
                FileChooserAction.SelectFolder,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept);
                if (filechooser.Run() != (int)ResponseType.Accept)
                    return;
                outputTextBox.Buffer.Text = filechooser.CurrentFolder;
                filechooser.Hide();
            }

            private void InputBut_ButtonPressEvent(object o, ButtonPressEventArgs args)
            {
                Gtk.FileChooserDialog filechooser =
                new Gtk.FileChooserDialog("Choose Input Folder",
                this,
                FileChooserAction.SelectFolder,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept);
                if (filechooser.Run() != (int)ResponseType.Accept)
                    return;
                inputTextBox.Buffer.Text = filechooser.CurrentFolder;
                filechooser.Hide();
            }

            #endregion

        }
    }
}
