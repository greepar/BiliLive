using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Material3.UI;

public partial class Material3 : Style
{
    public Material3()
    {
        AvaloniaXamlLoader.Load(this);
    }
}