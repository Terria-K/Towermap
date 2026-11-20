using Cosmiria;

namespace Towermap;

public struct App : IComponent
{
    public TowermapGame Game;

    public Node Render(UIStack ui, EventStack ev)
    {
        if (Game.Editor != null)
        {
            ui.State<EditorView>(0, out _).Editor = Game.Editor;
            return ui.Mount<EditorView>(0);
        }

        ref var prompt = ref ui.State<PromptView>(0, out _);
        if (prompt.Game == null)
        {
            prompt = PromptView.Create(Game, Game.SaveState);
        }
        return ui.Mount<PromptView>(0);
    }
}
