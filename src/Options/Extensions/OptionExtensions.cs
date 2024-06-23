using System;
using System.Linq;
using VentLib.Options.UI;
using VentLib.Options.Enum;
using System.Threading.Tasks;
using System.Collections.Generic;
using VentLib.Options.UI.Options;

namespace VentLib.Options.Extensions;
public static class OptionExtensions
{
    public static List<CategoryHeaderMasked> categoryHeaders = new();
    public static void SetHeader(this CategoryHeaderMasked category, string header, int maskLayer)
    {
        // this function should only be ran ONCE. So this is a nice way to filter out the headers.
        categoryHeaders.Add(category);
        category.Title.text = header;
		category.Background.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
		if (category.Divider != null)
		{
			category.Divider.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
		}
		category.Title.fontMaterial.SetFloat("_StencilComp", 3f);
		category.Title.fontMaterial.SetFloat("_Stencil", (float)maskLayer);
    }

    public static OptionBehaviour GetBehaviour(this GameOption option)
    {
        switch (option.OptionType) 
        {
            case OptionType.String:
                return (option as TextOption).Behaviour.Get();
            case OptionType.Bool:
                return (option as BoolOption).Behaviour.Get();
            case OptionType.Int:
            case OptionType.Float:
                return (option as FloatOption).Behaviour.Get();
            case OptionType.Player:
                return (option as UndefinedOption).Behaviour.Get();
            // default:
            //     return (option as UndefinedOption).Behaviour.Get();
        }
        return null;
    }
    public static T? GetBehaviour<T>(this GameOption option) where T : GameOption
    {
        return option.GetBehaviour() as T;
    }

    public static bool CanOverride(this OptionType type)
    {
        return type == OptionType.Undefined;
    }
}