using Cosmiria;

namespace Towermap;

public static class Styles
{
    public const int IconFont = 1;
    public const int MenuBarHeight = 28;
    public const int MenuBarWidth = 220;

    public const uint Black = 0x000000FF;
    public const uint Surface = 0x0B0B0BFF;
    public const uint Raised = 0x161616FF;
    public const uint Hover = 0x262626FF;
    public const uint Line = 0x2C2C2CFF;
    public const uint LineHover = 0x505050FF;
    public const float Radius = 2;

    private static Style S => default;

    public static readonly Style IconText = S.Font(IconFont).TextSizeOf(14);
    public static readonly Style Row = S.FlexRow().ItemsCenter().Gap(6);
    public static readonly Style Column = S.FlexCol().Gap(6);
    public static readonly Style Muted = S.TextColorOf(Tailwind.Neutral400).TextSizeOf(13);
    public static readonly Style Field = S.Grow().W(0).MinW(60);
    public static readonly Style Small = S.Px(8).Py(3);
    public static readonly Style Accent = S.Bg(0xFF33CCFF).BorderColorOf(0xFF66DDFF);
    public static readonly Style Panel = S.FlexCol().Gap(4).Padding(4).Bg(Black).Border(1, Line).Rounded(Radius);

    public static void Apply(Cosmiria.Theme theme)
    {
        theme.Button = theme.Button.Px(10).Py(4).Bg(Raised).Border(1, Line).Rounded(Radius);
        theme.ButtonHover = theme.ButtonHover.Bg(Hover);
        theme.ButtonActive = theme.ButtonActive.Bg(Black);
        theme.Checkbox = theme.Checkbox.Rounded(Radius).BorderColorOf(Tailwind.Neutral500);
        theme.Radio = theme.Radio.BorderColorOf(Tailwind.Neutral500);

        theme.TextInput = theme.TextInput.Px(6).Py(3).Bg(Black).Border(1, Line).Rounded(Radius);
        theme.TextArea = theme.TextArea.Bg(Black).Border(1, Line).Rounded(Radius);
        theme.TextInputHover = theme.TextInputHover.BorderColorOf(LineHover);
        theme.Placeholder = Tailwind.Neutral600;

        theme.ComboBox = theme.ComboBox.W(Length.Auto).Grow().MinW(80);
        theme.ComboField = theme.ComboField.Px(6).Py(3).Bg(Black).Border(1, Line).Rounded(Radius);
        theme.ComboFieldHover = theme.ComboFieldHover.BorderColorOf(LineHover);
        theme.ComboPopup = theme.ComboPopup.Bg(Surface).Border(1, Line).Rounded(Radius);
        theme.ComboOption = theme.ComboOption.Rounded(Radius);
        theme.ComboOptionHover = theme.ComboOptionHover.Bg(Hover);
        theme.NumberInput = theme.NumberInput.W(Length.Auto);

        theme.TabBar = theme.TabBar.Bg(Surface).Rounded(Radius);
        theme.Tab = theme.Tab.Rounded(Radius).TextColorOf(Tailwind.Neutral400);
        theme.TabHover = theme.TabHover.Bg(Raised);
        theme.TabSelected = theme.TabSelected.Bg(Hover);

        theme.SplitHandle = theme.SplitHandle.Bg(Line);
        theme.DockGroup = theme.DockGroup.Bg(Black);
        theme.DockTabBar = theme.DockTabBar.Bg(Surface);
        theme.DockContent = theme.DockContent.Gap(6).Padding(6);
        theme.DockFloat = theme.DockFloat.Bg(Black).Border(1, Line).Rounded(Radius);
        theme.DockTabCloseHover = theme.DockTabCloseHover.Bg(Hover);

        theme.MenuBar = theme.MenuBar.H(MenuBarHeight).Bg(Surface);
        theme.MenuTitle = theme.MenuTitle.Rounded(Radius);
        theme.MenuTitleHover = theme.MenuTitleHover.Bg(Hover);
        theme.MenuTitleOpen = theme.MenuTitleOpen.Bg(Hover);
        theme.MenuPopup = theme.MenuPopup.Bg(Surface).Border(1, Line).Rounded(Radius);
        theme.MenuItem = theme.MenuItem.Rounded(Radius);
        theme.MenuShortcut = theme.MenuShortcut.TextColorOf(Tailwind.Neutral500);
        theme.MenuSeparator = theme.MenuSeparator.Bg(Line);

        theme.ModalBox = theme.ModalBox.Bg(Surface).Border(1, Line).Rounded(Radius);
        theme.ModalBody = theme.ModalBody.Gap(8);
        theme.ModalClose = theme.ModalClose.Rounded(Radius);
        theme.ModalCloseHover = theme.ModalCloseHover.Bg(Hover);

        theme.TreeRow = theme.TreeRow.Rounded(Radius);
        theme.TreeRowHover = theme.TreeRowHover.Bg(Hover);
        theme.Selectable = theme.Selectable.Rounded(Radius);
        theme.SelectableHover = theme.SelectableHover.Bg(Hover);
        theme.CollapsibleHeader = theme.CollapsibleHeader.Rounded(Radius);
        theme.CollapsibleHeaderHover = theme.CollapsibleHeaderHover.Bg(Hover);
        theme.SeparatorLabel = theme.SeparatorLabel.TextColorOf(Tailwind.Neutral400);
        theme.SeparatorLine = theme.SeparatorLine.Bg(Line);
        theme.Label = theme.Label.TextColorOf(Tailwind.Neutral300);

        theme.TooltipBackground = Black;
        theme.TooltipBorder = Line;
        theme.TooltipText = Tailwind.Neutral100;
    }

    public static Node Icon(this UIStack ui, string icon) => ui.Text(icon).Styled(IconText);
}
