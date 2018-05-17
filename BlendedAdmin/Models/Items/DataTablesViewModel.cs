using System.Collections.Generic;
using BlendedJS;
using BlendedAdmin.Js;
using System.Linq;

namespace BlendedAdmin.Models.Items
{
    public class DataTablesViewModel
    {
        public string Title { get; set; }
        public string Options { get; set; }
    }

    public class DataTablesViewModelAssembler
    {
        public DataTablesViewModel ToModel(DataTablesView dataTables)
        {
            DataTablesViewModel model = new DataTablesViewModel();
            var options = dataTables.GetValueOrDefault("options");
            EnsureColumns(options);
            model.Options = options.ToJsonOrDefault();
            return model;
        }

        public void EnsureColumns(object options)
        {
            if (options == null)
                return;

            var columns = options.GetProperty("columns");
            if (columns == null)
            {
                var data = options.GetProperty("data");
                if (data is System.Collections.IEnumerable)
                {
                    var items = ((System.Collections.IEnumerable)data).GetEnumerator();
                    if (items.MoveNext())
                    {
                        if (items.Current is IDictionary<string, object> firstItem)
                        {
                            var generatedColumns = firstItem.Keys.Select(x =>
                            {
                                JsObject column = new JsObject();
                                column["data"] = x;
                                column["title"] = x;
                                return column;
                            });
                            options.SetProperty("columns", generatedColumns);
                        }
                    }
                }
            }
            
        }
    }
}
