using Sirenix.OdinInspector.Editor;

#nullable enable

namespace UI.GP.Editor
{
    public class GpProgressDialog : OdinEditorWindow
    {
        public object? thing;

        private void OnInspectorUpdate()
        {
            // Called ~10 times per second
            this.Repaint();
        }

        public static GpProgressDialog CreateAndGo(object thing)
        {
            var window = CreateInstance<GpProgressDialog>();
            window.thing = thing;
            window.Show();
            return window;
        }

        protected override object GetTarget()
        {
            return this.thing ?? "5";
        }
    }
}