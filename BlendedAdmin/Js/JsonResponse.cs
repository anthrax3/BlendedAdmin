using BlendedJS;

namespace BlendedAdmin.Js
{
    public class JsonResponse : JsObject
    {
        public JsonResponse()
        {

        }

        public JsonResponse(object json)
        {
            this["json"] = json;
        }
    }
}
