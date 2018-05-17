namespace BlendedAdmin.Js
{
    public class JsonView : View
    {
        public JsonView()
        {

        }

        public JsonView(object json)
        {
            this["json"] = json;
        }
    }
}
