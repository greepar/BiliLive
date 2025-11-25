using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace Material3.UI;

public partial class Material3Res : ResourceInclude
{
    public Material3Res(Uri? baseUri) : base(baseUri)
    {
    }

    public Material3Res(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}