using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;


namespace BiliLive.Resources.Controls;

public class MdButton : Border
{
    public static readonly StyledProperty<IBrush> RippleColorProperty =
        AvaloniaProperty.Register<RippleButton, IBrush>(nameof(RippleColor), 
            Brushes.White);

    public static readonly StyledProperty<double> RippleDurationProperty =
        AvaloniaProperty.Register<RippleButton, double>(nameof(RippleDuration), 0.8);
    
    public static readonly StyledProperty<bool> IsRippleEnabledProperty =
        AvaloniaProperty.Register<RippleButton, bool>(nameof(IsRippleEnabled), false);
    

    public static readonly StyledProperty<string> HoverShadowProperty =
        AvaloniaProperty.Register<RippleButton, string>(nameof(HoverShadow), "0 0 0 0 #000000");
    
    
    public IBrush RippleColor
    {
        get => GetValue(RippleColorProperty);
        set => SetValue(RippleColorProperty, value);
    }

    public double RippleDuration
    {
        get => GetValue(RippleDurationProperty);
        set => SetValue(RippleDurationProperty, value);
    }
    
    public bool IsRippleEnabled
    {
        get => GetValue(IsRippleEnabledProperty);
        set => SetValue(IsRippleEnabledProperty, value);
    }

    public string HoverShadow
    {
        get => GetValue(HoverShadowProperty);
        set => SetValue(HoverShadowProperty, value);
    }
    
    public MdButton()
    {
        PointerEntered += (_, _) => ApplyShadow(BoxShadows.Parse(HoverShadow));
        PointerExited += (_, _) => ApplyShadow(BoxShadows.Parse("0 0 0 0 #000000"));
    }

    private void ApplyShadow(BoxShadows shadow)
    {
        
    }
}