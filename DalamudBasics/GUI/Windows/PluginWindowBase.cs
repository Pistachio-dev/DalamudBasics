using Dalamud.Interface.Windowing;
using DalamudBasics.Logging;
using ImGuiNET;
using System;

namespace DalamudBasics.GUI.Windows
{
    public abstract class PluginWindowBase : Window
    {
        protected readonly ILogService logService;

        protected PluginWindowBase(ILogService logService, string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
        {
            this.logService = logService;
        }

        protected void DrawActionButton(Action gameAction, string buttonLabel)
        {
            if (ImGui.Button(buttonLabel))
            {
                gameAction();
            }
        }

        protected void DrawTooltip(string text)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(text);
            }
        }

        protected void DrawWithinDisableBlock(bool conditionToBeEnabled, Action drawAction)
        {
            ImGui.BeginGroup();
            if (!conditionToBeEnabled) { ImGui.BeginDisabled(); }
            drawAction();
            if (!conditionToBeEnabled) { ImGui.EndDisabled(); }
            ImGui.EndGroup();
        }

        public override sealed void Draw()
        {
            try
            {
                SafeDraw();
            }
            catch (Exception ex)
            {
                logService.Error(ex, "Error on draw function of window " + this.WindowName);
                throw;
            }
        }

        protected abstract void SafeDraw();
    }
}
