using System.Collections.Generic;

namespace BauphysikToolWPF.Models.UI
{
    interface IPropertyClass
    {
        // TODO: add notify property changed
        IEnumerable<IPropertyItem> PropertyBag { get; }
    }
}
