using System.Collections.Generic;

namespace BauphysikToolWPF.Models.UI
{
    interface IPropertyBag
    {
        // TODO: add notify property changed
        IEnumerable<IPropertyItem> PropertyBag { get; }
    }
}
