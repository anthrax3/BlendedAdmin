namespace BlendedAdmin.Js
{
    public class DataTablesView : View
    {
        public DataTablesView()
        {
        }

        public DataTablesView(object options)
        {
            this["options"] = options;
        }
    }
}
