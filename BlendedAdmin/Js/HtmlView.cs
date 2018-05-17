namespace BlendedAdmin.Js
{
    public class HtmlView : View
    {
        public HtmlView()
        {
        }

        public HtmlView(object html)
        {
            this["html"] = html;
        }
    }
}
