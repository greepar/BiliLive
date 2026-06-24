using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Material3.UI.Controls;

/// <summary>
/// Material 3 styled card container. Renders a rounded surface with one of the
/// three M3 card variants: Elevated, Filled, or Outlined.
/// Selected via the <c>Variant</c> property and reflected as a pseudo-class
/// (:elevated, :filled, :outlined) so themes can react in axaml.
/// </summary>
public class Card : ContentControl
{
    public static readonly StyledProperty<CardVariant> VariantProperty =
        AvaloniaProperty.Register<Card, CardVariant>(nameof(Variant), CardVariant.Elevated);

    public CardVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    static Card()
    {
        VariantProperty.Changed.AddClassHandler<Card>((c, _) => c.UpdatePseudoClasses());
    }

    public Card()
    {
        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":elevated", Variant == CardVariant.Elevated);
        PseudoClasses.Set(":filled", Variant == CardVariant.Filled);
        PseudoClasses.Set(":outlined", Variant == CardVariant.Outlined);
    }
}

public enum CardVariant
{
    Elevated,
    Filled,
    Outlined
}
